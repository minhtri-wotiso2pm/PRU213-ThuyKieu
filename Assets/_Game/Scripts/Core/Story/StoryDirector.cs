using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Dialogue;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using ThuyKieu.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThuyKieu.Core.Story
{
    /// <summary>Optional chapter-specific text for the completion screen.</summary>
    public interface IChapterSummary { IEnumerable<string> SummaryLines(Ink.Runtime.Story story); }

    /// <summary>
    /// Generic chapter flow for Ink-driven chapters (2 onwards):
    /// dialogue → explore (world resolves #explore choices) → blocking sequences → chapter complete.
    /// Presentation tags are routed to camera, actors, HUD, audio and screen effects.
    /// </summary>
    public class StoryDirector : MonoBehaviour
    {
        public struct ExploreOption { public int Index; public string Id; public string Text; }

        private struct Checkpoint
        {
            public string Ink;
            public Vector3 PlayerPosition;
            public Quaternion PlayerRotation;
            public int ClueCount;
        }

        [Header("Chapter")]
        [SerializeField] private TextAsset _compiledInk;
        [SerializeField] private int _chapterNumber = 2;
        [SerializeField] private string _chapterTitle = "Trên đường theo Mã Giám Sinh";
        [SerializeField] private string _entryKnot = "chapter_2";
        [SerializeField] private string _nextScene = "Chapter03_TuBaHouse";

        [Header("Systems")]
        [SerializeField] private DialogueManager _dialogue;
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private PlayerInteraction _interaction;
        [SerializeField] private ThirdPersonCameraController _camera;
        [SerializeField] private StoryHUD _hud;
        [SerializeField] private DialogueCameraRig _shots;
        [SerializeField] private ScreenFx _fx;
        [SerializeField] private StoryAudio _audio;

        private readonly List<ExploreOption> _exploreOptions = new List<ExploreOption>();
        private readonly List<string> _clues = new List<string>();
        private Dictionary<string, StorySequence> _sequences;
        private StoryActor[] _actors;
        private StoryActor _playerActor;
        private Checkpoint _checkpoint;
        private string _pendingSequence;
        private bool _exploreRequested, _chapterEnded, _busy;

        public Ink.Runtime.Story Story { get; private set; }
        public int ChapterNumber => _chapterNumber;
        public string ChapterTitle => _chapterTitle;
        public bool InExplore { get; private set; }
        public bool IsBusy => _busy;
        public bool DialogueActive => _dialogue != null && _dialogue.IsDialogueActive;
        public bool ChapterComplete { get; private set; }
        public string CurrentSequence { get; private set; }
        public IReadOnlyList<ExploreOption> ExploreOptions => _exploreOptions;
        public IReadOnlyList<string> Clues => _clues;
        public PlayerMovement Player => _player;
        public ThirdPersonCameraController CameraController => _camera;
        public StoryHUD Hud => _hud;
        public ScreenFx Fx => _fx;
        public StoryAudio Audio => _audio;
        public DialogueManager Dialogue => _dialogue;
        /// <summary>QA autopilot drives the chapter; sequences may shorten waits.</summary>
        public bool Autopilot { get; set; }

        public event Action<string, string> TagReceived;
        public event Action<string> SceneChanged;
        public event Action<bool> ExploreChanged;
        public event Action CheckpointRestored;
        public event Action<string> ClueAdded;

        private void Awake()
        {
            _actors = FindObjectsByType<StoryActor>(FindObjectsInactive.Include);
            _playerActor = _player != null ? _player.GetComponent<StoryActor>() : null;
            _sequences = new Dictionary<string, StorySequence>();
            foreach (var sequence in FindObjectsByType<StorySequence>(FindObjectsInactive.Include))
                if (!string.IsNullOrEmpty(sequence.Id)) _sequences[sequence.Id] = sequence;
        }

        private IEnumerator Start()
        {
            Story = new Ink.Runtime.Story(_compiledInk.text);
            if (InkStatePersistence.Instance != null && InkStatePersistence.Instance.TryGetStateFor(_chapterNumber, out string saved))
            {
                Story.state.LoadJson(saved);
                _clues.AddRange(InkStatePersistence.Instance.Clues);
            }
            else
                Story.ChoosePathString(_entryKnot);

            _dialogue.SetInkStory(Story);
            _dialogue.InkStopBeforeLine = StopBeforeLine;
            _dialogue.InkBlockAfterLine = BlockAfterLine;
            _dialogue.InkTagsChanged += RouteTags;
            _dialogue.InkExploreReached += OnExploreReached;
            _dialogue.DialogueEnded += OnDialogueEnded;
            _dialogue.DialogueStarted += OnDialogueStarted;

            SetFreeControl(false);
            yield return null; // Let views subscribe to the dialogue events first.
            if (_hud != null) yield return _hud.ChapterCard(_chapterNumber, _chapterTitle);
            Continue();
        }

        private void OnDestroy()
        {
            if (_dialogue == null) return;
            _dialogue.InkStopBeforeLine = null;
            _dialogue.InkBlockAfterLine = null;
            _dialogue.InkTagsChanged -= RouteTags;
            _dialogue.InkExploreReached -= OnExploreReached;
            _dialogue.DialogueEnded -= OnDialogueEnded;
            _dialogue.DialogueStarted -= OnDialogueStarted;
        }

        // ---------------------------------------------------------------- flow

        /// <summary>Resumes the story: explore mode if the world must choose, otherwise dialogue.</summary>
        public void Continue()
        {
            if (ChapterComplete || _busy) return;
            if (_dialogue.InkAtExploreChoices) { EnterExplore(); return; }
            _dialogue.ResumeInk();
        }

        private bool StopBeforeLine(string[] tags)
        {
            if (!tags.Any(t => t.Trim() == "chapter:" + (_chapterNumber + 1))) return false;
            _chapterEnded = true;
            return true;
        }

        private bool BlockAfterLine(string[] tags)
        {
            foreach (string raw in tags)
            {
                string tag = raw.Trim();
                if (!tag.StartsWith("gameplay:")) continue;
                string id = tag.Substring(9).Trim();
                if (_sequences.ContainsKey(id)) { _pendingSequence = id; return true; }
                Debug.LogWarning("[Story] No sequence registered for " + tag, this);
            }
            return false;
        }

        private void OnExploreReached() => _exploreRequested = true;

        private void OnDialogueStarted(DialogueData _)
        {
            SetFreeControl(false);
            if (_hud != null) { _hud.SetDialogueMode(true); _hud.ClearBark(); }
        }

        private void OnDialogueEnded()
        {
            ResetActorTalking();
            if (_shots != null) _shots.Clear();
            if (_hud != null) _hud.SetDialogueMode(false);
            if (_chapterEnded) { _chapterEnded = false; StartCoroutine(CompleteChapter()); return; }
            if (_pendingSequence != null)
            {
                string id = _pendingSequence;
                _pendingSequence = null;
                StartCoroutine(RunSequence(id));
                return;
            }
            if (_exploreRequested) { _exploreRequested = false; EnterExplore(); }
        }

        private IEnumerator RunSequence(string id)
        {
            _busy = true;
            CurrentSequence = id;
            if (_hud != null) _hud.ClearBark();
            yield return _sequences[id].Run(this);
            CurrentSequence = null;
            _busy = false;
            Continue();
        }

        private void EnterExplore()
        {
            _exploreOptions.Clear();
            var choices = Story.currentChoices;
            for (int i = 0; i < choices.Count; i++)
            {
                string id = choices[i].tags.First(t => t.StartsWith("explore:")).Substring(8).Trim();
                _exploreOptions.Add(new ExploreOption { Index = i, Id = id, Text = choices[i].text });
            }
            SaveCheckpoint();
            InExplore = true;
            SetFreeControl(true);
            if (_hud != null) _hud.SetExploreOptions(_exploreOptions.Select(o => o.Text).ToList());
            ExploreChanged?.Invoke(true);
        }

        public bool IsExploreAvailable(string id)
        {
            if (!InExplore || _busy) return false;
            foreach (var option in _exploreOptions) if (option.Id == id) return true;
            return false;
        }

        /// <summary>Called by world interactions: picks the matching #explore choice and resumes the story.</summary>
        public bool ChooseExplore(string id, Transform focus = null)
        {
            if (!IsExploreAvailable(id)) return false;
            var option = _exploreOptions.First(o => o.Id == id);
            InExplore = false;
            _exploreOptions.Clear();
            if (_hud != null) _hud.SetExploreOptions(null);
            ExploreChanged?.Invoke(false);
            SetFreeControl(false);
            if (focus != null && _playerActor != null) _playerActor.FaceInstant(focus.position);
            Story.ChooseChoiceIndex(option.Index);
            _dialogue.ResumeInk();
            return true;
        }

        private IEnumerator CompleteChapter()
        {
            ChapterComplete = true;
            SetFreeControl(false);
            if (InkStatePersistence.Instance != null)
            {
                InkStatePersistence.Instance.Save(Story, _chapterNumber + 1);
                InkStatePersistence.Instance.Clues.Clear();
                InkStatePersistence.Instance.Clues.AddRange(_clues);
            }
            var summary = GetComponent<IChapterSummary>();
            var lines = summary != null ? summary.SummaryLines(Story).ToList() : new List<string>();
            if (_audio != null) _audio.PlaySfx("chapter_complete");
            bool proceed = false;
            if (_hud != null) _hud.ShowChapterComplete(_chapterNumber, _chapterTitle, lines, () => proceed = true);
            else proceed = true;
            if (_camera != null) _camera.SetCursorLocked(false);
            while (!proceed && !Autopilot) yield return null;
            if (Autopilot) yield break;
            if (_hud != null) yield return _hud.Fade(1f, 0.8f);
            if (Application.CanStreamedLevelBeLoaded(_nextScene)) SceneManager.LoadScene(_nextScene);
            else Debug.LogWarning("[Story] Next scene not in build settings: " + _nextScene, this);
        }

        // ---------------------------------------------------------------- checkpoints & failure

        public void SaveCheckpoint()
        {
            _checkpoint = new Checkpoint
            {
                Ink = Story.state.ToJson(),
                PlayerPosition = _player.transform.position,
                PlayerRotation = _player.transform.rotation,
                ClueCount = _clues.Count
            };
        }

        /// <summary>Shows a failure card, restores the last checkpoint and returns to exploring.</summary>
        public void Fail(string headline, string detail)
        {
            if (_busy || ChapterComplete) return;
            StartCoroutine(FailRoutine(headline, detail));
        }

        private IEnumerator FailRoutine(string headline, string detail)
        {
            _busy = true;
            InExplore = false;
            ExploreChanged?.Invoke(false);
            SetFreeControl(false);
            if (_audio != null) _audio.PlaySfx("fail");
            if (_fx != null) _fx.Shake(0.35f, 0.5f);
            if (_hud != null) yield return _hud.FailCard(headline, detail, Autopilot ? 0.2f : 2.4f);
            Story.state.LoadJson(_checkpoint.Ink);
            if (_clues.Count > _checkpoint.ClueCount) _clues.RemoveRange(_checkpoint.ClueCount, _clues.Count - _checkpoint.ClueCount);
            _player.Teleport(_checkpoint.PlayerPosition, _checkpoint.PlayerRotation);
            if (_camera != null) _camera.SnapBehindTarget(_checkpoint.PlayerRotation.eulerAngles.y);
            CheckpointRestored?.Invoke();
            if (_hud != null) yield return _hud.Fade(0f, 0.6f);
            _busy = false;
            Continue();
        }

        // ---------------------------------------------------------------- control & variables

        public void SetFreeControl(bool free)
        {
            if (_player != null) _player.ControlLocked = !free;
            if (_interaction != null) _interaction.ControlLocked = !free;
            if (_camera != null) { _camera.ControlLocked = !free; _camera.SetCursorLocked(free); }
        }

        public int GetInt(string variable) => Story.variablesState[variable] is int value ? value : 0;
        public bool GetBool(string variable) => Story.variablesState[variable] is bool value ? value : GetInt(variable) != 0;
        public void SetInt(string variable, int value) => Story.variablesState[variable] = value;

        public StoryActor FindActor(string speakerId)
        {
            foreach (var actor in _actors) if (actor != null && actor.SpeakerId == speakerId) return actor;
            return null;
        }

        public void AddClue(string id)
        {
            if (_clues.Contains(id)) return;
            _clues.Add(id);
            if (_hud != null && ClueCatalog.TryGet(id, out string title, out string body)) _hud.ShowToast("MANH MỐI MỚI", title);
            if (_audio != null) _audio.PlaySfx("clue");
            ClueAdded?.Invoke(id);
        }

        // ---------------------------------------------------------------- presentation tags

        private void RouteTags(string[] tags)
        {
            string speaker = _dialogue.CurrentSpeakerId;
            string emotion = string.Empty;
            bool hasCamera = false;
            foreach (string raw in tags)
            {
                int colon = raw.IndexOf(':');
                if (colon < 0) continue;
                string key = raw.Substring(0, colon).Trim();
                string value = raw.Substring(colon + 1).Trim();
                switch (key)
                {
                    case "emotion": emotion = value; break;
                    case "camera": hasCamera = true; if (_shots != null) _shots.Shot(value); break;
                    case "objective": if (_hud != null) _hud.SetObjective(value); break;
                    case "clue": AddClue(value); break;
                    case "music": if (_audio != null) _audio.PlayMusic(value); break;
                    case "sfx": if (_audio != null) _audio.PlaySfx(value); break;
                    case "fx": if (_fx != null) _fx.SetMood(value); break;
                    case "scene": SceneChanged?.Invoke(value); break;
                }
                TagReceived?.Invoke(key, value);
            }
            if (!hasCamera && _shots != null) _shots.Auto(speaker);

            var speakerActor = FindActor(speaker);
            int gesture = Gesture(emotion);
            foreach (var actor in _actors)
            {
                if (actor == null || !actor.isActiveAndEnabled) continue;
                actor.SetTalking(actor == speakerActor, gesture);
                if (actor.IsMoving) continue;
                if (actor == speakerActor) { if (_playerActor != null && actor != _playerActor) actor.FaceTowards(_playerActor.transform.position); }
                else if (speakerActor != null && Vector3.Distance(actor.transform.position, speakerActor.transform.position) < 6f)
                    actor.FaceTowards(speakerActor.transform.position);
            }
        }

        private void ResetActorTalking()
        {
            foreach (var actor in _actors) if (actor != null) { actor.SetTalking(false, 0); actor.FaceTowards(null); }
        }

        public static int Gesture(string emotion)
        {
            switch (emotion)
            {
                case "crying": return 1;
                case "irritated_hidden": case "annoyed": case "cold": case "angry": case "mocking": case "rage": return 2;
                case "satisfied": case "smug": case "amused": case "warm_fake": return 3;
                case "defensive": case "caught_off_guard": case "determined": case "firm": return 4;
                case "thoughtful": case "calm_probe": case "realization": return 5;
                case "suspicious": case "fearful": case "businesslike": return 7;
                default: return 0;
            }
        }
    }
}

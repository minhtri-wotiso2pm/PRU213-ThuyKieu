using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Dialogue;
using ThuyKieu.Gameplay.Sequences;
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
    // Start must load the Ink state before the chapter worlds' Start reads story variables (scene order is not guaranteed).
    [DefaultExecutionOrder(-50)]
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
        /// <summary>Last value of each world-setting tag, replayed when a mid-chapter save is resumed.</summary>
        private static readonly string[] ResumeKeys = { "scene", "fx", "music", "objective" };
        private readonly Dictionary<string, string> _lastTags = new Dictionary<string, string>();
        /// <summary>Tags that set up the world, routed ahead of a montage so its stills show that world.</summary>
        private static readonly string[] WorldKeys = { "scene", "music", "sfx", "fx" };
        private string[] _preRouted;
        private Dictionary<string, StorySequence> _sequences;
        private StoryActor[] _actors;
        private StoryActor _playerActor;
        private StoryActor _lastSpeakerActor;
        private Checkpoint _checkpoint;
        private string _pendingSequence;
        private bool _exploreRequested, _chapterEnded, _busy;

        public Ink.Runtime.Story Story { get; private set; }
        public int ChapterNumber => _chapterNumber;
        public string NextScene => _nextScene;
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
        public DialogueCameraRig Shots => _shots;
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
            // The lục bát exchanges need no scene setup: their UI builds itself, so register them in every chapter.
            foreach (string id in LucBatCatalog.Ids)
                if (!_sequences.ContainsKey(id)) _sequences[id] = LucBatSequence.Create(gameObject, id);
            foreach (var montage in MontageCatalog.All)
                if (!_sequences.ContainsKey(montage.Id)) _sequences[montage.Id] = MontageSequence.Create(gameObject, montage.Id);
        }

        /// <summary>Adds a sequence a chapter world builds at runtime (call from Awake or Start, before the story runs).</summary>
        public void RegisterSequence(StorySequence sequence)
        {
            if (sequence != null && !string.IsNullOrEmpty(sequence.Id)) _sequences[sequence.Id] = sequence;
        }

        private IEnumerator Start()
        {
            Story = new Ink.Runtime.Story(_compiledInk.text);
            MidChapterSave.Data resume = null;
            if (InkStatePersistence.Instance != null && InkStatePersistence.Instance.ResumeMid)
            {
                InkStatePersistence.Instance.ResumeMid = false;
                resume = MidChapterSave.Load(_chapterNumber);
            }
            if (resume != null)
            {
                Story.state.LoadJson(resume.Ink);
                _clues.AddRange(resume.Clues);
            }
            else if (InkStatePersistence.Instance != null && InkStatePersistence.Instance.TryGetStateFor(_chapterNumber, out string saved))
            {
                Story.state.LoadJson(saved);
                _clues.AddRange(InkStatePersistence.Instance.Clues);
            }
            else
            {
                Story.ChoosePathString(_entryKnot);
                PageArchive.ApplyTo(Story); // a fresh run starts holding the pages kept from earlier runs
            }

            _dialogue.SetInkStory(Story);
            TrangSachView.Attach(_dialogue);
            _dialogue.InkStopBeforeLine = StopBeforeLine;
            _dialogue.InkPauseBeforeLine = PauseBeforeLine;
            _dialogue.InkBlockAfterLine = BlockAfterLine;
            _dialogue.InkTagsChanged += RouteTags;
            _dialogue.InkExploreReached += OnExploreReached;
            _dialogue.DialogueEnded += OnDialogueEnded;
            _dialogue.DialogueStarted += OnDialogueStarted;

            SetFreeControl(false);
            yield return null; // Let views subscribe to the dialogue events first.
            if (_hud != null) yield return _hud.ChapterCard(_chapterNumber, _chapterTitle);
            if (resume != null) yield return Resume(resume);
            Continue();
        }

        /// <summary>
        /// Puts the world back as it was at the saved checkpoint: replays the last scene / mood / music / objective tags
        /// (chapter worlds rebuild their set-up from the scene tag), waits for their transitions, then returns Kiều to
        /// where she stood.
        /// </summary>
        private IEnumerator Resume(MidChapterSave.Data save)
        {
            foreach (string tag in save.Tags)
            {
                int colon = tag.IndexOf(':');
                if (colon > 0) _lastTags[tag.Substring(0, colon)] = tag.Substring(colon + 1);
            }
            if (save.Tags.Count > 0) RouteTags(save.Tags.ToArray());
            StoryAutopilot.Note("RESUME mid-save · tags " + string.Join(" ", save.Tags) + " · to " + save.Position.ToString("F2"));
            yield return new WaitForSecondsRealtime(1.6f);
            _player.Teleport(save.Position, Quaternion.Euler(0f, save.Yaw, 0f));
            if (_camera != null) _camera.SnapBehindTarget(save.Yaw);
            if (_hud != null) _hud.ShowToast("TIẾP TỤC", "Từ điểm lưu lúc " + save.SavedAt);
        }

        private void OnDestroy()
        {
            if (_dialogue == null) return;
            _dialogue.InkStopBeforeLine = null;
            _dialogue.InkPauseBeforeLine = null;
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

        /// <summary>
        /// "# montage:&lt;id&gt;" plays its stills before the line appears. The line's world tags (scene, music, sfx, fx)
        /// are routed first so the stills show the staging they start; the line itself follows without routing them again.
        /// </summary>
        private bool PauseBeforeLine(string[] tags)
        {
            foreach (string raw in tags)
            {
                string tag = raw.Trim();
                if (!tag.StartsWith("montage:")) continue;
                string id = MontageSequence.Prefix + tag.Substring(8).Trim();
                if (!_sequences.ContainsKey(id)) { Debug.LogWarning("[Story] No montage registered for " + tag, this); return false; }
                _pendingSequence = id;
                _preRouted = tags;
                foreach (string world in tags)
                {
                    int colon = world.IndexOf(':');
                    if (colon < 0) continue;
                    string key = world.Substring(0, colon).Trim();
                    if (Array.IndexOf(WorldKeys, key) >= 0) RouteTag(key, world.Substring(colon + 1).Trim());
                }
                return true;
            }
            return false;
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
        public bool ChooseExplore(string id, Transform focus = null, Transform standPoint = null)
        {
            if (!IsExploreAvailable(id)) return false;
            var option = _exploreOptions.First(o => o.Id == id);
            InExplore = false;
            _exploreOptions.Clear();
            if (_hud != null) _hud.SetExploreOptions(null);
            ExploreChanged?.Invoke(false);
            SetFreeControl(false);
            if (standPoint != null && _player != null) _player.Teleport(standPoint.position, standPoint.rotation);
            else if (focus != null && _playerActor != null) _playerActor.FaceInstant(focus.position);
            Story.ChooseChoiceIndex(option.Index);
            _dialogue.ResumeInk();
            return true;
        }

        private IEnumerator CompleteChapter()
        {
            ChapterComplete = true;
            MidChapterSave.Clear();
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

        /// <summary>
        /// The end of the whole story (the epilogue has no next chapter): marks the run complete without the
        /// chapter-complete card, so the ending sequence owns the screen.
        /// </summary>
        public void EndStory()
        {
            ChapterComplete = true;
            SetFreeControl(false);
            MidChapterSave.Clear();
            GameProgress.MarkFinished();
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
            if ((Autopilot && !StoryAutopilot.MidSaveTest) || ChapterComplete) return;
            // The same moment is the mid-chapter save: "Continue" from the menu comes back here.
            MidChapterSave.Write(new MidChapterSave.Data
            {
                Chapter = _chapterNumber,
                Ink = _checkpoint.Ink,
                Position = _checkpoint.PlayerPosition,
                Yaw = _checkpoint.PlayerRotation.eulerAngles.y,
                Clues = new List<string>(_clues),
                Tags = ResumeKeys.Where(_lastTags.ContainsKey).Select(k => k + ":" + _lastTags[k]).ToList()
            });
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
        public void SetBool(string variable, bool value) => Story.variablesState[variable] = value;

        /// <summary>Adds an actor created at runtime so speaker tags, close-ups and talking animation reach it.</summary>
        public void RegisterActor(StoryActor actor)
        {
            if (actor != null && Array.IndexOf(_actors, actor) < 0) _actors = _actors.Append(actor).ToArray();
        }

        public StoryActor FindActor(string speakerId)
        {
            // The modern student speaks through Kiều's body (the player) in the classroom scenes.
            if (speakerId == "HocSinh") speakerId = "Kieu";
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
            bool preRouted = ReferenceEquals(tags, _preRouted);
            _preRouted = null;
            foreach (string raw in tags)
            {
                int colon = raw.IndexOf(':');
                if (colon < 0) continue;
                string key = raw.Substring(0, colon).Trim();
                string value = raw.Substring(colon + 1).Trim();
                if (preRouted && Array.IndexOf(WorldKeys, key) >= 0) continue;
                if (key == "emotion") emotion = value;
                if (key == "camera") hasCamera = true;
                RouteTag(key, value);
            }
            if (!hasCamera && _shots != null) _shots.Auto(speaker);

            var speakerActor = FindActor(speaker);
            StoryActor partner = _lastSpeakerActor != null && _lastSpeakerActor != speakerActor
                && _lastSpeakerActor.isActiveAndEnabled
                && speakerActor != null
                && Vector3.Distance(_lastSpeakerActor.transform.position, speakerActor.transform.position) < ConversationRange
                    ? _lastSpeakerActor
                    : _playerActor;
            int gesture = Gesture(emotion);
            foreach (var actor in _actors)
            {
                if (actor == null || !actor.isActiveAndEnabled) continue;
                actor.SetTalking(actor == speakerActor, gesture);
                if (actor.IsMoving) continue;
                // Only people within talking distance turn: a line called out to someone in another room swivels nobody.
                if (actor == speakerActor)
                {
                    if (partner != null && actor != partner && Vector3.Distance(actor.transform.position, partner.transform.position) < ConversationRange)
                        actor.TurnForLine(partner.transform.position);
                }
                else if (speakerActor != null && Vector3.Distance(actor.transform.position, speakerActor.transform.position) < ConversationRange)
                    actor.TurnForLine(speakerActor.transform.position);
            }
            if (speakerActor != null) _lastSpeakerActor = speakerActor;
        }

        private void RouteTag(string key, string value)
        {
            if (Array.IndexOf(ResumeKeys, key) >= 0) _lastTags[key] = value;
            if (key == "scene" || key == "music" || key == "sfx" || key == "gameplay") StoryAutopilot.Note("TAG " + key + ":" + value);
            switch (key)
            {
                case "camera": if (_shots != null) _shots.Shot(value); break;
                case "objective": if (_hud != null) _hud.SetObjective(value); break;
                case "clue": AddClue(value); break;
                case "music": if (_audio != null) _audio.PlayMusic(value); break;
                case "sfx": if (_audio != null) _audio.PlaySfx(value); break;
                case "fx": if (_fx != null) _fx.SetMood(value); break;
                case "scene": SceneChanged?.Invoke(value); break;
            }
            TagReceived?.Invoke(key, value);
        }

        private const float ConversationRange = 6f;

        private void ResetActorTalking()
        {
            _lastSpeakerActor = null;
            foreach (var actor in _actors)
            {
                if (actor == null) continue;
                actor.SetTalking(false, 0);
                // The player keeps the way she turned; everyone else goes back to how they stood before the talk.
                if (actor == _playerActor) actor.FaceTowards(null);
                else actor.EndLineTurn();
            }
        }

        public static int Gesture(string emotion)
        {
            switch (emotion)
            {
                case "crying": return 1;
                case "irritated_hidden": case "annoyed": case "angry": case "mocking": case "rage": return 2;
                case "satisfied": case "smug": case "amused": case "warm_fake": return 3;
                case "defensive": case "caught_off_guard": return 4;
                case "thoughtful": case "realization": case "suspicious": case "fearful": return 5;
                // The shared controller has states 0–6 only. Calm, controlled and businesslike lines use its normal
                // talking state; returning 7 here previously left those speakers in the preceding animation.
                case "pickup": return 6;
                default: return 0;
            }
        }
    }
}

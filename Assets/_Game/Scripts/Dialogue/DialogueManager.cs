using System;
using UnityEngine;

namespace ThuyKieu.Dialogue
{
    /// <summary>
    /// Runs a conversation: walks the lines of a node, hands choices to the view,
    /// follows branches and reports completion. It owns no UI: any view can listen
    /// to the events below, so Developer 6 can replace the view without touching this.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        private Action onConversationComplete;

        /// <summary>Current node, or null when idle.</summary>
        public DialogueData CurrentNode { get; private set; }

        /// <summary>Index of the line being shown inside CurrentNode.</summary>
        public int CurrentLineIndex { get; private set; }

        public Ink.Runtime.Story InkStory { get; private set; }
        private bool _inkActive;
        private string _pendingText;
        private string[] _pendingTags;
        private string _inkSpeaker = "Dẫn chuyện";
        public Func<string[], bool> InkPauseBeforeLine;
        public Func<string[], bool> InkStopBeforeLine;
        /// <summary>Asked when the player advances past a line; true closes the dialogue so a blocking sequence can run.</summary>
        public Func<string[], bool> InkBlockAfterLine;
        public event Action<string[]> InkTagsChanged;
        /// <summary>Raised just before the dialogue closes because every pending choice is an #explore choice.</summary>
        public event Action InkExploreReached;
        public bool IsDialogueActive => _inkActive || CurrentNode != null;
        public string CurrentSpeakerId { get; private set; } = "Narrator";
        public string[] CurrentInkTags { get; private set; } = new string[0];

        /// <summary>True when the story waits on choices that the world, not the dialogue UI, resolves.</summary>
        public bool InkAtExploreChoices
        {
            get
            {
                if (InkStory == null || InkStory.canContinue || InkStory.currentChoices.Count == 0) return false;
                foreach (var choice in InkStory.currentChoices)
                    if (choice.tags == null || !choice.tags.Exists(t => t.StartsWith("explore:"))) return false;
                return true;
            }
        }

        /// <summary>True when the last line of the node is on screen and choices should show.</summary>
        public bool IsAtChoicePoint => _inkActive ? InkStory.currentChoices.Count > 0 && !InkAtExploreChoices : IsDialogueActive
                                       && CurrentNode.HasChoices
                                       && CurrentLineIndex >= CurrentNode.Lines.Count - 1;

        /// <summary>Raised when a conversation opens. Views show themselves here.</summary>
        public event Action<DialogueData> DialogueStarted;

        /// <summary>Speaker name, line text. Raised for every line, including after a branch.</summary>
        public event Action<string, string> LineChanged;

        /// <summary>Raised when choices must be displayed (empty array = hide choices).</summary>
        public event Action<DialogueChoice[]> ChoicesChanged;

        /// <summary>Raised when the conversation closes, after the completion callback.</summary>
        public event Action DialogueEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Opens a conversation. onComplete fires once, when the whole conversation
        /// (including branches) is over. This is the hook NPCs use to start quests.
        /// </summary>
        public bool StartDialogue(DialogueData node, Action onComplete = null)
        {
            if (node == null)
            {
                Debug.LogWarning("[Dialogue] StartDialogue called with no DialogueData.", this);
                return false;
            }

            if (IsDialogueActive)
            {
                return false;
            }

            onConversationComplete = onComplete;
            CurrentNode = node;
            CurrentLineIndex = 0;

            if (DialogueStarted != null)
            {
                DialogueStarted(node);
            }

            PublishCurrentLine();
            return true;
        }

        /// <summary>Continue button / continue key. Advances one line, or closes at the end.</summary>
        public void Advance()
        {
            if (_inkActive)
            {
                if (IsAtChoicePoint) return;
                if (InkBlockAfterLine != null && InkBlockAfterLine(CurrentInkTags))
                {
                    CurrentInkTags = new string[0];
                    EndDialogue();
                    return;
                }
                AdvanceInk();
                return;
            }
            if (!IsDialogueActive)
            {
                return;
            }

            if (IsAtChoicePoint)
            {
                // Waiting for the player to pick a branch.
                return;
            }

            if (CurrentLineIndex < CurrentNode.Lines.Count - 1)
            {
                CurrentLineIndex++;
                PublishCurrentLine();
                return;
            }

            EndDialogue();
        }

        /// <summary>Follows a branch. Index refers to CurrentNode.Choices.</summary>
        public void SelectChoice(int choiceIndex)
        {
            if (_inkActive)
            {
                if (choiceIndex < 0 || choiceIndex >= InkStory.currentChoices.Count) return;
                InkStory.ChooseChoiceIndex(choiceIndex);
                AdvanceInk();
                return;
            }
            if (!IsDialogueActive || !CurrentNode.HasChoices)
            {
                return;
            }

            if (choiceIndex < 0 || choiceIndex >= CurrentNode.Choices.Count)
            {
                return;
            }

            DialogueChoice choice = CurrentNode.Choices[choiceIndex];
            DialogueData next = choice.NextNode;

            if (next == null)
            {
                EndDialogue();
                return;
            }

            CurrentNode = next;
            CurrentLineIndex = 0;
            PublishCurrentLine();
        }

        /// <summary>Closes the conversation and fires the completion callback exactly once.</summary>
        public void EndDialogue()
        {
            if (!IsDialogueActive)
            {
                return;
            }

            CurrentNode = null;
            _inkActive = false;
            CurrentLineIndex = 0;

            if (ChoicesChanged != null)
            {
                ChoicesChanged(new DialogueChoice[0]);
            }

            Action callback = onConversationComplete;
            onConversationComplete = null;

            if (callback != null)
            {
                callback();
            }

            if (DialogueEnded != null)
            {
                DialogueEnded();
            }
        }

        private void PublishCurrentLine()
        {
            string speaker = CurrentNode.GetSpeakerFor(CurrentLineIndex);
            string text = CurrentNode.Lines.Count > 0 ? CurrentNode.Lines[CurrentLineIndex].Text : string.Empty;

            if (LineChanged != null)
            {
                LineChanged(speaker, text);
            }

            if (ChoicesChanged == null)
            {
                return;
            }

            if (IsAtChoicePoint)
            {
                var array = new DialogueChoice[CurrentNode.Choices.Count];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = CurrentNode.Choices[i];
                }

                ChoicesChanged(array);
            }
            else
            {
                ChoicesChanged(new DialogueChoice[0]);
            }
        }

        public void SetInkStory(Ink.Runtime.Story story)
        {
            if (IsDialogueActive) throw new InvalidOperationException("End the active conversation before changing Ink story.");
            InkStory = story;
            _pendingText = null;
            _pendingTags = null;
            _inkSpeaker = "Dẫn chuyện";
            CurrentSpeakerId = "Narrator";
            CurrentInkTags = new string[0];
        }

        public void ResumeInk(Action onComplete = null)
        {
            if (IsDialogueActive || InkStory == null) return;
            _inkActive = true;
            onConversationComplete = onComplete;
            DialogueStarted?.Invoke(null);
            if (_pendingText != null) PublishInkLine();
            else AdvanceInk();
        }

        private void AdvanceInk()
        {
            while (InkStory.canContinue)
            {
                string previousState = InkStopBeforeLine != null ? InkStory.state.ToJson() : null;
                _pendingText = InkStory.Continue().Trim();
                _pendingTags = InkStory.currentTags.ToArray();
                if (InkStopBeforeLine != null && InkStopBeforeLine(_pendingTags))
                {
                    InkStory.state.LoadJson(previousState);
                    _pendingText = null;
                    EndDialogue();
                    return;
                }
                if (string.IsNullOrWhiteSpace(_pendingText)) continue;
                if (InkPauseBeforeLine != null && InkPauseBeforeLine(_pendingTags))
                {
                    EndDialogue();
                    return;
                }
                PublishInkLine();
                return;
            }
            if (InkAtExploreChoices)
            {
                InkExploreReached?.Invoke();
                EndDialogue();
            }
            else if (InkStory.currentChoices.Count > 0) PublishInkChoices();
            else EndDialogue();
        }

        private void PublishInkLine()
        {
            foreach (string tag in _pendingTags)
            {
                if (!tag.StartsWith("speaker:")) continue;
                CurrentSpeakerId = tag.Substring(8).Trim();
                _inkSpeaker = SpeakerCatalog.Get(CurrentSpeakerId).DisplayName;
            }
            CurrentInkTags = _pendingTags;
            InkTagsChanged?.Invoke(_pendingTags);
            LineChanged?.Invoke(_inkSpeaker, _pendingText);
            _pendingText = null;
            PublishInkChoices();
        }

        private void PublishInkChoices()
        {
            if (InkAtExploreChoices) { ChoicesChanged?.Invoke(new DialogueChoice[0]); return; }
            var choices = new DialogueChoice[InkStory.currentChoices.Count];
            for (int i = 0; i < choices.Length; i++)
                choices[i] = new DialogueChoice(InkStory.currentChoices[i].text, InkStory.currentChoices[i].tags);
            ChoicesChanged?.Invoke(choices);
        }
    }
}

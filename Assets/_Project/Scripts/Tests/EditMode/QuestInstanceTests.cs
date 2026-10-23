using NUnit.Framework;
using ThuyKieu.Quest;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Tests
{
    public class QuestInstanceTests
    {
        private QuestData quest;

        [SetUp]
        public void SetUp()
        {
            quest = ScriptableObject.CreateInstance<QuestData>();
            var so = new SerializedObject(quest);
            SerializedProperty objectives = so.FindProperty("objectives");
            objectives.arraySize = 2;
            SetObjective(objectives.GetArrayElementAtIndex(0), ObjectiveType.TalkToNPC, "father");
            SetObjective(objectives.GetArrayElementAtIndex(1), ObjectiveType.InteractWithObject, "letter");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(quest);
        }

        [Test]
        public void NewInstance_IsActive_WithNoObjectiveDone()
        {
            var instance = new QuestInstance(quest);

            Assert.AreEqual(QuestStatus.Active, instance.Status);
            Assert.IsFalse(instance.AllObjectivesCompleted());
        }

        [Test]
        public void TryCompleteObjective_MatchesTypeAndIdIgnoringCase()
        {
            var instance = new QuestInstance(quest);

            Assert.AreEqual(-1, instance.TryCompleteObjective(ObjectiveType.InteractWithObject, "father"));
            Assert.AreEqual(0, instance.TryCompleteObjective(ObjectiveType.TalkToNPC, "FATHER"));
            Assert.IsTrue(instance.IsObjectiveCompleted(0));
        }

        [Test]
        public void AllObjectivesCompleted_AfterEveryObjective()
        {
            var instance = new QuestInstance(quest);
            instance.TryCompleteObjective(ObjectiveType.TalkToNPC, "father");
            instance.TryCompleteObjective(ObjectiveType.InteractWithObject, "letter");

            Assert.IsTrue(instance.AllObjectivesCompleted());
        }

        [Test]
        public void CompletedQuest_IgnoresFurtherProgress()
        {
            var instance = new QuestInstance(quest);
            instance.MarkCompleted();

            Assert.AreEqual(-1, instance.TryCompleteObjective(ObjectiveType.TalkToNPC, "father"));
        }

        private static void SetObjective(SerializedProperty objective, ObjectiveType type, string targetId)
        {
            objective.FindPropertyRelative("type").enumValueIndex = (int)type;
            objective.FindPropertyRelative("targetId").stringValue = targetId;
        }
    }
}

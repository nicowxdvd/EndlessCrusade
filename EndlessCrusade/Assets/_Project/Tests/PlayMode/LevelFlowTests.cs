using EC.Data;
using EC.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EC.Tests.PlayMode
{
    public class LevelFlowTests
    {
        GameObject host;

        [TearDown]
        public void TearDown()
        {
            if (host != null)
                Object.DestroyImmediate(host);
        }

        StoryPlayer CreatePlayer()
        {
            host = new GameObject("Story");
            host.SetActive(false);
            var root = new GameObject("Root");
            root.transform.SetParent(host.transform);
            var player = host.AddComponent<StoryPlayer>();
            player.root = root;
            player.image = NewChild(root, "Image").AddComponent<Image>();
            player.text = NewChild(root, "Text").AddComponent<TextMeshProUGUI>();
            player.advanceButton = NewChild(root, "Advance").AddComponent<Button>();
            player.skipButton = NewChild(root, "Skip").AddComponent<Button>();
            host.SetActive(true);
            return player;
        }

        static GameObject NewChild(GameObject parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            return child;
        }

        static StorySequence Sequence(int panels)
        {
            var sequence = ScriptableObject.CreateInstance<StorySequence>();
            sequence.panels = new StoryPanel[panels];
            for (int i = 0; i < panels; i++)
                sequence.panels[i] = new StoryPanel { text = "p" + i };
            return sequence;
        }

        [Test]
        public void StoryPlayer_AdvancesThroughPanelsAndFinishes()
        {
            var player = CreatePlayer();
            var finished = 0;
            player.Play(Sequence(2), () => finished++);

            Assert.IsTrue(player.IsPlaying);
            Assert.AreEqual("p0", player.text.text);
            player.Advance();
            Assert.AreEqual("p1", player.text.text);
            player.Advance();
            Assert.IsFalse(player.IsPlaying);
            Assert.AreEqual(1, finished);
        }

        [Test]
        public void StoryPlayer_FinishSkipsRemainingPanels()
        {
            var player = CreatePlayer();
            var finished = 0;
            player.Play(Sequence(5), () => finished++);

            player.Finish();
            player.Finish();

            Assert.IsFalse(player.IsPlaying);
            Assert.AreEqual(1, finished);
        }

        [Test]
        public void StoryPlayer_NullSequenceFinishesImmediately()
        {
            var player = CreatePlayer();
            var finished = 0;
            player.Play(null, () => finished++);

            Assert.IsFalse(player.IsPlaying);
            Assert.AreEqual(1, finished);
        }

        [Test]
        public void Tutorial_OnlyAdvancesOnExpectedStepInOrder()
        {
            host = new GameObject("Tutorial");
            host.SetActive(false);
            var tutorial = host.AddComponent<TutorialController>();
            tutorial.root = new GameObject("Root");
            tutorial.root.transform.SetParent(host.transform);
            tutorial.label = tutorial.root.AddComponent<TextMeshProUGUI>();
            host.SetActive(true);
            var finished = 0;
            tutorial.Begin(() => finished++);

            tutorial.Notify(TutorialStep.Whip);
            Assert.AreEqual(TutorialStep.Move, tutorial.Current);

            tutorial.Notify(TutorialStep.Move);
            tutorial.Notify(TutorialStep.Sword);
            tutorial.Notify(TutorialStep.Whip);
            Assert.AreEqual(0, finished);
            tutorial.Notify(TutorialStep.HolyWater);

            Assert.IsFalse(tutorial.IsActive);
            Assert.AreEqual(1, finished);
        }
    }
}

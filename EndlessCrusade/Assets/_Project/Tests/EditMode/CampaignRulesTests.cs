using System.Collections.Generic;
using EC.Data;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class CampaignRulesTests
    {
        CampaignDefinition campaign;
        readonly List<Object> created = new List<Object>();

        T Make<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            created.Add(asset);
            return asset;
        }

        LevelDefinition Level(string id)
        {
            var level = Make<LevelDefinition>();
            level.id = id;
            return level;
        }

        [SetUp]
        public void SetUp()
        {
            var first = Make<ChapterDefinition>();
            first.levels = new[] { Level("a1"), Level("a2") };
            var empty = Make<ChapterDefinition>();
            empty.levels = new LevelDefinition[0];
            var second = Make<ChapterDefinition>();
            second.levels = new[] { Level("b1") };
            campaign = Make<CampaignDefinition>();
            campaign.chapters = new[] { first, empty, second };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var asset in created) Object.DestroyImmediate(asset);
            created.Clear();
        }

        [Test]
        public void FirstLevel_IsAvailableOnFreshInstall()
        {
            Assert.AreEqual(LevelNodeState.Available, CampaignRules.StateOf(campaign, "a1", new List<string>()));
        }

        [Test]
        public void LevelWithIncompletePrevious_IsLocked()
        {
            Assert.AreEqual(LevelNodeState.Locked, CampaignRules.StateOf(campaign, "a2", new List<string>()));
        }

        [Test]
        public void CompletedLevel_IsCompletedAndUnlocksNext()
        {
            var done = new List<string> { "a1" };
            Assert.AreEqual(LevelNodeState.Completed, CampaignRules.StateOf(campaign, "a1", done));
            Assert.AreEqual(LevelNodeState.Available, CampaignRules.StateOf(campaign, "a2", done));
        }

        [Test]
        public void NextChapter_UnlocksAcrossEmptyChapter()
        {
            Assert.AreEqual(LevelNodeState.Locked, CampaignRules.StateOf(campaign, "b1", new List<string> { "a1" }));
            Assert.AreEqual(LevelNodeState.Available, CampaignRules.StateOf(campaign, "b1", new List<string> { "a1", "a2" }));
        }

        [Test]
        public void UnknownLevel_IsLocked()
        {
            Assert.AreEqual(LevelNodeState.Locked, CampaignRules.StateOf(campaign, "zz", new List<string>()));
        }

        [Test]
        public void HasLevels_FalseForEmptyChapter()
        {
            Assert.IsFalse(CampaignRules.HasLevels(campaign.chapters[1]));
            Assert.IsTrue(CampaignRules.HasLevels(campaign.chapters[0]));
        }
    }
}

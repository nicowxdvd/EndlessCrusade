using System.Collections.Generic;
using EC.Data;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class TroopUnlockRulesTests
    {
        readonly List<Object> created = new List<Object>();
        CampaignDefinition campaign;

        T Make<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            created.Add(asset);
            return asset;
        }

        ChapterDefinition Chapter(string id, params string[] levelIds)
        {
            var chapter = Make<ChapterDefinition>();
            chapter.id = id;
            chapter.levels = new LevelDefinition[levelIds.Length];
            for (int i = 0; i < levelIds.Length; i++)
            {
                chapter.levels[i] = Make<LevelDefinition>();
                chapter.levels[i].id = levelIds[i];
            }
            return chapter;
        }

        TroopDefinition Troop(string unlockChapter)
        {
            var troop = Make<TroopDefinition>();
            troop.unlockChapterId = unlockChapter;
            return troop;
        }

        [SetUp]
        public void SetUp()
        {
            campaign = Make<CampaignDefinition>();
            campaign.chapters = new[] { Chapter("ch1", "l1"), Chapter("ch2", "l2a", "l2b"), Chapter("ch3") };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var asset in created) Object.DestroyImmediate(asset);
            created.Clear();
        }

        [Test]
        public void TroopWithoutChapter_IsAlwaysUnlocked()
        {
            Assert.IsTrue(TroopUnlockRules.IsUnlocked(Troop(""), campaign, new List<string>()));
        }

        [Test]
        public void Troop_UnlocksWhenLastLevelOfPreviousChapterIsCompleted()
        {
            var troop = Troop("ch2");
            Assert.IsFalse(TroopUnlockRules.IsUnlocked(troop, campaign, new List<string>()));
            Assert.IsTrue(TroopUnlockRules.IsUnlocked(troop, campaign, new List<string> { "l1" }));
        }

        [Test]
        public void Troop_StaysLockedUntilLastLevelNotJustFirst()
        {
            var troop = Troop("ch3");
            Assert.IsFalse(TroopUnlockRules.IsUnlocked(troop, campaign, new List<string> { "l1", "l2a" }));
            Assert.IsTrue(TroopUnlockRules.IsUnlocked(troop, campaign, new List<string> { "l1", "l2a", "l2b" }));
        }

        [Test]
        public void Troop_OfUnknownChapter_IsLocked()
        {
            Assert.IsFalse(TroopUnlockRules.IsUnlocked(Troop("nope"), campaign, new List<string> { "l1" }));
        }

        [Test]
        public void Troop_OfChapterAfterEmptyChapter_IsLocked()
        {
            campaign.chapters = new[] { Chapter("ch1", "l1"), Chapter("ch2"), Chapter("ch3") };
            Assert.IsFalse(TroopUnlockRules.IsUnlocked(Troop("ch3"), campaign, new List<string> { "l1" }));
        }
    }
}

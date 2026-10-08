using System.Collections.Generic;
using EC.Data;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class BossModuleTests
    {
        GameObject go;
        AreaStrikeModule strike;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("Troll");
            strike = go.AddComponent<AreaStrikeModule>();
            strike.telegraphSeconds = 1.2f;
            strike.vulnerableSeconds = 4f;
            strike.cooldown = 6f;
            strike.Advance(6f, false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Advance_WithoutTargetInRange_DoesNothing()
        {
            Assert.AreEqual(StrikeEvent.None, strike.Advance(1f, false));
            Assert.IsFalse(strike.Telegraphing);
        }

        [Test]
        public void Advance_TelegraphsThenStrikesAfterWarning()
        {
            Assert.AreEqual(StrikeEvent.TelegraphStarted, strike.Advance(0.1f, true));
            Assert.IsTrue(strike.Telegraphing);
            Assert.AreEqual(StrikeEvent.None, strike.Advance(1f, true));
            Assert.AreEqual(StrikeEvent.Struck, strike.Advance(0.25f, true));
            Assert.IsFalse(strike.Telegraphing);
        }

        [Test]
        public void Strike_OpensVulnerabilityWindowForFourSeconds()
        {
            strike.Advance(0.1f, true);
            strike.Advance(1.2f, true);
            Assert.IsTrue(strike.Vulnerable);
            strike.Advance(3.5f, false);
            Assert.IsTrue(strike.Vulnerable);
            strike.Advance(0.6f, false);
            Assert.IsFalse(strike.Vulnerable);
        }

        [Test]
        public void Strike_StartsCooldownBeforeNextTelegraph()
        {
            strike.Advance(0.1f, true);
            strike.Advance(1.2f, true);
            Assert.AreEqual(StrikeEvent.None, strike.Advance(5f, true));
            Assert.AreEqual(StrikeEvent.TelegraphStarted, strike.Advance(1.1f, true));
        }

        [Test]
        public void VampireLord_ChangesPhaseAtSixtySixAndThirtyThreePercent()
        {
            Assert.AreEqual(1, VampireLordModule.PhaseFor(1f));
            Assert.AreEqual(1, VampireLordModule.PhaseFor(0.67f));
            Assert.AreEqual(2, VampireLordModule.PhaseFor(0.66f));
            Assert.AreEqual(2, VampireLordModule.PhaseFor(0.34f));
            Assert.AreEqual(3, VampireLordModule.PhaseFor(0.33f));
            Assert.AreEqual(3, VampireLordModule.PhaseFor(0f));
        }

        [Test]
        public void CampaignRules_IsComplete_RequiresEveryLevelAndNoEmptyChapter()
        {
            var created = new List<Object>();
            LevelDefinition Level(string id)
            {
                var level = ScriptableObject.CreateInstance<LevelDefinition>();
                level.id = id;
                created.Add(level);
                return level;
            }

            var chapter = ScriptableObject.CreateInstance<ChapterDefinition>();
            chapter.levels = new[] { Level("a"), Level("b") };
            var campaign = ScriptableObject.CreateInstance<CampaignDefinition>();
            campaign.chapters = new[] { chapter };
            created.Add(chapter);
            created.Add(campaign);

            Assert.IsFalse(CampaignRules.IsComplete(campaign, new List<string> { "a" }));
            Assert.IsTrue(CampaignRules.IsComplete(campaign, new List<string> { "a", "b" }));

            var empty = ScriptableObject.CreateInstance<ChapterDefinition>();
            created.Add(empty);
            campaign.chapters = new[] { chapter, empty };
            Assert.IsFalse(CampaignRules.IsComplete(campaign, new List<string> { "a", "b" }));

            foreach (var asset in created) Object.DestroyImmediate(asset);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using EC.Core;
using EC.Data;
using EC.Services;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class UpgradeServiceTests
    {
        string dir;
        JsonSaveService save;
        CurrencyService currency;
        UpgradeService service;
        readonly List<Object> created = new List<Object>();

        UpgradeDefinition Upgrade(string id, UpgradeStat stat, float value, int maxLevel = 3, string target = null)
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.id = id;
            upgrade.stat = stat;
            upgrade.valuePerLevel = value;
            upgrade.maxLevel = maxLevel;
            upgrade.baseCost = 100;
            upgrade.costGrowth = 1.35f;
            upgrade.targetId = target;
            created.Add(upgrade);
            return upgrade;
        }

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ec_up_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            save = new JsonSaveService(dir);
            save.Load();
            currency = new CurrencyService(save);
            service = new UpgradeService(save, currency);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var asset in created) Object.DestroyImmediate(asset);
            created.Clear();
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        [Test]
        public void CostAtLevel_FollowsExponentialFormula()
        {
            var upgrade = Upgrade("u", UpgradeStat.HeroHealth, 0.1f);
            Assert.AreEqual(100, upgrade.CostAtLevel(0));
            Assert.AreEqual(135, upgrade.CostAtLevel(1));
            Assert.AreEqual(182, upgrade.CostAtLevel(2));
            Assert.AreEqual(Mathf.RoundToInt(100 * Mathf.Pow(1.35f, 5)), upgrade.CostAtLevel(5));
        }

        [Test]
        public void TryBuy_SpendsCostAndRaisesLevel()
        {
            var upgrade = Upgrade("u", UpgradeStat.HeroHealth, 0.1f);
            currency.Add(CurrencyType.Gold, 500);

            Assert.AreEqual(PurchaseResult.Success, service.TryBuy(upgrade));
            Assert.AreEqual(1, service.GetLevel("u"));
            Assert.AreEqual(400, currency.Balance(CurrencyType.Gold));
            Assert.AreEqual(135, service.NextCost(upgrade));
        }

        [Test]
        public void TryBuy_WithoutFunds_ChangesNothing()
        {
            var upgrade = Upgrade("u", UpgradeStat.HeroHealth, 0.1f);
            currency.Add(CurrencyType.Gold, 99);

            Assert.AreEqual(PurchaseResult.NotEnoughFunds, service.TryBuy(upgrade));
            Assert.AreEqual(0, service.GetLevel("u"));
            Assert.AreEqual(99, currency.Balance(CurrencyType.Gold));
        }

        [Test]
        public void TryBuy_AtMaxLevel_IsRejected()
        {
            var upgrade = Upgrade("u", UpgradeStat.HeroHealth, 0.1f, 1);
            currency.Add(CurrencyType.Gold, 1000);
            service.TryBuy(upgrade);

            Assert.AreEqual(PurchaseResult.MaxLevel, service.TryBuy(upgrade));
            Assert.AreEqual(900, currency.Balance(CurrencyType.Gold));
        }

        [Test]
        public void Levels_PersistAfterReload()
        {
            var upgrade = Upgrade("u", UpgradeStat.HeroHealth, 0.1f);
            currency.Add(CurrencyType.Gold, 500);
            service.TryBuy(upgrade);
            service.TryBuy(upgrade);

            var reloaded = new JsonSaveService(dir);
            reloaded.Load();
            Assert.AreEqual(2, new UpgradeService(reloaded, new CurrencyService(reloaded)).GetLevel("u"));
        }

        [Test]
        public void Consumables_RespectStackAndConsume()
        {
            var potion = ScriptableObject.CreateInstance<ConsumableDefinition>();
            created.Add(potion);
            potion.id = "potion";
            potion.currency = CurrencyType.Gold;
            potion.cost = 10;
            potion.maxStack = 2;
            currency.Add(CurrencyType.Gold, 100);

            Assert.AreEqual(PurchaseResult.Success, service.TryBuyConsumable(potion));
            Assert.AreEqual(PurchaseResult.Success, service.TryBuyConsumable(potion));
            Assert.AreEqual(PurchaseResult.MaxLevel, service.TryBuyConsumable(potion));
            Assert.AreEqual(80, currency.Balance(CurrencyType.Gold));

            Assert.IsTrue(service.TryConsume("potion"));
            Assert.AreEqual(1, service.GetCount("potion"));
            Assert.IsTrue(service.TryConsume("potion"));
            Assert.IsFalse(service.TryConsume("potion"));
        }

        [Test]
        public void StatModifierSet_CombinesLevels()
        {
            var health = Upgrade("hp", UpgradeStat.HeroHealth, 0.1f);
            var sword = Upgrade("sw", UpgradeStat.SwordDamage, 0.08f);
            var cooldown = Upgrade("cd", UpgradeStat.CooldownReduction, 0.5f);
            var troop = Upgrade("tr", UpgradeStat.TroopPower, 0.1f, 5, "squire");
            var crossbows = Upgrade("xb", UpgradeStat.BaseCrossbows, 0f, 1);
            var levels = new Dictionary<string, int> { { "hp", 3 }, { "sw", 2 }, { "cd", 3 }, { "tr", 2 }, { "xb", 1 } };

            var set = StatModifierSet.Compute(new[] { health, sword, cooldown, troop, crossbows }, id => levels.TryGetValue(id, out var l) ? l : 0);

            Assert.AreEqual(1.3f, set.heroHealth, 0.0001f);
            Assert.AreEqual(1.16f, set.swordDamage, 0.0001f);
            Assert.AreEqual(1f, set.whipDamage, 0.0001f);
            Assert.AreEqual(StatModifierSet.MinCooldownMultiplier, set.cooldown, 0.0001f);
            Assert.AreEqual(1.2f, set.TroopMultiplier("squire"), 0.0001f);
            Assert.AreEqual(1f, set.TroopMultiplier("paladin"), 0.0001f);
            Assert.IsTrue(set.baseCrossbows);
        }

        [Test]
        public void StatModifierSet_WithoutLevels_IsIdentity()
        {
            var health = Upgrade("hp", UpgradeStat.HeroHealth, 0.1f);
            var set = StatModifierSet.Compute(new[] { health }, id => 0);
            Assert.AreEqual(1f, set.heroHealth);
            Assert.IsFalse(set.baseCrossbows);
        }
    }
}

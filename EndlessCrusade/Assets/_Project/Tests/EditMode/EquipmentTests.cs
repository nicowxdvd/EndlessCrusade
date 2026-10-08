using System.Collections.Generic;
using System.IO;
using EC.Data;
using EC.Gameplay;
using EC.Services;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class EquipmentTests
    {
        readonly List<Object> created = new List<Object>();
        string dir;
        JsonSaveService save;
        EquipmentCatalog catalog;
        CampaignDefinition campaign;

        T Make<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            created.Add(asset);
            return asset;
        }

        EquipmentDefinition Item(string id, EquipmentSlot slot, int health = 0, float reduction = 0f, float block = 0f)
        {
            var item = Make<EquipmentDefinition>();
            item.id = id;
            item.slot = slot;
            item.bonusHealth = health;
            item.damageReduction = reduction;
            item.blockFraction = block;
            return item;
        }

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ec_eq_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            save = new JsonSaveService(dir);
            save.Load();

            catalog = Make<EquipmentCatalog>();
            catalog.equipment = new[] { Item("armor", EquipmentSlot.Armor, 80, 0.2f), Item("armor2", EquipmentSlot.Armor, 10, 0.1f), Item("shield", EquipmentSlot.Shield, 0, 0f, 0.7f) };
            var miracle = Make<MiracleDefinition>();
            miracle.id = "blessing";
            miracle.ability = Make<AbilityDefinition>();
            catalog.miracles = new[] { miracle };

            var level = Make<LevelDefinition>();
            level.id = "l1";
            var chapter = Make<ChapterDefinition>();
            chapter.id = "ch1";
            chapter.levels = new[] { level };
            chapter.rewardEquipmentIds = new[] { "armor", "blessing" };
            campaign = Make<CampaignDefinition>();
            campaign.chapters = new[] { chapter };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var asset in created) Object.DestroyImmediate(asset);
            created.Clear();
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        [Test]
        public void FreshInstall_OwnsNothing()
        {
            var service = new EquipmentService(save, catalog);
            Assert.IsEmpty(service.GrantChapterRewards(campaign));
            Assert.IsFalse(service.Owns("armor"));
        }

        [Test]
        public void CompletingChapter_GrantsRewardsOnlyOnce()
        {
            var service = new EquipmentService(save, catalog);
            save.Current.progress.completedLevels.Add("l1");

            Assert.AreEqual(2, service.GrantChapterRewards(campaign).Count);
            Assert.IsEmpty(service.GrantChapterRewards(campaign));
            Assert.IsTrue(service.Owns("armor"));
            Assert.IsTrue(service.IsEquipped("armor"));
            Assert.IsTrue(service.IsEquipped("blessing"));
        }

        [Test]
        public void Equip_ReplacesItemInSameSlot()
        {
            var service = new EquipmentService(save, catalog);
            save.Current.equipment.owned.Add("armor");
            save.Current.equipment.owned.Add("armor2");

            Assert.IsTrue(service.Equip("armor"));
            Assert.IsTrue(service.Equip("armor2"));
            Assert.IsFalse(service.IsEquipped("armor"));
            Assert.IsTrue(service.IsEquipped("armor2"));
            Assert.IsTrue(service.Unequip("armor2"));
            Assert.IsFalse(service.IsEquipped("armor2"));
        }

        [Test]
        public void Equip_NotOwned_IsRejected()
        {
            Assert.IsFalse(new EquipmentService(save, catalog).Equip("shield"));
        }

        [Test]
        public void Loadout_SumsArmorAndKeepsShieldBlock()
        {
            var loadout = HeroLoadout.Compute(catalog, new[] { "armor", "shield", "blessing" });
            Assert.AreEqual(80, loadout.bonusHealth);
            Assert.AreEqual(0.2f, loadout.damageReduction, 0.0001f);
            Assert.AreEqual(0.7f, loadout.blockFraction, 0.0001f);
            Assert.AreEqual(1, loadout.miracles.Count);
            Assert.IsTrue(loadout.Has(EquipmentSlot.Armor));
            Assert.IsFalse(loadout.Has(EquipmentSlot.HeavyWeapon));
        }

        [Test]
        public void EmptyLoadout_HasNoEquipment()
        {
            var loadout = HeroLoadout.Compute(catalog, new string[0]);
            Assert.AreEqual(0, loadout.bonusHealth);
            Assert.IsFalse(loadout.Has(EquipmentSlot.Armor));
            Assert.IsFalse(loadout.Has(EquipmentSlot.Shield));
        }

        [Test]
        public void HeroDefense_AppliesReductionAndBlock()
        {
            var defense = new HeroDefense(0.2f, 0.7f);
            Assert.AreEqual(80, defense.Modify(100, null));
            defense.Blocking = true;
            Assert.AreEqual(24, defense.Modify(100, null));
            Assert.AreEqual(1, defense.Modify(1, null));
            Assert.AreEqual(0, defense.Modify(0, null));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using EC.Core;
using EC.Data;
using EC.Gameplay;
using EC.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EC.Tests.PlayMode
{
    public class AbilityTests
    {
        readonly List<Object> created = new List<Object>();
        AbilityDefinition ability;
        PoolService pool;
        AbilityComponent abilityComponent;
        GameObject hero;

        [SetUp]
        public void SetUp()
        {
            ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.cooldown = 0.5f;
            ability.damage = 40;
            ability.radius = 2.5f;
            ability.undeadMultiplier = 2f;
            ability.throwDistance = 6f;
            ability.projectilePrefab = Template("Projectile", typeof(HolyWaterProjectile));
            ability.projectilePrefab.GetComponent<HolyWaterProjectile>().flightTime = 0.1f;
            ability.explosionPrefab = Template("Explosion", typeof(ExplosionEffect));
            ability.explosionPrefab.GetComponent<ExplosionEffect>().duration = 0.1f;
            created.Add(ability);

            pool = new GameObject("Pool").AddComponent<PoolService>();
            created.Add(pool.gameObject);

            hero = new GameObject("Hero");
            hero.SetActive(false);
            hero.AddComponent<HealthComponent>().Initialize(100, Team.Player);
            abilityComponent = hero.AddComponent<AbilityComponent>();
            abilityComponent.abilities = new[] { ability, null, null };
            abilityComponent.pool = pool;
            hero.transform.position = Vector3.zero;
            hero.SetActive(true);
            created.Add(hero);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in created)
                if (obj != null)
                    Object.DestroyImmediate(obj);
            created.Clear();
        }

        GameObject Template(string name, params System.Type[] components)
        {
            var go = new GameObject(name);
            foreach (var type in components)
                go.AddComponent(type);
            go.SetActive(false);
            created.Add(go);
            return go;
        }

        HealthComponent SpawnEnemy(float x, CreatureTag tags = CreatureTag.None, int health = 200)
        {
            var go = new GameObject("Enemy");
            go.SetActive(false);
            var h = go.AddComponent<HealthComponent>();
            h.Initialize(health, Team.Enemy);
            h.tags = tags;
            go.transform.position = new Vector3(x, 0f, 0f);
            go.SetActive(true);
            created.Add(go);
            return h;
        }

        [UnityTest]
        public IEnumerator ThrownWater_LandsAtThrowDistance_AndDamagesOnlyInsideRadius()
        {
            var inside = SpawnEnemy(6f);
            var edge = SpawnEnemy(8.4f);
            var outside = SpawnEnemy(8.6f);
            var behind = SpawnEnemy(-6f);

            Assert.IsTrue(abilityComponent.TryUse(0));
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(160, inside.Current);
            Assert.AreEqual(160, edge.Current);
            Assert.AreEqual(200, outside.Current);
            Assert.AreEqual(200, behind.Current);
        }

        [UnityTest]
        public IEnumerator Undead_TakeMultipliedDamage()
        {
            var normal = SpawnEnemy(6f);
            var undead = SpawnEnemy(6.5f, CreatureTag.Undead);

            abilityComponent.TryUse(0);
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(160, normal.Current);
            Assert.AreEqual(120, undead.Current);
        }

        [UnityTest]
        public IEnumerator Cooldown_BlocksSecondUse_ThenReleases()
        {
            var enemy = SpawnEnemy(6f);

            Assert.IsTrue(abilityComponent.TryUse(0));
            Assert.IsFalse(abilityComponent.TryUse(0));
            EventBus<AbilityRequested>.Publish(new AbilityRequested(0));
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(160, enemy.Current);

            yield return new WaitForSeconds(0.4f);
            Assert.AreEqual(0f, abilityComponent.GetNormalized(0));
            EventBus<AbilityRequested>.Publish(new AbilityRequested(0));
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(120, enemy.Current);
        }

        [Test]
        public void InvalidSlots_DoNothing()
        {
            Assert.IsFalse(abilityComponent.TryUse(1));
            Assert.IsFalse(abilityComponent.TryUse(2));
            Assert.IsFalse(abilityComponent.TryUse(3));
            Assert.IsFalse(abilityComponent.TryUse(-1));
        }

        [UnityTest]
        public IEnumerator CooldownEvents_ReportDecreasingNormalizedValue()
        {
            var values = new List<float>();
            void Handler(AbilityCooldownChanged evt)
            {
                if (evt.Slot == 0)
                    values.Add(evt.Normalized);
            }

            EventBus<AbilityCooldownChanged>.Subscribe(Handler);
            abilityComponent.TryUse(0);
            yield return new WaitForSeconds(0.7f);
            EventBus<AbilityCooldownChanged>.Unsubscribe(Handler);

            Assert.AreEqual(1f, values[0]);
            Assert.AreEqual(0f, values[values.Count - 1]);
            for (var i = 1; i < values.Count; i++)
                Assert.LessOrEqual(values[i], values[i - 1]);
        }

        [UnityTest]
        public IEnumerator Projectile_AndExplosion_AreReusedFromPool()
        {
            SpawnEnemy(6f, health: 100000);

            abilityComponent.TryUse(0);
            yield return new WaitForSeconds(0.3f);
            var pooledAfterFirst = pool.GetComponentsInChildren<PooledObject>(true).Length;
            Assert.AreEqual(2, pooledAfterFirst);
            Assert.AreEqual(0, pool.GetActiveCount(ability.projectilePrefab));
            Assert.AreEqual(0, pool.GetActiveCount(ability.explosionPrefab));

            yield return new WaitForSeconds(0.4f);
            abilityComponent.TryUse(0);
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(pooledAfterFirst, pool.GetComponentsInChildren<PooledObject>(true).Length);
            Assert.AreEqual(0, pool.GetActiveCount(ability.projectilePrefab));
            Assert.AreEqual(0, pool.GetActiveCount(ability.explosionPrefab));
        }

        [UnityTest]
        public IEnumerator SteadyStateThrow_DoesNotAllocateManagedMemory()
        {
            SpawnEnemy(6f, health: 1000000);
            abilityComponent.TryUse(0);
            yield return new WaitForSeconds(0.7f);

            var projectile = pool.Get(ability.projectilePrefab, Vector3.zero, Quaternion.identity);
            pool.Release(projectile);
            var explosion = pool.Get(ability.explosionPrefab, Vector3.zero, Quaternion.identity);
            pool.Release(explosion);

            var start = System.GC.GetAllocatedBytesForCurrentThread();
            var p = pool.Get(ability.projectilePrefab, Vector3.zero, Quaternion.identity);
            var projectileScript = p.GetComponent<HolyWaterProjectile>();
            projectileScript.Launch(ability, pool, Vector3.zero, new Vector3(6f, 0f, 0f), Team.Player, hero);
            var allocatedLaunch = System.GC.GetAllocatedBytesForCurrentThread() - start;
            Assert.AreEqual(0, allocatedLaunch);
            yield return new WaitForSeconds(0.3f);
        }

        [UnityTest]
        public IEnumerator DeadHero_CannotUseAbility()
        {
            hero.GetComponent<HealthComponent>().TakeDamage(1000, null);
            yield return null;
            Assert.IsFalse(abilityComponent.TryUse(0));
        }
    }

    public class AbilityHudTests
    {
        AbilityButton abilityButton;
        AbilityComponent abilityComponent;
        Scene sandbox;

        [UnityTearDown]
        public IEnumerator UnloadSandbox()
        {
            Time.timeScale = 1f;
            SceneManager.SetActiveScene(SceneManager.CreateScene("AbilityHudTestsEmpty"));
            yield return SceneManager.UnloadSceneAsync(sandbox);
        }

        [UnitySetUp]
        public IEnumerator LoadSandbox()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("LaneSandbox");
            yield return null;
            yield return null;
            sandbox = SceneManager.GetSceneByName("LaneSandbox");
            abilityButton = Object.FindFirstObjectByType<AbilityButton>();
            abilityComponent = Object.FindFirstObjectByType<AbilityComponent>();
        }

        [Test]
        public void SandboxHasAbilityButtonAndHolyWaterInSlotZero()
        {
            Assert.IsNotNull(abilityButton);
            Assert.IsNotNull(abilityComponent);
            Assert.AreEqual("holy_water", abilityComponent.abilities[0].id);
            Assert.IsTrue(abilityButton.button.interactable);
            Assert.AreEqual(Image.FillMethod.Radial360, abilityButton.cooldownFill.fillMethod);
            Assert.AreEqual(0f, abilityButton.cooldownFill.fillAmount);
        }

        [UnityTest]
        public IEnumerator ClickingStartsCooldown_BlocksButton_AndFillTracksRemaining()
        {
            var cooldown = abilityComponent.abilities[0].cooldown;

            abilityButton.OnClick();
            yield return null;

            Assert.IsFalse(abilityButton.button.interactable);
            Assert.Greater(abilityButton.cooldownFill.fillAmount, 0.9f);

            yield return new WaitForSeconds(cooldown * 0.5f);
            Assert.AreEqual(abilityComponent.GetNormalized(0), abilityButton.cooldownFill.fillAmount, 0.05f);
            Assert.AreEqual(0.5f, abilityButton.cooldownFill.fillAmount, 0.1f);
            Assert.IsFalse(abilityButton.button.interactable);

            abilityButton.OnClick();
            Assert.Greater(abilityComponent.GetNormalized(0), 0.3f);

            yield return new WaitForSeconds(cooldown * 0.5f + 0.3f);
            Assert.IsTrue(abilityButton.button.interactable);
            Assert.AreEqual(0f, abilityButton.cooldownFill.fillAmount);
        }
    }
}

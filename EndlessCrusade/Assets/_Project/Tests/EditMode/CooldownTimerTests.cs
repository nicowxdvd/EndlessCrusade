using EC.Core;
using EC.Data;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class CooldownTimerTests
    {
        [Test]
        public void NewTimer_IsReady()
        {
            var timer = new CooldownTimer(12f);
            Assert.IsTrue(timer.IsReady);
            Assert.AreEqual(0f, timer.Normalized);
        }

        [Test]
        public void TryStart_BlocksForDuration_ThenReleases()
        {
            var timer = new CooldownTimer(12f);

            Assert.IsTrue(timer.TryStart());
            Assert.AreEqual(1f, timer.Normalized);
            Assert.IsFalse(timer.TryStart());

            timer.Tick(11.9f);
            Assert.IsFalse(timer.IsReady);
            Assert.IsFalse(timer.TryStart());

            timer.Tick(0.2f);
            Assert.IsTrue(timer.IsReady);
            Assert.IsTrue(timer.TryStart());
        }

        [Test]
        public void Normalized_DecreasesLinearly()
        {
            var timer = new CooldownTimer(10f);
            timer.TryStart();

            timer.Tick(2.5f);
            Assert.AreEqual(0.75f, timer.Normalized, 0.0001f);

            timer.Tick(5f);
            Assert.AreEqual(0.25f, timer.Normalized, 0.0001f);
        }

        [Test]
        public void Tick_NeverGoesBelowZero()
        {
            var timer = new CooldownTimer(1f);
            timer.TryStart();
            timer.Tick(5f);
            Assert.AreEqual(0f, timer.Remaining);
            Assert.AreEqual(0f, timer.Normalized);
        }

        [Test]
        public void ZeroDuration_IsAlwaysReady()
        {
            var timer = new CooldownTimer(0f);
            Assert.IsTrue(timer.TryStart());
            Assert.IsTrue(timer.IsReady);
            Assert.AreEqual(0f, timer.Normalized);
        }

        [Test]
        public void Reset_ClearsCooldown()
        {
            var timer = new CooldownTimer(5f);
            timer.TryStart();
            timer.Reset();
            Assert.IsTrue(timer.IsReady);
        }
    }

    public class HolyWaterMathTests
    {
        AbilityDefinition ability;

        [SetUp]
        public void SetUp()
        {
            ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.damage = 40;
            ability.undeadMultiplier = 2f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(ability);
        }

        [Test]
        public void Damage_IsBaseForNonUndead()
        {
            Assert.AreEqual(40, HolyWaterProjectile.ComputeDamage(ability, CreatureTag.None));
            Assert.AreEqual(40, HolyWaterProjectile.ComputeDamage(ability, CreatureTag.Beast | CreatureTag.Flying));
        }

        [Test]
        public void Damage_IsMultipliedForUndead()
        {
            Assert.AreEqual(80, HolyWaterProjectile.ComputeDamage(ability, CreatureTag.Undead));
            Assert.AreEqual(80, HolyWaterProjectile.ComputeDamage(ability, CreatureTag.Undead | CreatureTag.Flying));
        }

        [Test]
        public void Trajectory_StartsAtOrigin_EndsAtTarget_AndPeaksInTheMiddle()
        {
            var from = new Vector3(0f, 1f, 0f);
            var to = new Vector3(6f, 1f, 0f);

            Assert.AreEqual(from, HolyWaterProjectile.Evaluate(from, to, 2.5f, 0f));
            Assert.AreEqual(to, HolyWaterProjectile.Evaluate(from, to, 2.5f, 1f));
            var mid = HolyWaterProjectile.Evaluate(from, to, 2.5f, 0.5f);
            Assert.AreEqual(3f, mid.x, 0.0001f);
            Assert.AreEqual(3.5f, mid.y, 0.0001f);
        }
    }
}

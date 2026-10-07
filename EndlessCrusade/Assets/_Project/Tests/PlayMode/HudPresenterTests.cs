using EC.Core;
using EC.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EC.Tests.PlayMode
{
    public class HudPresenterTests
    {
        GameObject root;
        GameObject hero;
        HudPresenter presenter;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Hud");
            root.SetActive(false);
            presenter = root.AddComponent<HudPresenter>();
            presenter.heroBar = NewBar("Hero");
            presenter.baseBar = NewBar("Base");
            presenter.waveLabel = new GameObject("Wave").AddComponent<TextMeshProUGUI>();
            presenter.waveLabel.transform.SetParent(root.transform);
            root.SetActive(true);
            hero = new GameObject("HeroStub");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(hero);
        }

        Image NewBar(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform);
            return go.AddComponent<Image>();
        }

        [Test]
        public void HeroBarFollowsHeroHealthOnly()
        {
            EventBus<HeroSpawned>.Publish(new HeroSpawned(hero));
            EventBus<HealthChanged>.Publish(new HealthChanged(hero, 30, 120));
            Assert.AreEqual(0.25f, presenter.heroBar.fillAmount, 0.001f);

            var other = new GameObject("Other");
            EventBus<HealthChanged>.Publish(new HealthChanged(other, 0, 50));
            Object.DestroyImmediate(other);
            Assert.AreEqual(0.25f, presenter.heroBar.fillAmount, 0.001f);
        }

        [Test]
        public void HeroBarReachesZeroOnDeath()
        {
            EventBus<HeroSpawned>.Publish(new HeroSpawned(hero));
            EventBus<HealthChanged>.Publish(new HealthChanged(hero, 0, 120));
            Assert.AreEqual(0f, presenter.heroBar.fillAmount);
        }

        [Test]
        public void BaseBarReflectsResistance()
        {
            EventBus<BaseResistanceChanged>.Publish(new BaseResistanceChanged(150, 300));
            Assert.AreEqual(0.5f, presenter.baseBar.fillAmount, 0.001f);
        }

        [Test]
        public void WaveLabelShowsOneBasedIndexAndTotal()
        {
            EventBus<WaveStarted>.Publish(new WaveStarted(1, 5));
            Assert.AreEqual("Oleada 2 / 5", presenter.waveLabel.text);
        }

        [Test]
        public void RatioHandlesZeroMaxAndOverflow()
        {
            Assert.AreEqual(0f, HudPresenter.Ratio(10, 0));
            Assert.AreEqual(1f, HudPresenter.Ratio(20, 10));
        }
    }
}

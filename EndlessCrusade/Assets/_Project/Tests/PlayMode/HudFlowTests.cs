using System.Collections;
using EC.Core;
using EC.Gameplay;
using EC.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EC.Tests.PlayMode
{
    public class HudFlowTests
    {
        PausePanel pause;
        ResultPanel result;
        HudPresenter hud;
        HealthComponent heroHealth;

        [UnitySetUp]
        public IEnumerator LoadSandbox()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("LaneSandbox");
            yield return null;
            yield return null;
            Bind();
        }

        [TearDown]
        public void ResetTime()
        {
            Time.timeScale = 1f;
        }

        void Bind()
        {
            pause = Object.FindFirstObjectByType<PausePanel>();
            result = Object.FindFirstObjectByType<ResultPanel>();
            hud = Object.FindFirstObjectByType<HudPresenter>();
            heroHealth = Object.FindFirstObjectByType<HeroController>().GetComponent<HealthComponent>();
        }

        [UnityTest]
        public IEnumerator PauseStopsTimeAndResumeRestoresIt()
        {
            Assert.IsFalse(pause.panel.activeSelf);
            pause.Pause();
            Assert.IsTrue(pause.panel.activeSelf);
            Assert.AreEqual(0f, Time.timeScale);
            yield return null;
            pause.Resume();
            Assert.IsFalse(pause.panel.activeSelf);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator HeroDeathShowsDefeatOnlyOnce()
        {
            Assert.IsFalse(result.panel.activeSelf);
            heroHealth.TakeDamage(heroHealth.maxHealth, null);
            yield return null;
            Assert.IsTrue(result.panel.activeSelf);
            Assert.AreEqual(result.defeatText, result.title.text);
            Assert.AreEqual(0f, hud.heroBar.fillAmount);

            EventBus<LevelEnded>.Publish(new LevelEnded(LevelOutcome.Victory));
            Assert.AreEqual(result.defeatText, result.title.text);
        }

        [UnityTest]
        public IEnumerator VictoryShowsVictoryPanel()
        {
            EventBus<LevelEnded>.Publish(new LevelEnded(LevelOutcome.Victory));
            yield return null;
            Assert.IsTrue(result.panel.activeSelf);
            Assert.AreEqual(result.victoryText, result.title.text);
        }

        [UnityTest]
        public IEnumerator RetryRestartsLevelWithFullHealthAndFreshWaves()
        {
            yield return new WaitForSeconds(4f);
            Assert.IsTrue(hud.waveLabel.text.StartsWith("Oleada 1"));
            heroHealth.TakeDamage(heroHealth.maxHealth, null);
            yield return null;
            Assert.IsTrue(result.panel.activeSelf);

            result.Retry();
            yield return new WaitForSecondsRealtime(1f);
            Bind();

            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(result.panel.activeSelf);
            Assert.IsTrue(heroHealth.IsAlive);
            Assert.AreEqual(heroHealth.maxHealth, heroHealth.Current);
            Assert.AreEqual(1f, hud.heroBar.fillAmount);
            Assert.AreEqual(string.Empty, hud.waveLabel.text);

            yield return new WaitForSeconds(4f);
            Assert.IsTrue(hud.waveLabel.text.StartsWith("Oleada 1"));
        }

        [Test]
        public void SafeAreaAnchorsExcludeNotchOnWideScreen()
        {
            SafeAreaFitter.AnchorsFor(new Rect(120f, 0f, 2160f, 1020f), 2400, 1080, out var min, out var max);
            Assert.AreEqual(0.05f, min.x, 0.0001f);
            Assert.AreEqual(0.95f, max.x, 0.0001f);
            Assert.AreEqual(0f, min.y);
            Assert.AreEqual(1020f / 1080f, max.y, 0.0001f);
        }

        [Test]
        public void TouchButtonsSitInsideSafeAreaWithMinSizeAndMargin()
        {
            var safeArea = hud.transform.Find("SafeArea");
            Assert.IsNotNull(safeArea.GetComponent<SafeAreaFitter>());
            foreach (var name in new[] { "Left", "Right", "Sword", "Whip" })
            {
                var rect = (RectTransform)safeArea.Find(name);
                Assert.IsNotNull(rect, name);
                Assert.GreaterOrEqual(rect.sizeDelta.x, 96f, name);
                Assert.GreaterOrEqual(rect.sizeDelta.y, 96f, name);
                Assert.Greater(Mathf.Abs(rect.anchoredPosition.x) - rect.sizeDelta.x * 0.5f, 0f, name);
                Assert.Greater(rect.anchoredPosition.y - rect.sizeDelta.y * 0.5f, 0f, name);
            }
        }
    }
}

using EC.Core;
using EC.UI;
using NUnit.Framework;

namespace EC.Tests.EditMode
{
    public class QualityTests
    {
        [Test]
        public void Choose_PicksTierByMemoryAndCores()
        {
            Assert.AreEqual(QualityTier.High, QualityPolicy.Choose(8000, 2048, 8));
            Assert.AreEqual(QualityTier.Medium, QualityPolicy.Choose(4000, 1024, 8));
            Assert.AreEqual(QualityTier.Medium, QualityPolicy.Choose(6000, 1024, 6));
            Assert.AreEqual(QualityTier.Low, QualityPolicy.Choose(3000, 512, 8));
            Assert.AreEqual(QualityTier.Low, QualityPolicy.Choose(8000, 512, 4));
        }

        [Test]
        public void Resolve_ManualOverrideWinsOverAutomatic()
        {
            Assert.AreEqual(QualityTier.High, QualityPolicy.Resolve(0, QualityTier.High));
            Assert.AreEqual(QualityTier.Low, QualityPolicy.Resolve(1, QualityTier.High));
            Assert.AreEqual(QualityTier.Medium, QualityPolicy.Resolve(2, QualityTier.Low));
            Assert.AreEqual(QualityTier.High, QualityPolicy.Resolve(3, QualityTier.Low));
            Assert.AreEqual(QualityTier.Low, QualityPolicy.Resolve(9, QualityTier.Low));
        }

        [Test]
        public void Profiles_MatchSpecTable()
        {
            Assert.AreEqual(1f, QualityPolicy.RenderScale(QualityTier.High));
            Assert.AreEqual(0.85f, QualityPolicy.RenderScale(QualityTier.Medium));
            Assert.AreEqual(0.7f, QualityPolicy.RenderScale(QualityTier.Low));
            Assert.AreEqual(4, QualityPolicy.Msaa(QualityTier.High));
            Assert.AreEqual(2, QualityPolicy.Msaa(QualityTier.Medium));
            Assert.AreEqual(1, QualityPolicy.Msaa(QualityTier.Low));
            Assert.IsTrue(QualityPolicy.BloomEnabled(QualityTier.High));
            Assert.IsFalse(QualityPolicy.BloomEnabled(QualityTier.Medium));
            Assert.IsTrue(QualityPolicy.VignetteEnabled(QualityTier.Medium));
            Assert.IsFalse(QualityPolicy.VignetteEnabled(QualityTier.Low));
        }

        [Test]
        public void FrameTimeSampler_ComputesP95AndAverage()
        {
            var sampler = new FrameTimeSampler(100);
            for (int i = 1; i <= 100; i++)
                sampler.Add(i);

            Assert.AreEqual(95f, sampler.Percentile(0.95f), 0.0001f);
            Assert.AreEqual(50.5f, sampler.Average(), 0.0001f);
            Assert.AreEqual(100, sampler.Count);
        }

        [Test]
        public void FrameTimeSampler_OverwritesOldestWhenFull()
        {
            var sampler = new FrameTimeSampler(3);
            sampler.Add(100f);
            sampler.Add(1f);
            sampler.Add(2f);
            sampler.Add(3f);

            Assert.AreEqual(3, sampler.Count);
            Assert.AreEqual(3f, sampler.Percentile(1f), 0.0001f);
        }

        [Test]
        public void FrameTimeSampler_WithoutSamples_ReturnsZero()
        {
            var sampler = new FrameTimeSampler(10);
            Assert.AreEqual(0f, sampler.Percentile(0.95f));
            Assert.AreEqual(0f, sampler.Average());
        }

        [Test]
        public void SettingsPanel_CyclesThroughAutoAndTiers()
        {
            Assert.AreEqual(1, SettingsPanel.NextQuality(0));
            Assert.AreEqual(0, SettingsPanel.NextQuality(3));
            Assert.AreEqual("Calidad: Auto", SettingsPanel.QualityCaption(0));
            Assert.AreEqual("Calidad: Alta", SettingsPanel.QualityCaption(3));
        }
    }
}

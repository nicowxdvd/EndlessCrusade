using EC.Core;
using EC.Services;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class AudioTests
    {
        [Test]
        public void FindFreeSource_ReturnsFirstIdleOrMinusOneWhenAllBusy()
        {
            Assert.AreEqual(2, AudioManager.FindFreeSource(new[] { true, true, false, true }));
            Assert.AreEqual(-1, AudioManager.FindFreeSource(new[] { true, true, true }));
            Assert.AreEqual(AudioManager.SfxSourceCount, new bool[AudioManager.SfxSourceCount].Length);
        }

        [Test]
        public void Crossfade_OutgoingAndIncomingAddUpToOne()
        {
            AudioManager.CrossfadeVolumes(0f, out var outgoing, out var incoming);
            Assert.AreEqual(1f, outgoing, 0.0001f);
            Assert.AreEqual(0f, incoming, 0.0001f);

            AudioManager.CrossfadeVolumes(0.4f, out outgoing, out incoming);
            Assert.AreEqual(1f, outgoing + incoming, 0.0001f);

            AudioManager.CrossfadeVolumes(2f, out outgoing, out incoming);
            Assert.AreEqual(0f, outgoing, 0.0001f);
            Assert.AreEqual(1f, incoming, 0.0001f);
            Assert.AreEqual(1.5f, AudioManager.CrossfadeSeconds, 0.0001f);
        }

        [Test]
        public void Library_FindsCueById()
        {
            var library = ScriptableObject.CreateInstance<AudioLibrary>();
            var cue = ScriptableObject.CreateInstance<AudioCue>();
            cue.id = "hit";
            library.cues = new[] { cue };

            Assert.AreSame(cue, library.Find("hit"));
            Assert.IsNull(library.Find("missing"));
            Assert.IsNull(library.Find(null));
            Object.DestroyImmediate(library);
            Object.DestroyImmediate(cue);
        }

        [Test]
        public void CueWithoutClips_HasNoClips()
        {
            var cue = ScriptableObject.CreateInstance<AudioCue>();
            Assert.IsFalse(cue.HasClips);
            Object.DestroyImmediate(cue);
        }

        [Test]
        public void MusicFor_PicksTrackBySceneAndChapter()
        {
            Assert.AreEqual("music_main", MusicDirector.MusicFor("Main", null));
            Assert.AreEqual("music_map", MusicDirector.MusicFor("CampaignMap", null));
            Assert.AreEqual("music_map", MusicDirector.MusicFor("Pachinko", null));
            Assert.AreEqual("music_ch1", MusicDirector.MusicFor("Level", "lv_1_1"));
            Assert.AreEqual("music_ch3", MusicDirector.MusicFor("Level", "lv_3_2"));
            Assert.AreEqual("music_ch1", MusicDirector.MusicFor("Level", null));
        }
    }
}

using EC.Core;
using EC.Data;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EC.Tests.PlayMode
{
    public class SpriteStateAnimatorTests
    {
        GameObject go;
        SpriteRenderer renderer;
        SpriteStateAnimator animator;
        SpriteAnimationSet set;
        Texture2D texture;

        [SetUp]
        public void SetUp()
        {
            texture = new Texture2D(4, 4);
            set = ScriptableObject.CreateInstance<SpriteAnimationSet>();
            set.clips = new[]
            {
                Clip("idle", 2, true),
                Clip("move", 3, true),
                Clip("attack_sword", 3, false),
                Clip("hurt", 2, false),
                Clip("dead", 2, false)
            };
            go = new GameObject("Hero");
            renderer = go.AddComponent<SpriteRenderer>();
            go.SetActive(false);
            animator = go.AddComponent<SpriteStateAnimator>();
            animator.animations = set;
            animator.target = renderer;
            go.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(set);
            Object.DestroyImmediate(texture);
        }

        SpriteClip Clip(string id, int count, bool loop)
        {
            var frames = new Sprite[count];
            for (var i = 0; i < count; i++)
            {
                frames[i] = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.zero);
                frames[i].name = id + "_" + i;
            }
            return new SpriteClip { id = id, frames = frames, framesPerSecond = 1f, loop = loop };
        }

        [Test]
        public void LoopingClip_WrapsAround()
        {
            Assert.AreEqual("idle_0", renderer.sprite.name);
            animator.Tick(1.1f);
            Assert.AreEqual("idle_1", renderer.sprite.name);
            animator.Tick(1f);
            Assert.AreEqual("idle_0", renderer.sprite.name);
        }

        [Test]
        public void OneShot_BlocksStateChangeUntilItEnds()
        {
            animator.Play("attack_sword");
            Assert.IsTrue(animator.IsPlayingOneShot);

            animator.SetState(EntityState.Move);
            animator.Tick(1.1f);
            Assert.AreEqual("attack_sword", animator.CurrentClipId);

            animator.Tick(2f);
            Assert.IsFalse(animator.IsPlayingOneShot);
            Assert.AreEqual("move", animator.CurrentClipId);
        }

        [Test]
        public void Hurt_InterruptsOneShot()
        {
            animator.Play("attack_sword");
            animator.SetState(EntityState.Hurt);
            Assert.AreEqual("hurt", animator.CurrentClipId);
        }

        [Test]
        public void Dead_IsTerminalAndHoldsLastFrame()
        {
            animator.SetState(EntityState.Dead);
            animator.Tick(5f);
            animator.SetState(EntityState.Idle);
            animator.Play("attack_sword");

            Assert.AreEqual("dead", animator.CurrentClipId);
            Assert.AreEqual("dead_1", renderer.sprite.name);
        }

        [Test]
        public void UnknownClip_WarnsOnceAndKeepsCurrent()
        {
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("holy_water"));
            animator.Play("holy_water");
            animator.Play("holy_water");

            Assert.AreEqual("idle", animator.CurrentClipId);
        }
    }
}

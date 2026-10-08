using System.Collections.Generic;
using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class SpriteStateAnimator : MonoBehaviour
    {
        public SpriteAnimationSet animations;
        public SpriteRenderer target;
        public EntityController controller;

        readonly HashSet<string> warned = new HashSet<string>();
        SpriteClip current;
        EntityState desired = EntityState.Idle;
        float elapsed;
        int frameIndex = -1;
        bool finished;
        bool dead;

        public bool IsPlayingOneShot => current != null && !current.loop && !finished;
        public string CurrentClipId => current != null ? current.id : null;

        void Awake()
        {
            if (controller == null)
                controller = GetComponentInParent<EntityController>();
            if (target == null)
                target = GetComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            dead = false;
            desired = EntityState.Idle;
            StartClip("idle");
        }

        void Update()
        {
            if (controller != null)
                SetState(controller.State);
            Tick(Time.deltaTime);
        }

        public void SetState(EntityState state)
        {
            if (dead || state == EntityState.Attack || state == desired)
                return;

            desired = state;
            switch (state)
            {
                case EntityState.Hurt:
                    StartClip("hurt");
                    break;
                case EntityState.Dead:
                    dead = true;
                    StartClip("dead");
                    break;
                default:
                    if (!IsPlayingOneShot)
                        StartClip(ClipFor(state));
                    break;
            }
        }

        public float SecondsToFrame(string clipId, int frame)
        {
            var clip = animations != null ? animations.Find(clipId) : null;
            if (clip == null || clip.framesPerSecond <= 0f)
                return 0f;
            return frame / clip.framesPerSecond;
        }

        public void Play(string clipId)
        {
            if (dead)
                return;
            StartClip(clipId);
        }

        public void Tick(float deltaTime)
        {
            if (current == null || finished)
                return;

            elapsed += deltaTime;
            var frames = current.frames;
            var index = (int)(elapsed * current.framesPerSecond);

            if (index >= frames.Length)
            {
                if (current.loop)
                {
                    index %= frames.Length;
                }
                else
                {
                    finished = true;
                    ShowFrame(frames.Length - 1);
                    ResumeDesired();
                    return;
                }
            }
            ShowFrame(index);
        }

        void ResumeDesired()
        {
            if (dead || desired == EntityState.Hurt || desired == EntityState.Attack)
                return;
            StartClip(ClipFor(desired));
        }

        static string ClipFor(EntityState state)
        {
            return state == EntityState.Move ? "move" : "idle";
        }

        void StartClip(string clipId)
        {
            var clip = animations != null ? animations.Find(clipId) : null;
            if (clip == null || clip.frames == null || clip.frames.Length == 0)
            {
                if (warned.Add(clipId))
                    Debug.LogWarning($"SpriteStateAnimator: clip '{clipId}' no existe en {name}", this);
                return;
            }

            current = clip;
            elapsed = 0f;
            finished = false;
            frameIndex = -1;
            ShowFrame(0);
        }

        void ShowFrame(int index)
        {
            if (index == frameIndex || target == null)
                return;
            frameIndex = index;
            target.sprite = current.frames[index];
        }
    }
}

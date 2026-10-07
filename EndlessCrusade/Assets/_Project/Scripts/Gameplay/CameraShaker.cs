using EC.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace EC.Gameplay
{
    public class CameraShaker : CinemachineExtension
    {
        float intensity;
        float duration;
        float remaining;

        public static float Amplitude(float intensity, float remaining, float duration)
        {
            if (duration <= 0f || remaining <= 0f)
                return 0f;
            return intensity * Mathf.Clamp01(remaining / duration);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventBus<CameraShakeRequested>.Subscribe(OnShake);
        }

        void OnDisable()
        {
            EventBus<CameraShakeRequested>.Unsubscribe(OnShake);
        }

        void OnShake(CameraShakeRequested evt)
        {
            if (evt.Intensity < Amplitude(intensity, remaining, duration))
                return;
            intensity = evt.Intensity;
            duration = evt.Duration;
            remaining = evt.Duration;
        }

        void Update()
        {
            if (remaining > 0f)
                remaining -= Time.deltaTime;
        }

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Finalize)
                return;
            var amplitude = Amplitude(intensity, remaining, duration);
            if (amplitude <= 0f)
                return;
            var offset = Random.insideUnitCircle * amplitude;
            state.PositionCorrection += new Vector3(offset.x, offset.y, 0f);
        }
    }
}

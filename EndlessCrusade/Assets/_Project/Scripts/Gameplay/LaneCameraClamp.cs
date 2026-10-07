using EC.Data;
using Unity.Cinemachine;
using UnityEngine;

namespace EC.Gameplay
{
    [ExecuteAlways]
    public class LaneCameraClamp : CinemachineExtension
    {
        public LaneConfig config;

        public static float MaxCenterX(float laneHalfLength, float orthoSize, float aspect)
        {
            return Mathf.Max(0f, laneHalfLength - orthoSize * aspect);
        }

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Body || config == null)
                return;

            var max = MaxCenterX(config.laneHalfLength, state.Lens.OrthographicSize, state.Lens.Aspect);
            var position = state.RawPosition;
            position.x = Mathf.Clamp(position.x, -max, max);
            state.RawPosition = position;
        }
    }
}

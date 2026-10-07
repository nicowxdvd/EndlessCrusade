using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    [RequireComponent(typeof(EntityController))]
    public class MovementComponent : MonoBehaviour
    {
        public LaneConfig lane;
        public float moveSpeed = 2f;

        public float Direction { get; set; }

        EntityController controller;

        void Awake()
        {
            controller = GetComponent<EntityController>();
        }

        void Update()
        {
            if (controller.State != EntityState.Move)
                return;
            Step(Direction, Time.deltaTime);
        }

        public void Step(float direction, float deltaTime)
        {
            Step(direction, deltaTime, moveSpeed);
        }

        public void Step(float direction, float deltaTime, float speed)
        {
            var position = transform.position;
            position.x = ClampX(position.x + Mathf.Sign(direction) * speed * deltaTime, lane);
            transform.position = position;
        }

        public static float ClampX(float x, LaneConfig lane)
        {
            if (lane == null)
                return x;
            return Mathf.Clamp(x, -lane.laneHalfLength, lane.laneHalfLength);
        }
    }
}

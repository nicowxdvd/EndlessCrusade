using System;
using UnityEngine;

namespace EC.UI
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PachinkoBall : MonoBehaviour
    {
        public float lockHeight = 2f;
        public float guideSpeed = 6f;
        public float timeout = 12f;

        Rigidbody2D body;
        float targetX;
        float bottomY;
        float age;
        bool active;

        public event Action<PachinkoBall> Landed;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 start, float slotX, float slotY)
        {
            transform.position = start;
            body.position = start;
            body.linearVelocity = Vector2.zero;
            targetX = slotX;
            bottomY = slotY;
            age = 0f;
            active = true;
        }

        public static float Steer(float x, float y, float targetX, float bottomY, float lockHeight, float maxSpeed, float deltaTime)
        {
            if (y > bottomY + lockHeight)
                return x;
            return Mathf.MoveTowards(x, targetX, maxSpeed * deltaTime);
        }

        void FixedUpdate()
        {
            if (!active)
                return;

            age += Time.fixedDeltaTime;
            var position = body.position;
            if (age >= timeout)
            {
                position = new Vector2(targetX, bottomY);
                body.position = position;
                Land();
                return;
            }

            position.x = Steer(position.x, position.y, targetX, bottomY, lockHeight, guideSpeed, Time.fixedDeltaTime);
            body.position = position;
            if (position.y <= bottomY && Mathf.Abs(position.x - targetX) < 0.05f)
                Land();
        }

        void Land()
        {
            active = false;
            body.linearVelocity = Vector2.zero;
            Landed?.Invoke(this);
        }
    }
}

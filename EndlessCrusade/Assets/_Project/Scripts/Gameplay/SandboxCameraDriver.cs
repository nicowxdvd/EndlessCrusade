using UnityEngine;
using UnityEngine.InputSystem;

namespace EC.Gameplay
{
    public class SandboxCameraDriver : MonoBehaviour
    {
        public float speed = 10f;

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            var direction = 0f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) direction += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) direction -= 1f;
            transform.position += Vector3.right * (direction * speed * Time.deltaTime);
        }
    }
}

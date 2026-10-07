using UnityEngine;

namespace EC.Gameplay
{
    public class ParallaxLayer : MonoBehaviour
    {
        [Range(0f, 1f)] public float factor = 0.5f;

        Camera mainCamera;
        float lastCameraX;

        void OnEnable()
        {
            mainCamera = null;
        }

        void LateUpdate()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                    return;
                lastCameraX = mainCamera.transform.position.x;
            }

            var cameraX = mainCamera.transform.position.x;
            var position = transform.position;
            position.x += (cameraX - lastCameraX) * factor;
            transform.position = position;
            lastCameraX = cameraX;
        }
    }
}

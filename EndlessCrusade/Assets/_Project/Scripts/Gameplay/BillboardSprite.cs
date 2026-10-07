using UnityEngine;

namespace EC.Gameplay
{
    public class BillboardSprite : MonoBehaviour
    {
        Camera mainCamera;

        void LateUpdate()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            transform.rotation = mainCamera.transform.rotation;
        }
    }
}

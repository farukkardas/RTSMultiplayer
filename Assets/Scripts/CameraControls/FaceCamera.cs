using UnityEngine;

namespace CameraControls
{
    public class FaceCamera : MonoBehaviour
    {
        private Transform _mainCameraTransform;

        private void Start()
        {
            _mainCameraTransform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            var rotation = _mainCameraTransform.rotation;
            
            transform.LookAt(
                transform.position + rotation * Vector3.forward,
                rotation * Vector3.up
            );
        }
    }
}
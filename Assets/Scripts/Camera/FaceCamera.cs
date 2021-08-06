using System;
using UnityEngine;

namespace Camera
{
    public class FaceCamera : MonoBehaviour
    {
        private Transform _mainCameraTransform;

        private void Start()
        {
            _mainCameraTransform = UnityEngine.Camera.main.transform;
        }

        private void LateUpdate()
        {
            transform.LookAt(
                transform.position + _mainCameraTransform.rotation * Vector3.forward,
                _mainCameraTransform.rotation * Vector3.up
            );
        }
    }
}
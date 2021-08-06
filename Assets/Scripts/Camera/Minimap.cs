using System;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Camera
{
    public class Minimap : MonoBehaviour , IPointerDownHandler, IDragHandler
    {
        [SerializeField] private RectTransform minimapRect = null;
        [SerializeField] private float mapScale = 20f;
        [SerializeField] private float offset = -6f;
        private Transform _playerCameraTransform;
        private IDragHandler _dragHandlerImplementation;

        private void Update()
        {
            if (_playerCameraTransform != null)
            {
                return;
            }

            if (NetworkClient.connection.identity == null)
            {
                return;
            }

            _playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform();
        }

        private void MoveCamera()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect,
                mousePos, null, out Vector2 localPoint
            ))
            {
                return;
            }


            var rect = minimapRect.rect;
            Vector2 lerp = new Vector2((localPoint.x - rect.x) / rect.width,
                localPoint.y - rect.y / rect.height);

            Vector3 newCameraPos = new Vector3(Mathf.Lerp(-mapScale, mapScale, lerp.x),
                _playerCameraTransform.position.y,
                Mathf.Lerp(-mapScale, mapScale, lerp.y));

            _playerCameraTransform.position = newCameraPos + new Vector3(0f,0f,offset);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            MoveCamera();
        }

        public void OnDrag(PointerEventData eventData)
        {
            MoveCamera();
        }
    }
}
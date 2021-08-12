using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

namespace Buildings
{
    public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Building building = null;
        [SerializeField] private Image iconImage = null;
        [SerializeField] private TMP_Text priceText = null;
        [SerializeField] private LayerMask floorMask = new LayerMask();


        private BoxCollider _buildingCollider;
        private Camera _mainCamera;
        private RTSPlayer _player;
        private GameObject _buildingPreviewInstance;
        private Renderer _buildingRendererInstance;

        


        private void Start()
        {
            _mainCamera = Camera.main;

            iconImage.sprite = building.GetIcon();
            priceText.text = building.GetPrice().ToString();
            _buildingCollider = building.GetComponent<BoxCollider>();
            _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        }

        private void Update()
        {
          

            if (_buildingPreviewInstance == null)
            {
                return;
            }

            UpdateBuildingPreview();
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (_player.GetResources() < building.GetPrice())
            {
                return;
            }

            _buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
            _buildingRendererInstance = _buildingPreviewInstance.GetComponentInChildren<Renderer>();
            _buildingPreviewInstance.SetActive(false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_buildingPreviewInstance == null)
            {
                return;
            }

            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            {
                _player.CmdTryPlaceBuilding(building.GetId(), hit.point);
            }


            Destroy(_buildingPreviewInstance);
        }

        private void UpdateBuildingPreview()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            {
                return;
            }

            _buildingPreviewInstance.transform.position = hit.point;

            if (!_buildingPreviewInstance.activeSelf)
            {
                _buildingPreviewInstance.SetActive(true);
            }


            Color color = _player.CanPlaceBuilding(_buildingCollider, hit.point) ? Color.green : Color.red;

            _buildingRendererInstance.material.SetColor("_BaseColor", color);
        }
    }
}
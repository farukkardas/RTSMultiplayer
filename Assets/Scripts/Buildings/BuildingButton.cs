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
        
        
        private BoxCollider buildingCollider;
        private UnityEngine.Camera mainCamera;
        private RTSPlayer player;
        private GameObject buildingPreviewInstance;
        private Renderer buildingRendererInstance;
        //private IPointerUpHandler _pointerUpHandlerImplementation;

        private void Start()
        {
            mainCamera = UnityEngine.Camera.main;

            iconImage.sprite = building.GetIcon();
            priceText.text = building.GetPrice().ToString();

            buildingCollider = building.GetComponent<BoxCollider>();
        }

        private void Update()
        {
            if (player == null)
            {
                player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            }

            if (buildingPreviewInstance == null)
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

            if (player.GetResources() < building.GetPrice())
            {
                return;
            }

            buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
            buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();
            buildingPreviewInstance.SetActive(false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (buildingPreviewInstance == null)
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            {
                player.CmdTryPlaceBuilding(building.GetId(), hit.point);
            }


            Destroy(buildingPreviewInstance);
        }

        private void UpdateBuildingPreview()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            {
                return;
            }

            buildingPreviewInstance.transform.position = hit.point;

            if (!buildingPreviewInstance.activeSelf)
            {
                buildingPreviewInstance.SetActive(true);
            }

            
            Color color = player.CanPlaceBuilding(buildingCollider,hit.point) ? Color.green : Color.red;
            
            buildingRendererInstance.material.SetColor("_BaseColor",color);
        }
    }
}
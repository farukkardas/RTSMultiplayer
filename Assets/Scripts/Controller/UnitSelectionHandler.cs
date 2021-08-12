using System;
using System.Collections.Generic;
using Buildings;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Controller
{
    public class UnitSelectionHandler : NetworkBehaviour
    {
        [SerializeField] private RectTransform unitSelectionArea = null;
        [SerializeField] private LayerMask layerMask = new LayerMask();
        private UnityEngine.Camera _mainCamera;
        private RTSPlayer _player;

        public List<UnitSelection> selectedUnits = new List<UnitSelection>();

        private Vector2 _startPos;


        private void Start()
        {
            _mainCamera = UnityEngine.Camera.main;
            UnitSelection.AuthorityOnUnitDeSpawned += AuthorityHandleUnitDeSpawned;
            GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
            _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        }

        private void OnDestroy()
        {
            UnitSelection.AuthorityOnUnitDeSpawned -= AuthorityHandleUnitDeSpawned;
            GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
        }

        private void ClientHandleGameOver(string winnerName)
        {
            enabled = false;
        }
      

        private void AuthorityHandleUnitDeSpawned(UnitSelection unit)
        {
            selectedUnits.Remove(unit);
        }

      
        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                StartSelectionArea();
            }

            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                ClearSelectionArea();
            }

            else if (Mouse.current.leftButton.isPressed)
            {
                UpdateSelectionArea();
            }
        }

        private void StartSelectionArea()
        {
            if (!Keyboard.current.leftShiftKey.isPressed)
            {
                foreach (UnitSelection selected in selectedUnits)
                {
                    selected.DeSelect();
                }

                selectedUnits.Clear();
            }
            unitSelectionArea.gameObject.SetActive(true);

            _startPos = Mouse.current.position.ReadValue();
            
            UpdateSelectionArea();
        }

        private void UpdateSelectionArea()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            float areaWidth = mousePosition.x - _startPos.x;

            float areaHeight = mousePosition.y - _startPos.y;

            unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
            unitSelectionArea.anchoredPosition = _startPos 
                                                 + new Vector2(areaWidth / 2, areaHeight / 2);
            
            
        }

        private void ClearSelectionArea()
        {
            unitSelectionArea.gameObject.SetActive(false);

            if (unitSelectionArea.sizeDelta.magnitude == 0)
            {
                Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    return;
                }

                if (!hit.collider.TryGetComponent<UnitSelection>(out UnitSelection unit))
                {
                    return;
                }

                if (!unit.hasAuthority)
                {
                    return;
                }

                selectedUnits.Add(unit);

                foreach (UnitSelection selected in selectedUnits)
                {
                    selected.Select();
                }
                
                return;
            }

            Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);

            Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

            foreach (UnitSelection unit in _player.GetMyUnits())
            {
                if(selectedUnits.Contains(unit)){continue;}
                
                Vector3 screenPos = _mainCamera.WorldToScreenPoint(unit.transform.position);

              //  if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
                //{
                    selectedUnits.Add(unit);
                    unit.Select();
                //}
            }
        }
        
    }
}
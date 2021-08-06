using System;
using System.Collections.Generic;
using Buildings;
using Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    public class UnitCommandGiver : MonoBehaviour
    {
        [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
        [SerializeField] private LayerMask layerMask = new LayerMask();
        private UnityEngine.Camera _mainCamera;
        private void Start()
        {
            _mainCamera = UnityEngine.Camera.main;

            GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
        }

        private void OnDestroy()
        {
            GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
        }

        private void ClientHandleGameOver(string winnerName)
        {
            enabled = false;
        }

        private void Update()
        {
            if(!Mouse.current.rightButton.wasPressedThisFrame){return;}

            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if(!Physics.Raycast(ray,out RaycastHit hit,Mathf.Infinity,layerMask)) {return;}

            if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
            {
                if (target.hasAuthority)
                {
                    TryMove(hit.point);
                    return;
                }

                TryTarget(target);
                return;
            }
            
            TryMove(hit.point);
            
        }
        
        private void TryTarget(Targetable target)
        {
            foreach (UnitSelection unit in unitSelectionHandler.selectedUnits)
            {
                unit.GetTargeter().CmdSetTarget(target.gameObject);
            }
        }
        private void TryMove(Vector3 point)
        {
            foreach (UnitSelection unit in unitSelectionHandler.selectedUnits)
            {
                unit.GetUnitMovement().CmdMove(point);
            }
        }
    }
}

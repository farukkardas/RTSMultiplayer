
using System;
using Controller;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

namespace Buildings
{
    public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
    {
        [SerializeField] private Health health = null;
        [SerializeField] private UnitSelection unitPrefab = null;
        [SerializeField] private Transform unitSpawnPoint = null;
        [SerializeField] private TMP_Text remainingUnitsText = null;
        [SerializeField] private Image unitProgressImage = null;
        [SerializeField] private int maxUnitQueue = 5;
        [SerializeField] private float spawnMoveRange = 7f;
        [SerializeField] private float unitSpawnDuration = 5f;

        [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
        private int _queuedUnits;

        private float _progressImageVelocity;

      

        private void Update()
        {
            if (isServer)
            {
                ProduceUnits();
            }

            if (isClient)
            {
                UpdateTimerDisplay();
            }
        }

      

        [SyncVar] private float _unitTimer;

        #region Server

        public override void OnStartServer()
        {
            health.ServerOnDie += ServerHandleDie;
        }

        public override void OnStopServer()
        {
            health.ServerOnDie -= ServerHandleDie;
        }

        [Server]
        private void ServerHandleDie()
        {
             NetworkServer.Destroy(gameObject);
        }

        [Command]
        private void CmdSpawnUnit()
        {
         if(_queuedUnits == maxUnitQueue) {return;}

           RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

           if(player.GetResources() < unitPrefab.GetResourceCost()) {return;}

           _queuedUnits++;
           
           player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());
        }

        [Server]
        private void ProduceUnits()
        {
           if(_queuedUnits == 0) {return;}

           _unitTimer += Time.deltaTime;
           
           if(_unitTimer < unitSpawnDuration) {return;}
           
           GameObject unitInstance = Instantiate(unitPrefab.gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);

           NetworkServer.Spawn(unitInstance.gameObject, connectionToClient);

           Vector3 spawnOffset = UnityEngine.Random.insideUnitSphere * spawnMoveRange;

           spawnOffset.y = unitSpawnPoint.position.y;

           UnitController unitController = unitInstance.GetComponent<UnitController>();
           
           unitController.ServerMove(unitSpawnPoint.position + spawnOffset);

           _queuedUnits--;
           _unitTimer = 0f;
        }
        #endregion


        #region Client

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!hasAuthority)
            {
                return;
            }

            CmdSpawnUnit();
        }

        private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
        {
            remainingUnitsText.text = newUnits.ToString();
        }
        
        private void UpdateTimerDisplay()
        {
            float newProgress = _unitTimer / unitSpawnDuration;

            if (newProgress < unitProgressImage.fillAmount)
            {
                unitProgressImage.fillAmount = newProgress;
            }

            else
            {
                unitProgressImage.fillAmount = Mathf.SmoothDamp(
                    unitProgressImage.fillAmount,newProgress, ref _progressImageVelocity,0.1f);
            }
        }


        #endregion
    }
}
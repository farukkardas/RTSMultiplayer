using System;
using Combat;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Controller
{
    public class UnitSelection : NetworkBehaviour
    {
        [SerializeField] private int resourceCost = 10;
        [SerializeField] private Targeter targeter = null;
        [SerializeField] private UnitController unitController = null;
        [SerializeField] private UnityEvent onSelected = null;
        [SerializeField] private UnityEvent onDeselected = null;
        [SerializeField] private Health health = null;
        
        public static event Action<UnitSelection> ServerOnUnitSpawned;
        public static event Action<UnitSelection> ServerOnUnitDeSpawned;
        
        public static event Action<UnitSelection> AuthorityOnUnitSpawned;
        public static event Action<UnitSelection> AuthorityOnUnitDeSpawned;

        public UnitController GetUnitMovement()
        {
            return unitController;
        }
        
        public int GetResourceCost()
        {
            return resourceCost;
        }

        public Targeter GetTargeter()
        {
            return targeter;
        }

        #region Server

        public override void OnStartServer()
        {
           ServerOnUnitSpawned?.Invoke(this);
           health.ServerOnDie += ServerHandleDie;
        }

        public override void OnStopClient()
        {
           ServerOnUnitDeSpawned?.Invoke(this); 
           health.ServerOnDie -= ServerHandleDie;
        }
        
        [Server]
        private void ServerHandleDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        #endregion
        
        #region Client--


        [Client]

        public override void OnStartAuthority()
        {
            if(!hasAuthority){return;}
            AuthorityOnUnitSpawned?.Invoke(this);
        }

        public override void OnStopAuthority()
        {
            if(!hasAuthority){return;}
            AuthorityOnUnitDeSpawned?.Invoke(this);
        }
        public void Select()
        {
            
            if(!hasAuthority){return;}
            onSelected?.Invoke();
        }

        [Client]
        public void DeSelect()
        {
            if(!hasAuthority) {return;}
           onDeselected?.Invoke();
        }

        #endregion
    }
}

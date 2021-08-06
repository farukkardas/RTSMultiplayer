using System;
using Mirror;
using UnityEngine;

namespace Buildings
{
    public class UnitBase : NetworkBehaviour
    {
        [SerializeField] private Health health = null;

        public static event Action<int> ServerOnPlayerDie; 
        public static event Action<UnitBase> ServerOnBaseSpawned;
        public static event Action<UnitBase> ServerOnBaseDeSpawned;


        #region Server

        public override void OnStartServer()
        {
            health.ServerOnDie += HandleServerDie;
            ServerOnBaseSpawned?.Invoke(this);
        }


        public override void OnStopServer()
        {
            health.ServerOnDie -= HandleServerDie;
            ServerOnBaseDeSpawned?.Invoke(this);
        }

        [Server]
        private void HandleServerDie()
        {
            ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);
            NetworkServer.Destroy(gameObject);
        }

        #endregion

        #region Client

        #endregion
    }
}
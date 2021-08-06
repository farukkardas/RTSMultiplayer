using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Buildings
{
    public class GameOverHandler : NetworkBehaviour
    {
        public static event Action ServerOnGameOver;
        public static event Action<string> ClientOnGameOver;
        private List<UnitBase> _bases = new List<UnitBase>();

        #region Server

        public override void OnStartServer()
        {
            UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
            UnitBase.ServerOnBaseDeSpawned += ServerHandleBaseDeSpawned;
        }

        public override void OnStopServer()
        {
            UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
            UnitBase.ServerOnBaseDeSpawned -= ServerHandleBaseDeSpawned;
        }

        [Server]
        private void ServerHandleBaseSpawned(UnitBase unitBase)
        {
            _bases.Add(unitBase);
        }

        [Server]
        private void ServerHandleBaseDeSpawned(UnitBase unitBase)
        {
            _bases.Remove(unitBase);

            if (_bases.Count != 1)
            {
                return;
            }

            int playerId = _bases[0].connectionToClient.connectionId;
            
            RpcGameOver($"Player{playerId}");
            ServerOnGameOver?.Invoke();
        }

        #endregion

        #region Client

        [ClientRpc]
        private void RpcGameOver(string winner)
        {
            ClientOnGameOver?.Invoke(winner);
        }
        

        #endregion
        
    }
}
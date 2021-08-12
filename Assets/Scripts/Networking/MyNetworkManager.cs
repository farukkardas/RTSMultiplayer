using System;
using System.Collections.Generic;
using Buildings;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Networking
{
    public class MyNetworkManager : NetworkManager
    {
        [SerializeField] private GameObject unitBasePrefab = null;
        [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

        public static event Action ClientOnConnected;
        public static event Action ClientOnDissconnected;

        private bool _isGameInProgress = false;

        public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

        #region Server

        public override void OnServerConnect(NetworkConnection conn)
        {
            if (!_isGameInProgress)
            {
                return;
            }

            conn.Disconnect();
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);

            RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

            Players.Remove(player);
        }

        public override void OnStopServer()
        {
            Players.Clear();

            _isGameInProgress = false;
        }

        public void StartGame()
        {
            if (Players.Count < 2)
            {
                return;
            }

            _isGameInProgress = true;

            ServerChangeScene("Scene_Map_01");
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            base.OnServerAddPlayer(conn);

            RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

            Players.Add(player);
            
            player.SetDisplayName($"Player {Players.Count}");

            player.SetTeamColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
            
            player.SetPartyOwner(Players.Count == 1);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
            {
                GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

                NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

                foreach (RTSPlayer player in Players)
                {
                    GameObject baseInstance =
                        Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);
                    
                    NetworkServer.Spawn(baseInstance,player.connectionToClient);
                }
            }
        }

        #endregion


        #region Client

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            ClientOnConnected?.Invoke();
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);

            ClientOnDissconnected?.Invoke();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            
            Players.Clear();
        }

        #endregion
    }
}
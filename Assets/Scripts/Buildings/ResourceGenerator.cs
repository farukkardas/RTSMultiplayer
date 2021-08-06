using System;
using Mirror;
using UnityEngine;

namespace Buildings
{
    public class ResourceGenerator : NetworkBehaviour
    {
        [SerializeField] private Health health = null;
        [SerializeField] private int resourcesPerInterval = 5;
        [SerializeField] private float interval = 2f;

        private float _timer;
        private RTSPlayer _player;


        public override void OnStartServer()
        {
            _timer = interval;
            _player = connectionToClient.identity.GetComponent<RTSPlayer>();

            health.ServerOnDie += ServerHandleDie;
            GameOverHandler.ServerOnGameOver += ServeHandleGameOver;
        }

        public override void OnStopServer()
        {
            health.ServerOnDie -= ServerHandleDie;
            GameOverHandler.ServerOnGameOver -= ServeHandleGameOver;
        }

        [ServerCallback]
        private void Update()
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0)
            {
                _player.SetResources(_player.GetResources() + resourcesPerInterval);
                _timer += interval;
            }
            
            
        }

        private void ServerHandleDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        private void ServeHandleGameOver()
        {
            enabled = false;
        }
    }
}
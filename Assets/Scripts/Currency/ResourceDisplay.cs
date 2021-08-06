using System;
using Mirror;
using TMPro;
using UnityEngine;

namespace Currency
{
    public class ResourceDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text goldAmount = null;
        private RTSPlayer _player;
        private bool _isPlayerNull;
        private RTSPlayer _rtsPlayer;


        private void Update()
        {
            if (_player == null)
            {
                _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            }

            if (_player != null)
            {
                ClientHandleResourcesUpdated(_player.GetResources());
                _player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
            }
        }

        private void OnDestroy()
        {
            _player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
        }

        private void ClientHandleResourcesUpdated(int amount)
        {
            goldAmount.text = $"Resources {amount}";
        }
    }
}

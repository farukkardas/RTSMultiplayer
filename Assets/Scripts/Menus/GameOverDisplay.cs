using System;
using Buildings;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menus
{
    public class GameOverDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverDisplayParent = null;
        [SerializeField] private TMP_Text winnerNameText = null;
        
        private void Start()
        {
            GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
        }
        
        private void OnDestroy()
        {
            GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
        }

        public void LeaveGame()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();
            }

            else
            {
                NetworkManager.singleton.StopClient();
            }
        }
        
        private void ClientHandleGameOver(string winner)
        {
            winnerNameText.text = $"{winner} has won!";
            
            gameOverDisplayParent.SetActive(true);
        }
       
    }
}

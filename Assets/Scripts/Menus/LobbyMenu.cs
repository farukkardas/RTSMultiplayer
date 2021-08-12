using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGameButton = null;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text startInfo = null;


    private void Start()
    {
        MyNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
        startInfo.enabled = false;
    }


    private void OnDestroy()
    {
        MyNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        startGameButton.gameObject.SetActive(state);
    }

    public void StartGame()
    {
        List<RTSPlayer> _playersList = ((MyNetworkManager) NetworkManager.singleton).Players;

        if (_playersList.Count < 2)
        {
            StartCoroutine(StartInfoText());
        }
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
    }

    public IEnumerator StartInfoText()
    {
        startInfo.enabled = true;
        startGameButton.interactable = false;
        yield return new WaitForSeconds(2f);
        startGameButton.interactable = true;
        startInfo.enabled = false;
    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    private void ClientHandleInfoUpdated()
    {
    
        List<RTSPlayer> _playersList = ((MyNetworkManager) NetworkManager.singleton).Players;

        for (int i = 0; i < _playersList.Count; i++)
        {
            playerNameTexts[i].text = _playersList[i].GetDisplayName();
        }

        for (int i = _playersList.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting for player...";
        }
        
    }

    public void LeaveLobby()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }

        else
        {
            NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(0);
        }   
    }
}
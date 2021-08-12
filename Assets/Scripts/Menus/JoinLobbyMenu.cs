using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
   [SerializeField] private GameObject landingPagePanel = null;
   [SerializeField] private TMP_InputField adressInput = null;
   [SerializeField] private Button joinButton = null;

   private void OnEnable()
   {
      MyNetworkManager.ClientOnConnected += HandleClientConnected;
      MyNetworkManager.ClientOnDissconnected += HandleClientDisconnected;
   }

   private void OnDisable()
   {
      MyNetworkManager.ClientOnConnected -= HandleClientConnected;
      MyNetworkManager.ClientOnDissconnected -= HandleClientDisconnected;
   }

   public void Join()
   {
      string adress = adressInput.text;

      NetworkManager.singleton.networkAddress = adress;
      NetworkManager.singleton.StartClient();

      joinButton.interactable = false;
   }

   private void HandleClientConnected()
   {
      joinButton.interactable = true;
      
      gameObject.SetActive(false);
      landingPagePanel.SetActive(false);
   }

   private void HandleClientDisconnected()
   {
      joinButton.interactable = true;

   }
}

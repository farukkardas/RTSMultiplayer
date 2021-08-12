using System;
using System.Collections.Generic;
using Buildings;
using Controller;
using Mirror;
using Networking;
using TMPro;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform playerCamTransform = null;
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private float buildingRangeLimit = 5f;
    [SerializeField] private GameObject chatUI = null;
    [SerializeField] private TMP_Text chatText = null;
    [SerializeField] private TMP_InputField inputField = null;

    private Color _teamColor = new Color();
    private List<UnitSelection> _myUnits = new List<UnitSelection>();
    private List<Building> _myBuildings = new List<Building>();

    private static event Action<string> OnMessage;


    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int _gold = 100;

    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;

    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string displayName;

    public event Action<int> ClientOnResourcesUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

    public static event Action ClientOnInfoUpdated;

    public string GetDisplayName()
    {
        return displayName;
    }

    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }

    public Transform GetCameraTransform()
    {
        return playerCamTransform;
    }

    public Color GetTeamColor()
    {
        return _teamColor;
    }

    public List<UnitSelection> GetMyUnits()
    {
        return _myUnits;
    }

    public List<Building> GetMyBuildings()
    {
        return _myBuildings;
    }

    public int GetResources()
    {
        return _gold;
    }


    #region Server

    public override void OnStartServer()
    {
        UnitSelection.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        UnitSelection.ServerOnUnitDeSpawned += ServerHandleUnitDeSpawned;
        Building.ServerOnBuildingDeSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;

        chatUI.SetActive(true);
        OnMessage += HandleNewMessage;


        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer()
    {
        UnitSelection.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        UnitSelection.ServerOnUnitDeSpawned -= ServerHandleUnitDeSpawned;
    }

    private void ServerHandleUnitSpawned(UnitSelection unitSelection)
    {
        if (unitSelection.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }

        _myUnits.Add(unitSelection);
    }

    private void ServerHandleUnitDeSpawned(UnitSelection unitSelection)
    {
        _myUnits.Remove(unitSelection);
    }


    private void ServerHandleBuildingSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }

        _myBuildings.Add(building);
    }

    private void ServerHandleBuildingDeSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }

        _myBuildings.Remove(building);
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 pointLocation)
    {
        Building buildingToPlace = null;

        foreach (Building building in buildings)
        {
            if (building.GetId() == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }

        if (buildingToPlace == null)
        {
            return;
        }

        if (_gold < buildingToPlace.GetPrice())
        {
            return;
        }

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();


        if (!CanPlaceBuilding(buildingCollider, pointLocation))
        {
            return;
        }

        GameObject buildingInstance =
            Instantiate(buildingToPlace.gameObject, pointLocation, buildingToPlace.transform.rotation);

        NetworkServer.Spawn(buildingInstance, connectionToClient);

        SetResources(_gold - buildingToPlace.GetPrice());
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server]
    public void SetResources(int newGold)
    {
        _gold = newGold;
    }

    [Server]
    public void SetPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    [Server]
    public void SetTeamColor(Color newTeamColor)
    {
        _teamColor = newTeamColor;
    }

    [Command]
    public void CmdStartGame()
    {
        if (!isPartyOwner)
        {
            return;
        }

        ((MyNetworkManager) NetworkManager.singleton).StartGame();
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        if (NetworkServer.active)
        {
            return;
        }

        chatUI.SetActive(true);

        OnMessage += HandleNewMessage;

        UnitSelection.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        UnitSelection.AuthorityOnUnitDeSpawned += AuthorityHandleUnitDeSpawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingDeSpawned;
    }

    [ClientCallback]
    private void OnDestroy()
    {
        if (!hasAuthority)
        {
            return;
        }

        OnMessage -= HandleNewMessage;
    }

    private void HandleNewMessage(string message)
    {
        chatText.text += message;
    }

   [Client]
    public void Send(string message)
    {
        if (!Input.GetKeyDown(KeyCode.Return))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(message))
        {   
            return;
        }

        CmdSendMessage(message);
        inputField.text = string.Empty;
    }


    [Command]
    private void CmdSendMessage(string message)
    {
        if (!hasAuthority)
        {
            chatUI.SetActive(false);
        }   
        
        //validation rules  ( spamı burada önleyeceksin)
        RpcHandleMessage($"[{1}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        OnMessage?.Invoke($"\n{message}");
    }


    public override void OnStartClient()
    {
        base.OnStartClient();

        if (NetworkServer.active)
        {
            return;
        }


        DontDestroyOnLoad(gameObject);

        ((MyNetworkManager) NetworkManager.singleton).Players.Add(this);
    }

    public override void OnStopClient()
    {
        ClientOnInfoUpdated?.Invoke();

        if (!isClientOnly)
        {
            return;
        }

        ((MyNetworkManager) NetworkManager.singleton).Players.Remove(this);

        if (!hasAuthority)
        {
            return;
        }


        UnitSelection.ServerOnUnitSpawned -= AuthorityHandleUnitSpawned;
        UnitSelection.ServerOnUnitDeSpawned -= AuthorityHandleUnitDeSpawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingDeSpawned;
    }


    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if (!hasAuthority)
        {
            return;
        }

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void AuthorityHandleUnitSpawned(UnitSelection unitSelection)
    {
        _myUnits.Add(unitSelection);
    }

    private void AuthorityHandleUnitDeSpawned(UnitSelection unitSelection)
    {
        _myUnits.Remove(unitSelection);
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        _myBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDeSpawned(Building building)
    {
        _myBuildings.Remove(building);
    }

    private void ClientHandleResourcesUpdated(int oldGold, int newGold)
    {
        ClientOnResourcesUpdated?.Invoke(newGold);
    }

    private void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
    {
        if (Physics.CheckBox(point + buildingCollider.center, buildingCollider.size / 2, Quaternion.identity,
            buildingBlockLayer))
        {
            return false;
        }


        foreach (Building building in _myBuildings)
        {
            if ((point - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }

        return false;
    }

    #endregion
}
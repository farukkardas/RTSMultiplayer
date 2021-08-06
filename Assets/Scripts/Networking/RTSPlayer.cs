using System;
using System.Collections.Generic;
using Buildings;
using Controller;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform playerCamTransform = null;
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private float buildingRangeLimit = 5f;

    private Color _teamColor = new Color();
    private List<UnitSelection> _myUnits = new List<UnitSelection>();
    private List<Building> _myBuildings = new List<Building>();

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int _gold = 100;

    public event Action<int> ClientOnResourcesUpdated;

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
    public void SetResources(int newGold)
    {
        _gold = newGold;
    }

    [Server]
    public void SetTeamColor(Color newTeamColor)
    {
        _teamColor = newTeamColor;
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        if (NetworkServer.active)
        {
            return;
        }

        UnitSelection.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        UnitSelection.AuthorityOnUnitDeSpawned += AuthorityHandleUnitDeSpawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingDeSpawned;
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority)
        {
            return;
        }

        UnitSelection.ServerOnUnitSpawned -= AuthorityHandleUnitSpawned;
        UnitSelection.ServerOnUnitDeSpawned -= AuthorityHandleUnitDeSpawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingDeSpawned;
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
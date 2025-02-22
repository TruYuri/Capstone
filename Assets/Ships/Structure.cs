﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// Extension of Ship to handle structure behavior.
/// </summary>
public class Structure : Ship, ListableObject
{
    private const string LIST_PREFAB = "ShipListing";
    private List<string> constructables;
    private float defense;
    private int deployedCapacity;
    private int gatherRate;
    private int range;
    private int swapPopulation;
    private int swapCapacity;
    private ResourceGatherType types; // bitfield definition of structure gather types
    private Tile tile;

    public float Defense 
    { 
        get { return defense; }
        set { defense = value; } 
    }
    public int DeployedCapacity
    {
        get { return deployedCapacity; }
        set { deployedCapacity = value; }
    }

    public int GatherRate
    {
        get { return gatherRate; }
        set { gatherRate = value; }
    }

    public int Range
    {
        get { return range; }
        set { range = value; }
    }

    public Tile Tile
    {
        get { return tile; }
        set { tile = value; }
    }

    public List<string> Constructables { get { return constructables; } }

    /// <summary>
    /// Constructor for the Structure class.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="hull"></param>
    /// <param name="firepower"></param>
    /// <param name="speed"></param>
    /// <param name="capacity"></param>
    /// <param name="rCapacity"></param>
    /// <param name="defense"></param>
    /// <param name="deployedCapacity"></param>
    /// <param name="gatherRate"></param>
    /// <param name="range"></param>
    /// <param name="constructables"></param>
    /// <param name="shipProperties"></param>
    /// <param name="type"></param>
    public Structure(string name, float hull, float firepower, float speed, int capacity, int rCapacity,
        float defense, int deployedCapacity, int gatherRate, int range, List<string> constructables, ShipProperties shipProperties, ResourceGatherType type)
        : base(name, hull, firepower, speed, capacity, rCapacity, 0, shipProperties)
    {
        this.defense = defense;
        this.deployedCapacity = deployedCapacity;
        this.gatherRate = gatherRate;
        this.constructables = constructables;
        this.range = range;
        this.swapPopulation = deployedCapacity;
        this.swapCapacity = 99999;
        this.types = type;
    }

    /// <summary>
    /// Recalculate the resources needed to build this structure.
    /// </summary>
    public override void RecalculateResources()
    {
        // find a way to factor this out later - this is just dirty
        if (name == "Relay")
        {
            base.RecalculateResources();
            requiredResources[Resource.Asterminium] = range * 2;
        }
        else if (name == "Warp Portal")
        {
            base.RecalculateResources();
            requiredResources[Resource.Asterminium] = range * 5;
        }
        else
        {
            requiredResources[Resource.Ore] = Mathf.CeilToInt(defense);
            requiredResources[Resource.Oil] = 0;
            requiredResources[Resource.Asterminium] = 0;
            requiredResources[Resource.Forest] = deployedCapacity;
        }
    }

    /// <summary>
    /// Make a copy of this ship. Intended for building.
    /// </summary>
    /// <returns>A new copy of this ship.</returns>
    public override Ship Copy()
    {
        var ship = new Structure(name, hull, firepower, speed, capacity, resourceCapacity,
            defense, deployedCapacity, gatherRate, range, constructables, shipProperties, types);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }

    /// <summary>
    /// Gather resources at this structure's tile.
    /// </summary>
    /// <param name="rType">The resource type of the tile.</param>
    /// <param name="resources">The amount of resources at the tile.</param>
    /// <param name="pType">The tile's population type.</param>
    /// <param name="population">The tile's population.</param>
    /// <param name="team">The tile's team.</param>
    /// <returns>The type of resources gathered, and their amounts gathered.</returns>
    public List<KeyValuePair<ResourceGatherType, int>> Gather(Resource rType, int resources, Inhabitance pType, int population, Team team)
    {
        var gathertypes = new List<KeyValuePair<ResourceGatherType, int>>();
        int min;

        if((types & ResourceGatherType.Natural) > 0)
        {
            min = Math.Min(Mathf.CeilToInt(gatherRate * (team == Team.Plinthen ? 1.15f : 1f)), resources);
            GameManager.Instance.Players[team].AddResources(this, rType, min);
            gathertypes.Add(new KeyValuePair<ResourceGatherType, int>(ResourceGatherType.Natural, min));
        }
        if((types & ResourceGatherType.Soldiers) > 0 && pType != Inhabitance.Uninhabited)
        {
            min = Math.Min(gatherRate, capacity - CountPopulation());
            min = Math.Min(min, population);
            GameManager.Instance.Players[team].AddSoldiers(this, pType, min);
            gathertypes.Add(new KeyValuePair<ResourceGatherType, int>(ResourceGatherType.Soldiers, min));
        }

        return gathertypes;
    }

    /// <summary>
    /// Deploy this structure to a tile.
    /// </summary>
    /// <param name="tile">The tile to deploy to.</param>
    public void Deploy(Tile tile)
    {
        swapPopulation = capacity;
        capacity = deployedCapacity;
        shipProperties |= ShipProperties.Untransferable;
        resourceCapacity = 99999;
        swapCapacity = resourceCapacity;
        isDeployed = true;
        this.tile = tile;
    }

    /// <summary>
    /// Undeploy this structure from a tile.
    /// </summary>
    /// <param name="tile">The tile to undeploy from.</param>
    public void Undeploy(Tile tile)
    {
        capacity = swapPopulation;
        swapPopulation = deployedCapacity;
        shipProperties = shipProperties & (~ShipProperties.Untransferable);
        resourceCapacity = swapCapacity;
        swapCapacity = 99999;
        tile.Population += CountPopulation();
        population = new Dictionary<Inhabitance, int>()
        {
            { Inhabitance.Primitive, 0 },
            { Inhabitance.Industrial, 0 },
            { Inhabitance.SpaceAge, 0 }
        };
        isDeployed = false;
        this.tile = null;
    }

    /// <summary>
    /// Populate a UI panel with a deployed structure's info.
    /// </summary>
    /// <param name="list">The UI panel to populate.</param>
    public void PopulateStructurePanel(GameObject list)
    {
        list.transform.FindChild("StructureIcon").GetComponent<Image>().sprite = GUIManager.Instance.Icons[name];
        list.transform.FindChild("StructureName").GetComponent<Text>().text = name;
        list.transform.FindChild("Capacity").GetComponent<Text>().text = CountPopulation().ToString() + "/" + deployedCapacity.ToString();
        list.transform.FindChild("Defense").GetComponent<Text>().text = defense.ToString();
        list.transform.FindChild("GatherRate").GetComponent<Text>().text = gatherRate.ToString();
        list.transform.FindChild("OilAmount").GetComponent<Text>().text = resources[Resource.Oil].ToString();
        list.transform.FindChild("OreAmount").GetComponent<Text>().text = resources[Resource.Ore].ToString();
        list.transform.FindChild("AsterminiumAmount").GetComponent<Text>().text = resources[Resource.Asterminium].ToString();
        list.transform.FindChild("ForestAmount").GetComponent<Text>().text = resources[Resource.Forest].ToString();
    }

    /// <summary>
    /// Populates a UI element with this ship's info.
    /// </summary>
    /// <param name="listName">The internal name of the list the element belongs to.</param>
    /// <param name="index">The index in the list</param>
    /// <param name="data">Optional data</param>
    /// <returns>A new UI element for a list.</returns>
    GameObject ListableObject.CreateListEntry(string listName, int index, System.Object data)
    {
        var shipEntry = UnityEngine.Resources.Load<GameObject>(LIST_PREFAB);
        var entry = GameObject.Instantiate(shipEntry) as GameObject;
        var icon = entry.transform.FindChild("Icon").GetComponent<Image>();

        var name = this.name;
        icon.sprite = GUIManager.Instance.Icons[name];

        if (HumanPlayer.Instance.Squad.Tile != null && this == HumanPlayer.Instance.Squad.Tile.Structure)
            name = "(Deployed) " + name;
        entry.transform.FindChild("Name").GetComponent<Text>().text = name;
        entry.transform.FindChild("Population").GetComponent<Text>().text = CountPopulation().ToString() + "/" + capacity;
        entry.GetComponent<CustomUIAdvanced>().data = listName + "|" + index.ToString();

        return entry;
    }

    /// <summary>
    /// Populates a pop-up info panel for buildable items.
    /// </summary>
    /// <param name="popUp">The gameobject to populate.</param>
    /// <param name="data">Optional data.</param>
    void ListableObject.PopulateBuildInfo(GameObject popUp, System.Object data)
    {
        var go = popUp; // construction info only
        go.transform.FindChild("HullText").GetComponent<Text>().text = hull.ToString();
        go.transform.FindChild("FirepowerText").GetComponent<Text>().text = firepower.ToString();
        go.transform.FindChild("SpeedText").GetComponent<Text>().text = speed.ToString();
        go.transform.FindChild("CapacityText").GetComponent<Text>().text = capacity.ToString();
        go.transform.FindChild("Description").GetComponent<Text>().text = (string)data;
        go.transform.FindChild("DefenseText").GetComponent<Text>().text = defense.ToString();
        go.transform.FindChild("DeployedCapacityText").GetComponent<Text>().text = deployedCapacity.ToString();
        go.transform.FindChild("GatherRateText").GetComponent<Text>().text = gatherRate.ToString();
        go.transform.FindChild("ResourceCapacityText").GetComponent<Text>().text = resourceCapacity.ToString();
        go.transform.FindChild("DefenseIcon").gameObject.SetActive(true);
        go.transform.FindChild("DefenseText").gameObject.SetActive(true);
        go.transform.FindChild("DeployedCapacityIcon").gameObject.SetActive(true);
        go.transform.FindChild("DeployedCapacityText").gameObject.SetActive(true);
        go.transform.FindChild("GatherRateIcon").gameObject.SetActive(true);
        go.transform.FindChild("GatherRateText").gameObject.SetActive(true);
    }
}

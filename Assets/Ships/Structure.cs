using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

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
    private ResourceGatherType types;

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

    public List<string> Constructables { get { return constructables; } }

    public Structure(Sprite icon, string name, float hull, float firepower, float speed, int capacity, int rCapacity,
        float defense, int deployedCapacity, int gatherRate, int range, List<string> constructables, ShipProperties shipProperties, ResourceGatherType type, Dictionary<Resource, int> requiredResources)
        : base(icon, name, hull, firepower, speed, capacity, rCapacity, shipProperties, requiredResources)
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

    protected override bool CanConstruct(Dictionary<Resource, int> resources)
    {
        return base.CanConstruct(resources);
    }

    public override Ship Copy()
    {
        var ship = new Structure(icon, name, baseHull, baseFirepower, baseSpeed, baseCapacity, baseResourceCapacity,
            defense, deployedCapacity, gatherRate, range, constructables, shipProperties, types, requiredResources);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }

    public List<KeyValuePair<ResourceGatherType, int>> Gather(Resource rType, int resources, Inhabitance pType, int population, Team team)
    {
        var gathertypes = new List<KeyValuePair<ResourceGatherType, int>>();
        int min;

        if((types & ResourceGatherType.Natural) > 0)
        {
            min = Math.Min(gatherRate, resources);
            this.resources[rType] += min;
            gathertypes.Add(new KeyValuePair<ResourceGatherType, int>(ResourceGatherType.Natural, min));
        }
        if((types & ResourceGatherType.Research) > 0)
        {
            gathertypes.Add(new KeyValuePair<ResourceGatherType, int>(ResourceGatherType.Research, gatherRate));
        }
        if((types & ResourceGatherType.Soldiers) > 0 && pType != Inhabitance.Uninhabited)
        {
            min = Math.Min(gatherRate, capacity - CountPopulation());
            min = Math.Min(min, population);
            this.population[pType] += min;
            gathertypes.Add(new KeyValuePair<ResourceGatherType, int>(ResourceGatherType.Soldiers, min));
        }

        return gathertypes;
    }

    public void Deploy(Tile tile)
    {
        swapPopulation = capacity;
        capacity = deployedCapacity;
        shipProperties |= ShipProperties.Untransferable;
        resourceCapacity = 99999;
        swapCapacity = resourceCapacity;
        isDeployed = true;
    }

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
    }

    public void PopulateStructurePanel(GameObject list)
    {
        list.transform.FindChild("StructureIcon").GetComponent<Image>().sprite = icon;
        list.transform.FindChild("StructureName").GetComponent<Text>().text = name;
        list.transform.FindChild("Capacity").GetComponent<Text>().text = CountPopulation().ToString() + " / " + deployedCapacity.ToString();
        list.transform.FindChild("Defense").GetComponent<Text>().text = defense.ToString();
        list.transform.FindChild("GatherRate").GetComponent<Text>().text = gatherRate.ToString();
        list.transform.FindChild("OilAmount").GetComponent<Text>().text = resources[Resource.Oil].ToString();
        list.transform.FindChild("OreAmount").GetComponent<Text>().text = resources[Resource.Ore].ToString();
        list.transform.FindChild("AsterminiumAmount").GetComponent<Text>().text = resources[Resource.Asterminium].ToString();
        list.transform.FindChild("ForestAmount").GetComponent<Text>().text = resources[Resource.Forest].ToString();
    }

    GameObject ListableObject.CreateListEntry(string listName, int index, System.Object data)
    {
        var shipEntry = UnityEngine.Resources.Load<GameObject>(LIST_PREFAB);
        var entry = GameObject.Instantiate(shipEntry) as GameObject;
        var icon = entry.transform.FindChild("Icon").GetComponent<Image>();
        icon.sprite = this.icon;

        var name = this.name;
        if (HumanPlayer.Instance.Squad.Tile != null && this == HumanPlayer.Instance.Squad.Tile.Structure)
            name = "(Deployed) " + name;
        entry.transform.FindChild("Name").GetComponent<Text>().text = name;
        entry.transform.FindChild("Population").GetComponent<Text>().text = CountPopulation().ToString() + " / " + capacity;
        entry.GetComponent<CustomUIAdvanced>().data = listName + "|" + index.ToString();

        return entry;
    }

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

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Structure : Ship
{
    private List<string> constructables;
    private Dictionary<Resource, int> resources;
    private float defense;
    private int deployedCapacity;
    private int gatherRate;

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

    public Dictionary<Resource, int> Resources { get { return resources; } }

    public List<string> Constructables { get { return constructables; } }

    public Structure(Sprite icon, string name, float hull, float firepower, float speed, int capacity,
        float defense, int deployedCapacity, int gatherRate, List<string> constructables, ShipType shipType, Dictionary<Resource, int> requiredResources)
        : base(icon, name, hull, firepower, speed, capacity, shipType, requiredResources)
    {
        this.defense = defense;
        this.deployedCapacity = deployedCapacity;
        this.gatherRate = gatherRate;
        this.constructables = constructables;
        this.resources = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, 0 },
            { Resource.Forest, 0 },
            { Resource.Oil, 0 },
            { Resource.Ore, 0 }
        };
    }

    protected override bool CanConstruct(Dictionary<Resource, int> resources)
    {
        return base.CanConstruct(resources);
    }

    public override Ship Copy()
    {
        var ship = new Structure(icon, name, baseHull, baseFirepower, baseSpeed, baseCapacity, 
            defense, deployedCapacity, gatherRate, constructables, shipType, requiredResources);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }

    public void PopulateStructurePanel(GameObject list)
    {
        list.transform.FindChild("StructureIcon").GetComponent<Image>().sprite = icon;
        list.transform.FindChild("StructureName").GetComponent<Text>().text = name;
        list.transform.FindChild("Capacity").GetComponent<Text>().text = totalPopulation.ToString()
            + " / " + deployedCapacity.ToString();
        list.transform.FindChild("Defense").GetComponent<Text>().text = defense.ToString();
        list.transform.FindChild("GatherRate").GetComponent<Text>().text = gatherRate.ToString();
        list.transform.FindChild("OilAmount").GetComponent<Text>().text = resources[Resource.Oil].ToString();
        list.transform.FindChild("OreAmount").GetComponent<Text>().text = resources[Resource.Ore].ToString();
        list.transform.FindChild("AsterminiumAmount").GetComponent<Text>().text = resources[Resource.Asterminium].ToString();
        list.transform.FindChild("ForestAmount").GetComponent<Text>().text = resources[Resource.Forest].ToString();
    }
}

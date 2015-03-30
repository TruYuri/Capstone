using UnityEngine;
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

    public Dictionary<Resource, int> Resources 
    {
        get { return resources; }
        set { resources = value; }
    }

    public List<string> Constructables { get { return constructables; } }

    public Structure(Sprite icon, string name, float hull, float firepower, float speed, int capacity,
        float defense, int deployedCapacity, int gatherRate, List<string> constructables, ShipType shipType, 
        int ore, int oil, int asterminium, int forest, int stations)
        : base(icon, name, hull, firepower, speed, capacity, shipType, 
        ore, oil, asterminium, forest, stations)
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

    public override bool CanConstruct(int stations, Structure structure)
    {
        return base.CanConstruct(stations, structure);
    }

    public override Ship Copy()
    {
        var ship = new Structure(icon, name, baseHull, baseFirepower, baseSpeed, baseCapacity, 
            defense, deployedCapacity, gatherRate, constructables, shipType,
            requiredOre, requiredOil, requiredAsterminium, requiredForest, requiredStations);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }
}

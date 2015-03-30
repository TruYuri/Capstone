using UnityEngine;
using System.Collections.Generic;

public class Structure : Ship
{
    private List<string> constructables;
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

    public List<string> Constructables { get { return constructables; } }

    public Structure(Sprite icon, string name, float hull, float firepower, float speed, int capacity, 
        float defense, int deployedCapacity, int gatherRate, List<string> constructables, ShipType shipType)
        : base(icon, name, hull, firepower, speed, capacity, shipType)
    {
        this.defense = defense;
        this.deployedCapacity = deployedCapacity;
        this.gatherRate = gatherRate;
        this.constructables = constructables;
    }

    public override Ship Copy()
    {
        var ship = new Structure(icon, name, baseHull, baseFirepower, baseSpeed, baseCapacity, defense, deployedCapacity, gatherRate, constructables, shipType);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }
}

using UnityEngine;
using System.Collections;

public class Structure : Ship
{
    private float defense;
    private int deployedCapacity;

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

    public Structure(string name, float hull, float firepower, float speed, int capacity, float defense, int deployedCapacity)
        : base(name, hull, firepower, speed, capacity)
    {
        this.defense = defense;
        this.deployedCapacity = deployedCapacity;
    }

    public override Ship Copy()
    {
        var ship = new Structure(name, baseHull, baseFirepower, baseSpeed, baseCapacity, defense, deployedCapacity);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }
}

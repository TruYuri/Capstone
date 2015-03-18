using UnityEngine;
using System.Collections;

public class Relay : Ship
{
    private int range;
    private float defense;

    public int Range
    {
        get { return range; }
        set { range = value; }
    }
    
    public float Defense
    {
        get { return defense; }
        set { defense = value; }
    }

    public Relay(string name, float hull, float firepower, float speed, int capacity, int range, float defense, ShipType shipType)
        : base(name, hull, firepower, speed, capacity, shipType)
    {
        this.range = range;
        this.defense = defense;
    }

    public override Ship Copy()
    {
        var ship = new Relay(name, baseHull, baseFirepower, baseSpeed, baseCapacity, range, defense, shipType);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }
}

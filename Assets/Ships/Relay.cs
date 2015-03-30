using UnityEngine;
using System.Collections;

public class Relay : Ship
{
    private int range;

    public int Range
    {
        get { return range; }
        set { range = value; }
    }

    public Relay(Sprite icon, string name, float hull, float firepower, float speed, int capacity,
        int range, int ore, int oil, int asterminium, int forest, int stations)
        : base(icon, name, hull, firepower, speed, capacity, ShipType.Relay, 
        ore, oil, asterminium, forest, stations)
    {
        this.range = range;
    }

    public override bool CanConstruct(int stations, Structure structure)
    {
        return base.CanConstruct(stations, structure);
    }

    public override Ship Copy()
    {
        var ship = new Relay(icon, name, baseHull, baseFirepower, baseSpeed, baseCapacity, 
            range, requiredOre, requiredOil, requiredAsterminium, requiredForest, requiredStations);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }
}

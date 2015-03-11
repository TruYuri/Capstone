using UnityEngine;
using System.Collections;

public class WarpPortal : Ship
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

    public WarpPortal(string name, float hull, float firepower, float speed, int capacity, int range)
        : base(name, hull, firepower, speed, capacity)
    {
        this.range = range;
    }
}

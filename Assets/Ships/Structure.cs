using UnityEngine;
using System.Collections;

public class Structure : Ship
{
    private float defense;
    private int deployedCapacity;

    public Structure(string name, float hull, float firepower, float speed, int capacity, float defense, int deployedCapacity)
        : base(name, hull, firepower, speed, capacity)
    {
        this.defense = defense;
        this.deployedCapacity = deployedCapacity;
    }
}

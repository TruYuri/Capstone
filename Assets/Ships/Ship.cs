using UnityEngine;
using System.Collections;

public class Ship
{
    protected string name;
    protected float hull;
    protected float firepower;
    protected float speed;
    protected int capacity;
    protected float protection;

    protected float baseHull;
    protected float baseFirepower;
    protected float baseSpeed;
    protected int baseCapacity;

    protected int totalPopulation;
    protected int primitivePopulation;
    protected int industrialPopulation;
    protected int spaceAgePopulation;

    public string Name
    {
        get { return name; }
    }
    public float Hull 
    { 
        get { return hull; }
        set { hull = value; }
    }
    public float Firepower 
    { 
        get { return firepower; }
        set { firepower = value; }
    }
    public float Speed 
    { 
        get { return speed; }
        set { speed = value; }
    }
    public int Capacity 
    { 
        get { return capacity; }
        set { capacity = value; }
    }
    public float Protection 
    { 
        get { return protection; }
        set { protection = value; }
    }

    public Ship(string name, float hull, float firepower, float speed, int capacity)
    {
        this.name = name;
        this.hull = this.baseHull = hull;
        this.firepower = this.baseFirepower = firepower;
        this.speed = this.baseSpeed = speed;
        this.capacity = this.baseCapacity = capacity;
    }
}

using UnityEngine;
using System.Collections.Generic;

public abstract class Research
{
    protected string name;
    protected float hull;
    protected float firepower;
    protected float speed;
    protected int capacity;
    protected float protection;
    protected int level;
    protected Dictionary<string, int> researchables;

    public string Name { get { return name; } }
    public float Hull { get { return hull; } }
    public float Firepower { get { return firepower; } }
    public float Speed { get { return speed; } }
    public float Capacity { get { return capacity; } }
    public float Protection { get { return protection; } }

    public Research(string name, int level, float hull, float firepower, float speed, int capacity)
    {
        this.researchables = new Dictionary<string, int>();
        this.name = name;
        this.hull = hull;
        this.firepower = firepower;
        this.speed = speed;
        this.capacity = capacity;
    }

    public int GetResearchLevel(string researchName) { return researchables[researchName]; }

    public virtual void AdvanceResearchLevel(string researchName, int stations) 
    {
        var invalidLevel = researchables[researchName] >= 10;
        var invalidStations = stations < researchables[researchName] * level;

        if (invalidLevel || invalidStations)
            return;
    }

    public virtual bool Unlock() { return false; }

    public virtual void Display() { }
}

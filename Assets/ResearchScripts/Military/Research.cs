using UnityEngine;
using System.Collections;

public abstract class Research
{
    private int hull;
    private int firepower;
    private int speed;
    private int capacity;
    private string name;

    // properties here
    public int Hull { get { return hull; } }
    public int Firepower { get { return firepower; } }
    public int Speed { get { return speed; } }
    public int Capacity { get { return capacity; } }
    public string Name { get { return name; } }

    public Research(int hull, int firepower, int speed, int capacity)
    {
        this.hull = hull;
        this.firepower = firepower;
        this.speed = speed;
        this.capacity = capacity;
    }

    public virtual void AdvanceHull()
    {

    }

    public virtual void AdvanceFirepower()
    {

    }

    public virtual void AdvanceSpeed()
    {

    }

    public virtual void AdvanceCapacity()
    {

    }

    public virtual bool Unlock() { return false; }
}

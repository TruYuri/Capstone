using UnityEngine;
using System.Collections.Generic;

public class Research
{
    protected string name;
    protected int level;
    protected Dictionary<string, int> upgrades;
    protected List<Research> prereqs;

    public string Name { get { return name; } }

    public Research(string name, int level, List<Research> prereqs)
    {
        this.upgrades = new Dictionary<string, int>();
        this.prereqs = prereqs;
        this.level = level;
        this.name = name;
    }

    protected bool CanUpgrade(string name, int stations)
    {
        var invalidLevel = upgrades[name] >= 10;
        var invalidStations = stations < (upgrades[name] + 1) * (level);

        if (invalidLevel || invalidStations)
            return false;

        return true;
    }

    public virtual void UpgradeResearch(string name)
    {
    }

    public virtual bool Unlock() 
    {
        return false;
    }

    public virtual void Display(GameObject panel, int stations) 
    { 
    }
}

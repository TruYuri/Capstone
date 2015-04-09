using UnityEngine;
using System.Collections.Generic;

public class Research
{
    protected string name;
    protected int level;
    protected Dictionary<string, int> upgrades;

    public string Name { get { return name; } }

    public Research(string name, int level)
    {
        this.upgrades = new Dictionary<string, int>();
        this.level = level;
        this.name = name;
    }

    public virtual bool UpgradeResearch(string name, int stations)
    {
        var invalidLevel = upgrades[name] >= 10;
        var invalidStations = stations < (upgrades[name] + 1) * (level);

        if (invalidLevel || invalidStations)
            return false;

        return true;
    }

    public virtual bool Unlock() 
    {
        return false;
    }

    public virtual void Display(GameObject panel) 
    { 
    }
}

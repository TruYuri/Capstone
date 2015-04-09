using UnityEngine;
using System.Collections.Generic;

// This class upgrades various concepts but also acts as the base for the Resource Transport, thus is a military script.
public class ComplexResearch : Research
{
    private const string DEFENSE = "Defense";
    private const string CAPACITY = "Capacity";

    private Dictionary<string, Structure> structureDefinitions; 

    public ComplexResearch(Dictionary<string, Ship> shipDefinitions) : base("Complex", 3)
    {
        structureDefinitions = new Dictionary<string, Structure>()
        {
            { "Gathering Complex", shipDefinitions["Gathering Complex"] as Structure },
            { "Research Complex", shipDefinitions["Research Complex"] as Structure },
            { "Military Complex", shipDefinitions["Military Complex"] as Structure },
            { "Base", shipDefinitions["Base"] as Structure }
        };

        upgrades.Add(DEFENSE, 0);
        upgrades.Add(CAPACITY, 0);
    }

    public override bool UpgradeResearch(string name, int stations)
    {
        var meetsCriteria = base.UpgradeResearch(name, stations);

        if (!meetsCriteria)
            return false;

        switch (name)
        {
            case DEFENSE:
                UpgradeDefense();
                break;
            case CAPACITY:
                UpgradeCapacity();
                break;
        }

        return true;
    }

    private void UpgradeDefense()
    {
        upgrades[DEFENSE]++;
    }

    private void UpgradeCapacity()
    {
        upgrades[CAPACITY]++;
    }

    public override bool Unlock()
    {
        structureDefinitions["Gathering Complex"].Unlocked = true;
        structureDefinitions["Research Complex"].Unlocked = true;
        structureDefinitions["Military Complex"].Unlocked = true;
        structureDefinitions["Base"].Unlocked = true;
        return true;
    }

    public override void Display(GameObject panel)
    {
        
    }
}


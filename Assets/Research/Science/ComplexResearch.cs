using UnityEngine;
using UnityEngine.UI;
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

    public override void UpgradeResearch(string name)
    {
        switch (name)
        {
            case DEFENSE:
                UpgradeDefense();
                break;
            case CAPACITY:
                UpgradeCapacity();
                break;
        }
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

    public override void Display(GameObject panel, int stations)
    {
        var items = new Dictionary<string, Transform>()
        {
            { DEFENSE, panel.transform.FindChild("ComplexDefenseButton") },
            { CAPACITY, panel.transform.FindChild("ComplexCapacityButton") },
        };

        foreach (var item in items)
        {
            item.Value.FindChild("CountText").GetComponent<Text>().text = upgrades[item.Key].ToString() + "/10";
            if (CanUpgrade(item.Key, stations) && Unlock())
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }
    }
}


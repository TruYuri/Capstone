using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// This class upgrades various concepts but also acts as the base for the Resource Transport, thus is a military script.
public class ComplexResearch : Research
{
    private const string DEFENSE = "Defense";
    private const string CAPACITY = "Capacity";

    private Dictionary<string, Structure> structureDefinitions;

    public ComplexResearch(Dictionary<string, Ship> shipDefinitions, List<Research> prereqs)
        : base("Complex", 3, prereqs)
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

        shipDefinitions["Gathering Complex"].Unlocked = true;
        shipDefinitions["Research Complex"].Unlocked = true;
        shipDefinitions["Base"].Unlocked = true;
        shipDefinitions["Military Complex"].Unlocked = true;
        shipDefinitions["Resource Transport"].Unlocked = true;
    }

    public override void UpgradeResearch(string name, Dictionary<Resource, int> resources)
    {
        switch (name)
        {
            case DEFENSE:
                UpgradeDefense();
                // subtract resources
                break;
            case CAPACITY:
                UpgradeCapacity();
                // subtract resources
                break;
        }

        foreach (var struc in structureDefinitions)
            struc.Value.RecalculateResources();
    }

    private void UpgradeDefense()
    {
        upgrades[DEFENSE]++;

        // upgrade complexes
    }

    private void UpgradeCapacity()
    {
        upgrades[CAPACITY]++;

        // upgrade complexes
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources)
    {
        var items = new Dictionary<string, Transform>()
        {
            { DEFENSE, panel.transform.FindChild("ComplexDefenseButton") },
            { CAPACITY, panel.transform.FindChild("ComplexCapacityButton") },
        };

        var p2 = panel.transform.FindChild("Complex");

        p2.FindChild("StatsDefenseText").GetComponent<Text>().text = "Defense: " + (upgrades[DEFENSE] * 10) + "%";
        p2.FindChild("StatsCapacityText").GetComponent<Text>().text = "Capacity: " + (upgrades[CAPACITY] * 10) + "%";

        foreach (var item in items)
        {
            item.Value.FindChild("CountText").GetComponent<Text>().text = upgrades[item.Key].ToString() + "/10";
            if (CanUpgrade(item.Key, resources[Resource.Stations]) && CanUnlock(resources))
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }
    }
}


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

        foreach (var upgrade in upgrades)
        {
            costs.Add(upgrade.Key, new Dictionary<Resource, int>()
                { 
                    { Resource.Asterminium, 0 },
                    { Resource.Forest, 0 },
                    { Resource.Oil, 0 },
                    { Resource.Ore, 0 },
                    { Resource.Stations, 0 }
                });
        }

        shipDefinitions["Gathering Complex"].Unlocked = true;
        shipDefinitions["Research Complex"].Unlocked = true;
        shipDefinitions["Base"].Unlocked = true;
        shipDefinitions["Military Complex"].Unlocked = true;
        shipDefinitions["Resource Transport"].Unlocked = true;
        unlocked = true;

        RecalculateResourceCosts(0);
    }

    public override Dictionary<Resource, int> UpgradeResearch(string name, float reduction)
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

        foreach (var struc in structureDefinitions)
            struc.Value.RecalculateResources();

        var r = costs[name];
        RecalculateResourceCosts(reduction);
        return r;
    }

    private void RecalculateResourceCosts(float reduction)
    {
        costs[DEFENSE] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[DEFENSE] + 1) * structureDefinitions["Base"].Defense * 2 * (1.0f - reduction)) },
            { Resource.Forest, Mathf.CeilToInt((upgrades[DEFENSE] + 1) * structureDefinitions["Base"].Defense * 2 * (1.0f - reduction)) }
        };

        costs[CAPACITY] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[CAPACITY] + 1) * structureDefinitions["Base"].DeployedCapacity * (1.0f - reduction)) },
            { Resource.Forest, Mathf.CeilToInt((upgrades[CAPACITY] + 1) * structureDefinitions["Base"].DeployedCapacity * (1.0f - reduction)) }
        };

        var types = new List<Resource>() { Resource.Asterminium, Resource.Ore, Resource.Oil, Resource.Forest };
        foreach (var c in costs)
        {
            foreach (var t in types)
                if (!c.Value.ContainsKey(t))
                    c.Value.Add(t, 0);
        }
    }

    private void UpgradeDefense()
    {
        upgrades[DEFENSE]++;

        foreach (var ship in structureDefinitions)
            ship.Value.Defense += ship.Value.Defense * 0.1f;
    }

    private void UpgradeCapacity()
    {
        upgrades[CAPACITY]++;

        foreach (var ship in structureDefinitions)
            ship.Value.DeployedCapacity += Mathf.CeilToInt(ship.Value.DeployedCapacity * 0.1f);
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources, float reduction)
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
            if (CanUpgrade(item.Key, resources, reduction) && unlocked)
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }
    }
}


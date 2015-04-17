using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// This class upgrades various concepts but also acts as the base for the Resource Transport, thus is a military script.
public class EfficiencyResearch : Research
{
    private const string GATHERING = "Gathering";
    private const string RESEARCH = "Research";
    private const string MILITARY = "Military";
    private const string RESOURCE_TRANSPORT = "Resource Transport";

    private Dictionary<string, Ship> shipDefinitions;

    public EfficiencyResearch(Dictionary<string, Ship> shipDefinitions, List<Research> prereqs)
        : base("Efficiency", 2, prereqs)
    {
        this.shipDefinitions = shipDefinitions;
        upgrades.Add(GATHERING, 0);
        upgrades.Add(RESEARCH, 0);
        upgrades.Add(MILITARY, 0);
        upgrades.Add(RESOURCE_TRANSPORT, 0);

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

        RecalculateResourceCosts();
    }

    public override Dictionary<Resource, int> UpgradeResearch(string name)
    {
        switch(name)
        {
            case GATHERING:
                upgrades[GATHERING]++;
                // upgrade complexes
                // subtract resources
                break;
            case RESEARCH:
                upgrades[RESEARCH]++;
                // upgrade complexes
                // subtract resources
                break;
            case MILITARY:
                UpgradeMilitary();
                // subtract resources
                break;
            case RESOURCE_TRANSPORT:
                upgrades[RESOURCE_TRANSPORT]++;
                shipDefinitions[RESOURCE_TRANSPORT].Hull += 5;
                shipDefinitions[RESOURCE_TRANSPORT].Capacity += 0;
                break;
        }

        foreach (var ship in shipDefinitions)
            ship.Value.RecalculateResources();
        var r = costs[name];
        RecalculateResourceCosts();
        return r;
    }

    private void RecalculateResourceCosts()
    {
        costs[GATHERING] = new Dictionary<Resource, int>()
        {
            { Resource.Forest, ((upgrades[GATHERING] + 1) * 400) },
            { Resource.Ore, ((upgrades[GATHERING] + 1) * 300) },
            { Resource.Oil, ((upgrades[GATHERING] + 1) * 200) },
            { Resource.Asterminium, ((upgrades[GATHERING] + 1) * 100) }
        };

        costs[RESEARCH] = new Dictionary<Resource, int>()
        {
            { Resource.Forest, ((upgrades[RESEARCH] + 1) * 400) },
            { Resource.Ore, ((upgrades[RESEARCH] + 1) * 300) },
            { Resource.Oil, ((upgrades[RESEARCH] + 1) * 200) },
            { Resource.Asterminium, ((upgrades[RESEARCH] + 1) * 100) }
        };

        costs[MILITARY] = new Dictionary<Resource, int>()
        {
            { Resource.Forest, ((upgrades[MILITARY] + 1) * 400) },
            { Resource.Ore, ((upgrades[MILITARY] + 1) * 300) },
            { Resource.Oil, ((upgrades[MILITARY] + 1) * 200) },
            { Resource.Asterminium, ((upgrades[MILITARY] + 1) * 100) }
        };

        costs[RESOURCE_TRANSPORT] = new Dictionary<Resource, int>()
        {
            { Resource.Forest, ((upgrades[RESOURCE_TRANSPORT] + 1) * 400) },
            { Resource.Ore, ((upgrades[RESOURCE_TRANSPORT] + 1) * 300) },
            { Resource.Oil, ((upgrades[RESOURCE_TRANSPORT] + 1) * 200) },
            { Resource.Asterminium, ((upgrades[RESOURCE_TRANSPORT] + 1) * 100) }
        };
    }

    private void UpgradeMilitary()
    {
        List<string> militaryShipNames = new List<string> 
        { 
            "Fighter", 
            "Transport", 
            "Guard Satellite", 
            "Heavy Fighter", 
            "Behemoth" 
        };

        var oldBonus = upgrades[MILITARY] * 0.01f;
        upgrades[MILITARY]++;
        var newBonus = upgrades[MILITARY] * 0.01f;

        // not sure if these are being calculated correctly
        foreach(var ship in militaryShipNames)
        {
            // undo previous
            shipDefinitions[ship].Hull += newBonus - oldBonus;
            shipDefinitions[ship].Firepower += newBonus - oldBonus;
            shipDefinitions[ship].Speed += newBonus - oldBonus;
            // shipDefinitions[ship].Capacity += newBonus - oldBonus;
        }
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources)
    {
        var items = new Dictionary<string, Transform>()
        {
            { GATHERING, panel.transform.FindChild("EfficiencyGatheringButton") },
            { RESEARCH, panel.transform.FindChild("EfficiencyResearchButton") },
            { MILITARY, panel.transform.FindChild("EfficiencyMilitaryButton") },
            { RESOURCE_TRANSPORT, panel.transform.FindChild("EfficiencyResourceButton") },
        };

        var p2 = panel.transform.FindChild("Efficiency");

        p2.FindChild("StatsResourceText").GetComponent<Text>().text = "Resource Hull: " + shipDefinitions["Resource Transport"].Speed.ToString();
        p2.FindChild("StatsMilitaryText").GetComponent<Text>().text = "Miitary: " + (upgrades[MILITARY] * 1) .ToString() + "%";
        p2.FindChild("StatsGatheringText").GetComponent<Text>().text = "Gather: " + (upgrades[GATHERING] * 2).ToString() + "%";
        p2.FindChild("StatsResearchText").GetComponent<Text>().text = "Research: -" + (upgrades[RESEARCH] * 1).ToString() + "%";

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

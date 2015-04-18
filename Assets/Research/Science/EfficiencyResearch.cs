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
    private Player player;

    public EfficiencyResearch(Dictionary<string, Ship> shipDefinitions, List<Research> prereqs, Player player)
        : base("Efficiency", 2, prereqs)
    {
        this.shipDefinitions = shipDefinitions;
        this.player = player;
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

        shipDefinitions["Resource Transport"].Unlocked = unlocked = true;
        RecalculateResourceCosts(0);
    }

    public override Dictionary<Resource, int> UpgradeResearch(string name, float reduction)
    {
        switch(name)
        {
            case GATHERING:
                upgrades[GATHERING]++;
                var structureNames = new List<string>
                {
                    "Base",
                    "Military Complex",
                    "Research Complex",
                    "Gathering Complex"
                };

                foreach (var struc in structureNames)
                    ((Structure)shipDefinitions[struc]).GatherRate += Mathf.CeilToInt(((Structure)shipDefinitions[struc]).GatherRate * 0.02f);
                break;
            case RESEARCH:
                upgrades[RESEARCH]++;
                player.ResearchCostReduction = upgrades[RESEARCH] * 0.01f;
                break;
            case MILITARY:
                foreach(var ship in shipDefinitions)
                {
                    ship.Value.Firepower += ship.Value.Firepower * 0.01f;
                    ship.Value.Hull += ship.Value.Hull * 0.01f;
                    ship.Value.Speed += ship.Value.Speed * 0.01f;
                    ship.Value.Capacity += Mathf.CeilToInt(ship.Value.Capacity * 0.01f);
                }
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
        RecalculateResourceCosts(reduction);
        return r;
    }

    private void RecalculateResourceCosts(float reduction)
    {
        costs[GATHERING] = new Dictionary<Resource, int>()
        {
            { Resource.Forest, Mathf.CeilToInt(((upgrades[GATHERING] + 1) * 400 * (1.0f - reduction))) },
            { Resource.Ore, Mathf.CeilToInt(((upgrades[GATHERING] + 1) * 300 * (1.0f - reduction))) },
            { Resource.Oil, Mathf.CeilToInt(((upgrades[GATHERING] + 1) * 200 * (1.0f - reduction))) },
            { Resource.Asterminium, Mathf.CeilToInt(((upgrades[GATHERING] + 1) * 100 * (1.0f - reduction))) }
        };

        costs[RESEARCH] = new Dictionary<Resource, int>()
        {
            { Resource.Forest, Mathf.CeilToInt(((upgrades[RESEARCH] + 1) * 400 * (1.0f - reduction))) },
            { Resource.Ore, Mathf.CeilToInt(((upgrades[RESEARCH] + 1) * 300 * (1.0f - reduction))) },
            { Resource.Oil, Mathf.CeilToInt(((upgrades[RESEARCH] + 1) * 200 * (1.0f - reduction))) },
            { Resource.Asterminium, Mathf.CeilToInt(((upgrades[RESEARCH] + 1) * 100 * (1.0f - reduction))) }
        };

        costs[MILITARY] = new Dictionary<Resource, int>()
        {
            { Resource.Forest, Mathf.CeilToInt(((upgrades[MILITARY] + 1) * 400 * (1.0f - reduction))) },
            { Resource.Ore, Mathf.CeilToInt(((upgrades[MILITARY] + 1) * 300 * (1.0f - reduction))) },
            { Resource.Oil, Mathf.CeilToInt(((upgrades[MILITARY] + 1) * 200 * (1.0f - reduction))) },
            { Resource.Asterminium, Mathf.CeilToInt(((upgrades[MILITARY] + 1) * 100 * (1.0f - reduction))) }
        };

        costs[RESOURCE_TRANSPORT] = new Dictionary<Resource, int>()
        {
            { Resource.Forest, Mathf.CeilToInt(((upgrades[RESOURCE_TRANSPORT] + 1) * 400 * (1.0f - reduction))) },
            { Resource.Ore, Mathf.CeilToInt(((upgrades[RESOURCE_TRANSPORT] + 1) * 300 * (1.0f - reduction))) },
            { Resource.Oil, Mathf.CeilToInt(((upgrades[RESOURCE_TRANSPORT] + 1) * 200 * (1.0f - reduction))) },
            { Resource.Asterminium, Mathf.CeilToInt(((upgrades[RESOURCE_TRANSPORT] + 1) * 100 * (1.0f - reduction))) }
        };
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources, float reduction)
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
            if (CanUpgrade(item.Key, resources, reduction) && unlocked)
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }
    }
}

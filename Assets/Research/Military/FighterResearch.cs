using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FighterResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string THRUSTERS = "Thrusters";

    private Ship fighterShip;

    public FighterResearch(Ship ship, List<Research> prereqs) 
        : base(ship.Name, 1, prereqs)
    {
        this.fighterShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(THRUSTERS, 0);

        fighterShip.Unlocked = true;

        foreach(var upgrade in upgrades)
        {
            costs.Add(upgrade.Key, new Dictionary<Resource,int>()
                { 
                    { Resource.Asterminium, 0 },
                    { Resource.Forest, 0 },
                    { Resource.Oil, 0 },
                    { Resource.Ore, 0 },
                    { Resource.Stations, 0 }
                });
        }

        unlocked = fighterShip.Unlocked = true;
        RecalculateResourceCosts(0);
    }

    public override Dictionary<Resource, int> UpgradeResearch(string name, float reduction) 
    {
        switch(name)
        {
            case ARMOR:
                upgrades[ARMOR]++;
                fighterShip.Hull += 0.25f;
                break;
            case PLATING:
                upgrades[PLATING]++;
                fighterShip.Protection = upgrades[PLATING] * 0.02f;
                fighterShip.Plating++;
                break;
            case PLASMAS:
                upgrades[PLASMAS]++;
                fighterShip.Firepower += 0.25f;
                break;
            case THRUSTERS:
                upgrades[THRUSTERS]++;
                fighterShip.Speed += 1.0f;
                break;
        }

        fighterShip.RecalculateResources();
        var r = costs[name];
        RecalculateResourceCosts(reduction);
        return r;
    }

    private void RecalculateResourceCosts(float reduction)
    {
        costs[ARMOR] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[ARMOR] + 1) * 0.25f * fighterShip.Hull * (1.0f - reduction)) }
        };

        costs[PLATING] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, Mathf.CeilToInt((upgrades[PLATING] + 1) * fighterShip.Hull * (1.0f - reduction)) }
        };

        costs[PLASMAS] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[PLASMAS] + 1) * 0.25f * fighterShip.Firepower * (1.0f - reduction)) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[PLASMAS] + 1) * 0.25f * fighterShip.Firepower * (1.0f - reduction)) }
        };

        costs[THRUSTERS] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[THRUSTERS] + 1) * 1f * fighterShip.Speed * 10f * (1.0f - reduction)) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[THRUSTERS] + 1) * 1f * fighterShip.Speed * 10f * (1.0f - reduction)) }
        };

        var types = new List<Resource>() { Resource.Asterminium, Resource.Ore, Resource.Oil, Resource.Forest };
        foreach (var c in costs)
        {
            foreach (var t in types)
                if (!c.Value.ContainsKey(t))
                    c.Value.Add(t, 0);
        }
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources, float reduction)
    {
        RecalculateResourceCosts(reduction);

        var items = new Dictionary<string, Transform>()
        {
            { THRUSTERS, panel.transform.FindChild("FighterThrustersButton") },
            { ARMOR, panel.transform.FindChild("FighterArmorButton") },
            { PLATING, panel.transform.FindChild("FighterAsterminiumButton") },
            { PLASMAS, panel.transform.FindChild("FighterPlasmasButton") }
        };

        var p2 = panel.transform.FindChild("Fighter");

        p2.FindChild("StatsSpeedText").GetComponent<Text>().text = "Speed: " + fighterShip.Speed.ToString();
        p2.FindChild("StatsFirepowerText").GetComponent<Text>().text = "Firepower: " + fighterShip.Firepower.ToString();
        p2.FindChild("StatsHullText").GetComponent<Text>().text = "Hull: " + fighterShip.Hull.ToString();
        p2.FindChild("StatsCapacityText").GetComponent<Text>().text = "Capacity: " + fighterShip.Capacity.ToString();

        foreach (var item in items)
        {
            item.Value.FindChild("CountText").GetComponent<Text>().text = upgrades[item.Key].ToString() + "/10";
            if (CanUpgrade(item.Key, resources, reduction) && unlocked)
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }

        if (upgrades[ARMOR] < 5)
            items[PLATING].GetComponent<Button>().interactable = false;
    }
}

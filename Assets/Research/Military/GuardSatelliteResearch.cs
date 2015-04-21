using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GuardSatelliteResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";

    private Ship guardSatelliteShip;

    public GuardSatelliteResearch(Ship ship, List<Research> prereqs) 
        : base(ship.Name, 3, prereqs)
    {
        this.guardSatelliteShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(TORPEDOES, 0);

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

        RecalculateResourceCosts(0);
    }

    public override Dictionary<Resource, int> UpgradeResearch(string name, float reduction)
    {
        switch (name)
        {
            case ARMOR:
                upgrades[ARMOR]++;
                guardSatelliteShip.Hull += 0.5f;
                break;
            case PLATING:
                upgrades[PLATING]++;
                guardSatelliteShip.Protection = upgrades[PLATING] * 0.02f;
                guardSatelliteShip.Plating++;
                break;
            case PLASMAS:
                upgrades[PLASMAS]++;
                guardSatelliteShip.Firepower += 1.0f;
                break;
            case TORPEDOES:
                guardSatelliteShip.Firepower -= upgrades[TORPEDOES] * 0.02f;
                upgrades[TORPEDOES]++;
                guardSatelliteShip.Firepower += upgrades[TORPEDOES] * 0.02f;
                break;
        }

        guardSatelliteShip.RecalculateResources();
        var r = costs[name];
        RecalculateResourceCosts(reduction);
        return r;
    }

    private void RecalculateResourceCosts(float reduction)
    {
        costs[ARMOR] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[ARMOR] + 1) * 0.5f * guardSatelliteShip.Hull * (1.0f - reduction)) }
        };

        costs[PLATING] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, Mathf.CeilToInt((upgrades[PLATING] + 1) * guardSatelliteShip.Hull * (1.0f - reduction)) }
        };

        costs[PLASMAS] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[PLASMAS] + 1) * 1 * guardSatelliteShip.Firepower * (1.0f - reduction)) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[PLASMAS] + 1) * 1 * guardSatelliteShip.Firepower * (1.0f - reduction)) }
        };

        costs[TORPEDOES] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, Mathf.CeilToInt((upgrades[TORPEDOES] + 1) * guardSatelliteShip.Firepower) }
        };

        var types = new List<Resource>() { Resource.Asterminium, Resource.Ore, Resource.Oil, Resource.Forest };
        foreach (var c in costs)
        {
            foreach (var t in types)
                if (!c.Value.ContainsKey(t))
                    c.Value.Add(t, 0);
        }
    }

    public override Dictionary<Resource, int> Unlock(float reduction)
    {
        base.Unlock(reduction);
        guardSatelliteShip.Unlocked = true;
        return guardSatelliteShip.CanConstruct(null, 5, reduction).Value;
    }

    public override bool CanUnlock(Dictionary<Resource, int> resources, float reduction)
    {
        if (unlocked || guardSatelliteShip.Unlocked)
            return true;

        bool unlock = true;

        foreach (var p in prereqs)
            unlock = unlock && p.Unlocked;
        unlock = unlock && guardSatelliteShip.CanConstruct(resources, 5, reduction).Key;

        return unlock;
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources, float reduction)
    {
        RecalculateResourceCosts(reduction);

        var items = new Dictionary<string, Transform>()
        {
            { TORPEDOES, panel.transform.FindChild("GuardTorpedoesButton") },
            { ARMOR, panel.transform.FindChild("GuardArmorButton") },
            { PLATING, panel.transform.FindChild("GuardAsterminiumButton") },
            { PLASMAS, panel.transform.FindChild("GuardPlasmasButton") }
        };

        var p2 = panel.transform.FindChild("GuardSatelliteUnlocked");
        var p1 = panel.transform.FindChild("GuardSatellite");

        p1.gameObject.SetActive(false);
        p2.gameObject.SetActive(false);

        if (unlocked)
        {
            p2.gameObject.SetActive(true);
            p2.FindChild("StatsSpeedText").GetComponent<Text>().text = "Speed: " + guardSatelliteShip.Speed.ToString();
            p2.FindChild("StatsFirepowerText").GetComponent<Text>().text = "Firepower: " + guardSatelliteShip.Firepower.ToString();
            p2.FindChild("StatsHullText").GetComponent<Text>().text = "Hull: " + guardSatelliteShip.Hull.ToString();
            p2.FindChild("StatsCapacityText").GetComponent<Text>().text = "Capacity: " + guardSatelliteShip.Capacity.ToString();
        }
        else
        {
            var r = guardSatelliteShip.CanConstruct(null, 5, reduction).Value;

            if (!costs.ContainsKey("Unlock"))
                costs.Add("Unlock", null);

            costs["Unlock"] = r;

            p1.gameObject.SetActive(true);
            p1.GetComponentInChildren<Button>().interactable = CanUnlock(resources, reduction);
            p1.FindChild("ForestText").GetComponent<Text>().text = costs["Unlock"][Resource.Forest].ToString();
            p1.FindChild("OreText").GetComponent<Text>().text = costs["Unlock"][Resource.Ore].ToString();
            p1.FindChild("OilText").GetComponent<Text>().text = costs["Unlock"][Resource.Oil].ToString();
            p1.FindChild("AsterminiumText").GetComponent<Text>().text = costs["Unlock"][Resource.Asterminium].ToString();
        }

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

    public override void DisplayPopup(GameObject panel, string upgrade, float reduction)
    {
        if (upgrade == "Unlock")
        {
            var r = guardSatelliteShip.CanConstruct(null, 5, reduction).Value;

            if (!costs.ContainsKey("Unlock"))
                costs.Add("Unlock", null);

            costs["Unlock"] = r;
        }

        base.DisplayPopup(panel, upgrade, reduction);
    }
}

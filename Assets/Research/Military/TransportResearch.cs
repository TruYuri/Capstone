using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TransportResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string THRUSTERS = "Thrusters";
    private const string CAPACITY = "Capacity";

    private Ship transportShip;

    public TransportResearch(Ship ship, List<Research> prereqs)
        : base(ship.Name, 2, prereqs)
    {
        this.transportShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(THRUSTERS, 0);
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

        RecalculateResourceCosts(0);
    }

    public override Dictionary<Resource, int> UpgradeResearch(string name, float reduction)
    {

        switch (name)
        {
            case ARMOR:
                upgrades[ARMOR]++;
                transportShip.Hull += 2.0f;
                break;
            case PLATING:
                upgrades[PLATING]++;
                transportShip.Protection = upgrades[PLATING] * 0.02f;
                transportShip.Plating++;
                break;
            case THRUSTERS:
                upgrades[THRUSTERS]++;
                transportShip.Speed += 0.5f;
                break;
            case CAPACITY:
                upgrades[CAPACITY]++;
                transportShip.Capacity += 100;
                break;
        }

        transportShip.RecalculateResources();
        var r = costs[name];
        RecalculateResourceCosts(reduction);
        return r;
    }

    private void RecalculateResourceCosts(float reduction)
    {
        costs[ARMOR] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[ARMOR] + 1) * 2 * transportShip.Hull * (1.0f - reduction)) }
        };

        costs[PLATING] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, Mathf.CeilToInt((upgrades[PLATING] + 1) * transportShip.Hull * (1.0f - reduction)) }
        };

        costs[THRUSTERS] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[THRUSTERS] + 1) * 0.5f * transportShip.Speed * 10f * (1.0f - reduction)) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[THRUSTERS] + 1) * 0.5f * transportShip.Speed * 10f * (1.0f - reduction)) }
        };

        costs[CAPACITY] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[CAPACITY] + 1) * 100f * transportShip.Hull / 2.0f * (1.0f - reduction)) },
            { Resource.Forest, Mathf.CeilToInt((upgrades[CAPACITY] + 1) * 100f * transportShip.Hull / 2.0f * (1.0f - reduction)) }
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
        transportShip.Unlocked = true;

        return transportShip.CanConstruct(null, 5, reduction).Value;
    }

    public override bool CanUnlock(Dictionary<Resource, int> resources, float reduction)
    {
        if (unlocked || transportShip.Unlocked)
            return true;

        bool unlock = true;

        foreach (var p in prereqs)
            unlock = unlock && p.Unlocked;
        unlock = unlock && transportShip.CanConstruct(resources, 5, reduction).Key;

        return unlock;
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources, float reduction) 
    {
        var items = new Dictionary<string, Transform>()
        {
            { THRUSTERS, panel.transform.FindChild("TransportThrustersButton") },
            { CAPACITY, panel.transform.FindChild("TransportCapacityButton") },
            { ARMOR, panel.transform.FindChild("TransportArmorButton") },
            { PLATING, panel.transform.FindChild("TransportAsterminiumButton") },
        };

        var p2 = panel.transform.FindChild("TransportUnlocked");
        var p1 = panel.transform.FindChild("Transport");

        p1.gameObject.SetActive(false);
        p2.gameObject.SetActive(false);

        if (unlocked)
        {
            p2.gameObject.SetActive(true);
            p2.FindChild("StatsSpeedText").GetComponent<Text>().text = "Speed: " + transportShip.Speed.ToString();
            p2.FindChild("StatsFirepowerText").GetComponent<Text>().text = "Firepower: " + transportShip.Firepower.ToString();
            p2.FindChild("StatsHullText").GetComponent<Text>().text = "Hull: " + transportShip.Hull.ToString();
            p2.FindChild("StatsCapacityText").GetComponent<Text>().text = "Capacity: " + transportShip.Capacity.ToString();
        }
        else
        {
            p1.gameObject.SetActive(true);
            p1.GetComponentInChildren<Button>().interactable = CanUnlock(resources, reduction);
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
}

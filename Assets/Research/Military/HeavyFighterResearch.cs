using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeavyFighterResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";
    private const string THRUSTERS = "Thrusters";
    private const string CAPACITY = "Capacity";

    private Ship heavyFighterShip;

    public HeavyFighterResearch(Ship ship, List<Research> prereqs)
        : base(ship.Name, 4, prereqs)
    {
        this.heavyFighterShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(TORPEDOES, 0);
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
                heavyFighterShip.Hull += 0.5f;
                break;
            case PLATING:
                upgrades[PLATING]++;
                heavyFighterShip.Protection = upgrades[PLATING] * 0.02f;
                heavyFighterShip.Plating++;
                break;
            case PLASMAS:
                upgrades[PLASMAS]++;
                heavyFighterShip.Firepower += 0.75f;
                break;
            case TORPEDOES:
                heavyFighterShip.Firepower -= upgrades[TORPEDOES] * 0.02f;
                upgrades[TORPEDOES]++;
                heavyFighterShip.Firepower += upgrades[TORPEDOES] * 0.02f;
                break;
            case THRUSTERS:
                upgrades[THRUSTERS]++;
                heavyFighterShip.Speed += 0.5f;
                break;
            case CAPACITY:
                upgrades[CAPACITY]++;
                heavyFighterShip.Capacity += 10;
                break;
        }

        heavyFighterShip.RecalculateResources();
        var r = costs[name];
        RecalculateResourceCosts(reduction);
        return r;
    }

    private void RecalculateResourceCosts(float reduction)
    {
        costs[ARMOR] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[ARMOR] + 1) * 0.5f * heavyFighterShip.Hull * (1.0f - reduction)) }
        };

        costs[PLATING] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, Mathf.CeilToInt((upgrades[PLATING] + 1) * heavyFighterShip.Hull * (1.0f - reduction)) }
        };

        costs[PLASMAS] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[PLASMAS] + 1) * 0.75f * heavyFighterShip.Firepower * (1.0f - reduction)) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[PLASMAS] + 1) * 0.75f * heavyFighterShip.Firepower * (1.0f - reduction)) }
        };

        costs[TORPEDOES] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, Mathf.CeilToInt((upgrades[TORPEDOES] + 1) * heavyFighterShip.Firepower * (1.0f - reduction)) }
        };

        costs[THRUSTERS] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[THRUSTERS] + 1) * 0.25f * heavyFighterShip.Speed * 10f * (1.0f - reduction)) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[THRUSTERS] + 1) * 0.25f * heavyFighterShip.Speed * 10f * (1.0f - reduction)) }
        };

        costs[CAPACITY] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[CAPACITY] + 1) * 10f * heavyFighterShip.Hull / 2.0f * (1.0f - reduction)) },
            { Resource.Forest, Mathf.CeilToInt((upgrades[CAPACITY] + 1) * 10f * heavyFighterShip.Hull / 2.0f * (1.0f - reduction)) }
        };
    }

    public override Dictionary<Resource, int> Unlock(float reduction)
    {
        base.Unlock(reduction);
        heavyFighterShip.Unlocked = true;
        return heavyFighterShip.CanConstruct(null, 5, reduction).Value;
    }

    public override bool CanUnlock(Dictionary<Resource, int> resources, float reduction)
    {
        if (unlocked || heavyFighterShip.Unlocked)
            return true;

        bool unlock = true;

        foreach (var p in prereqs)
            unlock = unlock && p.Unlocked;
        unlock = unlock && heavyFighterShip.CanConstruct(resources, 5, reduction).Key;

        return unlock;
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources, float reduction)
    {
        var items = new Dictionary<string, Transform>()
        {
            { TORPEDOES, panel.transform.FindChild("HeavyTorpedoesButton") },
            { THRUSTERS, panel.transform.FindChild("HeavyThrustersButton") },
            { CAPACITY, panel.transform.FindChild("HeavyCapacityButton") },
            { ARMOR, panel.transform.FindChild("HeavyArmorButton") },
            { PLATING, panel.transform.FindChild("HeavyAsterminiumButton") },
            { PLASMAS, panel.transform.FindChild("HeavyPlasmasButton") }
        };

        var p2 = panel.transform.FindChild("HeavyFighterUnlocked");
        var p1 = panel.transform.FindChild("HeavyFighter");

        p1.gameObject.SetActive(false);
        p2.gameObject.SetActive(false);

        if (unlocked)
        {
            p2.gameObject.SetActive(true);
            p2.FindChild("StatsSpeedText").GetComponent<Text>().text = "Speed: " + heavyFighterShip.Speed.ToString();
            p2.FindChild("StatsFirepowerText").GetComponent<Text>().text = "Firepower: " + heavyFighterShip.Firepower.ToString();
            p2.FindChild("StatsHullText").GetComponent<Text>().text = "Hull: " + heavyFighterShip.Hull.ToString();
            p2.FindChild("StatsCapacityText").GetComponent<Text>().text = "Capacity: " + heavyFighterShip.Capacity.ToString();
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

        if (upgrades[PLASMAS] < 5)
            items[TORPEDOES].GetComponent<Button>().interactable = false;
    }
}

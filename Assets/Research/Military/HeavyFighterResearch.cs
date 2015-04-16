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
    }

    public override void UpgradeResearch(string name, Dictionary<Resource, int> resources)
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
    }

    public override void Unlock()
    {
        base.Unlock();
        heavyFighterShip.Unlocked = true;
    }

    public override bool CanUnlock(Dictionary<Resource, int> resources)
    {
        if (unlocked || heavyFighterShip.Unlocked)
            return true;

        bool unlock = true;

        foreach (var p in prereqs)
            unlock = unlock && p.Unlocked;
        unlock = unlock && heavyFighterShip.CanConstruct(resources, 5);

        return unlock;
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources)
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
            p1.GetComponentInChildren<Button>().interactable = CanUnlock(resources);
        }

        foreach (var item in items)
        {
            item.Value.FindChild("CountText").GetComponent<Text>().text = upgrades[item.Key].ToString() + "/10";
            if (CanUpgrade(item.Key, resources[Resource.Stations]) && CanUnlock(resources))
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

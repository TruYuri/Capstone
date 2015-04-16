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
        Unlock();
    }

    public override void UpgradeResearch(string name, Dictionary<Resource, int> resources) 
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
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources)
    {
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
            if (CanUpgrade(item.Key, resources[Resource.Stations]) && CanUnlock(resources))
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }

        if (upgrades[ARMOR] < 5)
            items[PLATING].GetComponent<Button>().interactable = false;
    }
}

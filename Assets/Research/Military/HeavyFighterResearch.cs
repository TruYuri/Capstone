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

    public HeavyFighterResearch(Ship ship) : base(ship.Name, 4)
    {
        this.heavyFighterShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(TORPEDOES, 0);
        upgrades.Add(THRUSTERS, 0);
        upgrades.Add(CAPACITY, 0);
    }

    public override void UpgradeResearch(string name)
    {
        switch (name)
        {
            case ARMOR:
                UpgradeArmor();
                break;
            case PLATING:
                UpgradePlating();
                break;
            case PLASMAS:
                UpgradePlasmas();
                break;
            case TORPEDOES:
                UpgradeTorpedoes();
                break;
            case THRUSTERS:
                UpgradeThrusters();
                break;
            case CAPACITY:
                UpgradeCapacity();
                break;
        }

        heavyFighterShip.RecalculateResources();
    }

    private void UpgradeArmor()
    {
        upgrades[ARMOR]++;
        heavyFighterShip.Hull += 0.5f;
    }

    private void UpgradePlating()
    {
        upgrades[PLATING]++;
        heavyFighterShip.Protection = upgrades[PLATING] * 0.02f;
        heavyFighterShip.Plating++;
    }

    private void UpgradePlasmas()
    {
        upgrades[PLASMAS]++;
        heavyFighterShip.Firepower += 0.75f;
    }

    private void UpgradeTorpedoes()
    {
        heavyFighterShip.Firepower -= upgrades[TORPEDOES] * 0.02f;
        upgrades[TORPEDOES]++;
        heavyFighterShip.Firepower += upgrades[TORPEDOES] * 0.02f;
    }

    private void UpgradeThrusters()
    {
        upgrades[THRUSTERS]++;
        heavyFighterShip.Speed += 0.5f;
    }

    private void UpgradeCapacity()
    {
        upgrades[CAPACITY]++;
        heavyFighterShip.Capacity += 10;
    }

    public override bool Unlock()
    {
        heavyFighterShip.Unlocked = true;
        return heavyFighterShip.Unlocked;
    }

    public override void Display(GameObject panel, int stations) 
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

        foreach (var item in items)
        {
            item.Value.FindChild("CountText").GetComponent<Text>().text = upgrades[item.Key].ToString() + "/10";
            if (CanUpgrade(item.Key, stations) && Unlock())
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

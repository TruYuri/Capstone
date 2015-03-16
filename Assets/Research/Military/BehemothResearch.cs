using UnityEngine;
using System.Collections;

public class BehemothResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";
    private const string THRUSTERS = "Thrusters";
    private const string CAPACITY = "Capacity";

    private Ship behemothShip;

    public BehemothResearch(Ship ship) : base(ship.Name, 5)
    {
        this.behemothShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(TORPEDOES, 0);
        upgrades.Add(THRUSTERS, 0);
        upgrades.Add(CAPACITY, 0);
    }

    public override bool UpgradeResearch(string name, int stations)
    {
        var meetsCriteria = base.UpgradeResearch(name, stations);

        if (!meetsCriteria)
            return false;

        switch (name)
        {
            case ARMOR:
                UpgradeArmor();
                break;
            case PLATING:
                return UpgradePlating();
            case PLASMAS:
                UpgradePlasmas();
                break;
            case TORPEDOES:
                return UpgradeTorpedoes();
            case THRUSTERS:
                UpgradeThrusters();
                break;
            case CAPACITY:
                UpgradeCapacity();
                break;
        }

        return true;
    }

    private void UpgradeArmor()
    {
        upgrades[ARMOR]++;
        behemothShip.Hull += 3.0f;
    }

    private bool UpgradePlating()
    {
        if (upgrades[ARMOR] < 5)
            return false;

        upgrades[PLATING]++;
        behemothShip.Protection = upgrades[PLATING] * 0.02f;
        return true;
    }

    private void UpgradePlasmas()
    {
        upgrades[PLASMAS]++;
        behemothShip.Firepower += 2.0f;
    }

    private bool UpgradeTorpedoes()
    {
        if (upgrades[PLASMAS] < 5)
            return false;

        behemothShip.Firepower -= upgrades[TORPEDOES] * 0.02f;
        upgrades[TORPEDOES]++;
        behemothShip.Firepower += upgrades[TORPEDOES] * 0.02f;
        return true;
    }

    private void UpgradeThrusters()
    {
        upgrades[THRUSTERS]++;
        behemothShip.Speed += 0.25f;
    }

    private void UpgradeCapacity()
    {
        upgrades[CAPACITY]++;
        behemothShip.Capacity += 50;
    }

    public override bool Unlock()
    {
        return true;
    }

    public override void Display()
    {
    }
}
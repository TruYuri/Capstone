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

    public override void UpgradeResearch(string name, int stations)
    {
        base.UpgradeResearch(name, stations);

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
    }

    private void UpgradeArmor()
    {
        upgrades[ARMOR]++;
        behemothShip.Hull += 3.0f;
    }

    private void UpgradePlating()
    {
        if (upgrades[ARMOR] < 5)
            return;

        upgrades[PLATING]++;
        behemothShip.Protection = upgrades[PLATING] * 0.02f;
    }

    private void UpgradePlasmas()
    {
        upgrades[PLASMAS]++;
        behemothShip.Firepower += 2.0f;
    }

    private void UpgradeTorpedoes()
    {
        if (upgrades[PLASMAS] < 5)
            return;

        behemothShip.Firepower -= upgrades[TORPEDOES] * 0.02f;
        upgrades[TORPEDOES]++;
        behemothShip.Firepower += upgrades[TORPEDOES] * 0.02f;
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
using UnityEngine;
using System.Collections;

public class TransportResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string THRUSTERS = "Thrusters";
    private const string CAPACITY = "Capacity";

    private Ship transportShip;

    public TransportResearch(Ship ship) : base(ship.Name, 2)
    {
        this.transportShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
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
        transportShip.Hull += 2.0f;
    }

    private void UpgradePlating()
    {
        if (upgrades[ARMOR] < 5)
            return;

        upgrades[PLATING]++;
        transportShip.Protection = upgrades[PLATING] * 0.02f;
    }

    private void UpgradeThrusters()
    {
        upgrades[THRUSTERS]++;
        transportShip.Speed += 0.5f;
    }

    private void UpgradeCapacity()
    {
        upgrades[CAPACITY]++;
        transportShip.Capacity += 100;
    }

    public override bool Unlock()
    {
        return true;
    }

    public override void Display()
    {
    }
}

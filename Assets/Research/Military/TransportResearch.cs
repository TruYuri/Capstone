
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
                UpgradePlating();
                break;
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
        transportShip.Hull += 2.0f;
    }

    private bool UpgradePlating()
    {
        if (upgrades[ARMOR] < 5)
            return false;

        upgrades[PLATING]++;
        transportShip.Protection = upgrades[PLATING] * 0.02f;
        return true;
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
        transportShip.Unlocked = true;
        return transportShip.Unlocked;
    }

    public override void Display(GameObject panel) 
    {

    }
}

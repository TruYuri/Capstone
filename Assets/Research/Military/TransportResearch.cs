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

    public TransportResearch(Ship ship) : base(ship.Name, 2)
    {
        this.transportShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
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
        transportShip.Unlocked = true;
        return transportShip.Unlocked;
    }

    public override void Display(GameObject panel, int stations) 
    {
        var items = new Dictionary<string, Transform>()
        {
            { THRUSTERS, panel.transform.FindChild("TransportThrustersButton") },
            { CAPACITY, panel.transform.FindChild("TransportCapacityButton") },
            { ARMOR, panel.transform.FindChild("TransportArmorButton") },
            { PLATING, panel.transform.FindChild("TransportAsterminiumButton") },
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
    }
}

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

        transportShip.RecalculateResources();
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
        transportShip.Plating++;
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

    public override bool CanUnlock(Dictionary<Resource, int> resources)
    {
        if (unlocked || transportShip.Unlocked || prereqs == null)
        {
            unlocked = true;
            return true;
        }

        bool unlock = true;

        foreach (var p in prereqs)
            unlock = unlock && p.Unlocked;
        unlock = unlock && transportShip.CanConstruct(resources, 5);

        transportShip.Unlocked = unlocked = unlock;
        return unlock;
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources) 
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
            p2.gameObject.SetActive(true);
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
    }
}

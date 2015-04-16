using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BehemothResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";
    private const string THRUSTERS = "Thrusters";
    private const string CAPACITY = "Capacity";

    private Ship behemothShip;

    public BehemothResearch(Ship ship, List<Research> prereqs)
        : base(ship.Name, 5, prereqs)
    {
        this.behemothShip = ship;
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

        behemothShip.RecalculateResources();
    }

    private void UpgradeArmor()
    {
        upgrades[ARMOR]++;
        behemothShip.Hull += 3.0f;
    }

    private void UpgradePlating()
    {
        upgrades[PLATING]++;
        behemothShip.Protection = upgrades[PLATING] * 0.02f;
        behemothShip.Plating++;

    }

    private void UpgradePlasmas()
    {
        upgrades[PLASMAS]++;
        behemothShip.Firepower += 2.0f;
    }

    private void UpgradeTorpedoes()
    {
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

    public override bool CanUnlock(Dictionary<Resource, int> resources)
    {
        if (unlocked || behemothShip.Unlocked || prereqs == null)
        {
            unlocked = true;
            return true;
        }

        bool unlock = true;

        foreach (var p in prereqs)
            unlock = unlock && p.Unlocked;
        unlock = unlock && behemothShip.CanConstruct(resources, 5);

        behemothShip.Unlocked = unlocked = unlock;
        return unlock;
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources)
    {
        var items = new Dictionary<string, Transform>()
        {
            { TORPEDOES, panel.transform.FindChild("BehemothTorpedoesButton") },
            { THRUSTERS, panel.transform.FindChild("BehemothThrustersButton") },
            { CAPACITY, panel.transform.FindChild("BehemothCapacityButton") },
            { ARMOR, panel.transform.FindChild("BehemothArmorButton") },
            { PLATING, panel.transform.FindChild("BehemothAsterminiumButton") },
            { PLASMAS, panel.transform.FindChild("BehemothPlasmasButton") }
        };

        var p2 = panel.transform.FindChild("BehemothUnlocked");
        var p1 = panel.transform.FindChild("Behemoth");

        p1.gameObject.SetActive(false);
        p2.gameObject.SetActive(false);

        if (unlocked) 
            p2.gameObject.SetActive(true);
        else
        {
            p1.gameObject.SetActive(true);
            p1.GetComponentInChildren<Button>().interactable = CanUnlock(resources);
        }

        foreach(var item in items)
        {
            item.Value.FindChild("CountText").GetComponent<Text>().text = upgrades[item.Key].ToString() + "/10";
            if (CanUpgrade(item.Key, resources[Resource.Stations]) && unlocked)
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
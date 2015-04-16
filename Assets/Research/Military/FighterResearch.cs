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
    }

    public override void UpgradeResearch(string name) 
    {
        switch(name)
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
            case THRUSTERS:
                UpgradeThrusters();
                break;
        }

        fighterShip.RecalculateResources();
    }

    private void UpgradeArmor()
    {
        upgrades[ARMOR]++;
        fighterShip.Hull += 0.25f;
    }

    private void UpgradePlating()
    {
        upgrades[PLATING]++;
        fighterShip.Protection = upgrades[PLATING] * 0.02f;
        fighterShip.Plating++;
    }

    private void UpgradePlasmas()
    {
        upgrades[PLASMAS]++;
        fighterShip.Firepower += 0.25f;
    }

    private void UpgradeThrusters()
    {
        upgrades[THRUSTERS]++;
        fighterShip.Speed += 1.0f;
    }

    public override bool CanUnlock(Dictionary<Resource, int> resources)
    {
        if (unlocked || fighterShip.Unlocked || prereqs == null)
        {
            unlocked = true;
            return true;
        }

        bool unlock = true;

        foreach (var p in prereqs)
            unlock = unlock && p.Unlocked;
        unlock = unlock && fighterShip.CanConstruct(resources, 5);

        fighterShip.Unlocked = unlocked = unlock;
        return unlock;
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

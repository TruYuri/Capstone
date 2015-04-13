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

    public FighterResearch(Ship ship) : base(ship.Name, 1)
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

    public override bool Unlock() 
    {
        fighterShip.Unlocked = true;
        return fighterShip.Unlocked;
    }

    public override void Display(GameObject panel, int stations)  
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
            if (CanUpgrade(item.Key, stations) && Unlock())
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }

        if (upgrades[ARMOR] < 5)
            items[PLATING].GetComponent<Button>().interactable = false;
    }
}

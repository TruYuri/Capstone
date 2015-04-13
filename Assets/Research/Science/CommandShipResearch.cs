using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CommandShipResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";
    private const string THRUSTERS = "Thrusters";

    Ship commandShip;

    public CommandShipResearch(Ship ship) : base(ship.Name, 1)
    {
        this.commandShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(TORPEDOES, 0);
        upgrades.Add(THRUSTERS, 0);
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
        }

        commandShip.RecalculateResources();
    }

    private void UpgradeArmor()
    {
        upgrades[ARMOR]++;
        commandShip.Hull += 3.0f;
    }

    private void UpgradePlating()
    {
        upgrades[PLATING]++;
        commandShip.Protection = upgrades[PLATING] * 0.02f;
        commandShip.Plating++;
    }

    private void UpgradePlasmas()
    {
        upgrades[PLASMAS]++;
        commandShip.Firepower += 2.0f;
    }

    private void UpgradeTorpedoes()
    {
        commandShip.Firepower -= upgrades[TORPEDOES] * 0.02f;
        upgrades[TORPEDOES]++;
        commandShip.Firepower += upgrades[TORPEDOES] * 0.02f;
    }

    private void UpgradeThrusters()
    {
        upgrades[THRUSTERS]++;
        commandShip.Speed += 2.0f;
    }

    public override bool Unlock()
    {
        commandShip.Unlocked = true;
        return commandShip.Unlocked;
    }

    public override void Display(GameObject panel, int stations) 
    {
        var items = new Dictionary<string, Transform>()
        {
            { TORPEDOES, panel.transform.FindChild("CommandTorpedoesButton") },
            { THRUSTERS, panel.transform.FindChild("CommandThrustersButton") },
            { ARMOR, panel.transform.FindChild("CommandArmorButton") },
            { PLATING, panel.transform.FindChild("CommandAsterminiumButton") },
            { PLASMAS, panel.transform.FindChild("CommandPlasmasButton") }
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

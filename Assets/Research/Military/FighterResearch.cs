using UnityEngine;
using System.Collections;

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

    public override bool UpgradeResearch(string name, int stations) 
    {
        var meetsCriteria = base.UpgradeResearch(name, stations);

        if (!meetsCriteria)
            return false;

        switch(name)
        {
            case ARMOR:
                UpgradeArmor();
                break;
            case PLATING:
                return UpgradePlating();
            case PLASMAS:
                UpgradePlasmas();
                break;
            case THRUSTERS:
                UpgradeThrusters();
                break;
        }

        return true;
    }

    private void UpgradeArmor()
    {
        upgrades[ARMOR]++;
        fighterShip.Hull += 0.25f;
    }

    private bool UpgradePlating()
    {
        if (upgrades[ARMOR] < 5)
            return false;

        upgrades[PLATING]++;
        fighterShip.Protection = upgrades[PLATING] * 0.02f;
        return true;
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

    public override void Display(GameObject panel)  
    {
    }
}

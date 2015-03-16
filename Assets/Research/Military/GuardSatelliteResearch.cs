using UnityEngine;
using System.Collections;

public class GuardSatelliteResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";

    private Ship guardSatelliteShip;

    public GuardSatelliteResearch(Ship ship) : base(ship.Name, 3)
    {
        this.guardSatelliteShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(TORPEDOES, 0);
    }

    public override bool UpgradeResearch(string researchName, int stations)
    {
        var meetsCriteria = base.UpgradeResearch(name, stations);

        if (!meetsCriteria)
            return false;

        switch (researchName)
        {
            case ARMOR:
                AdvanceArmor();
                break;
            case PLATING:
                return AdvancePlating();
            case PLASMAS:
                AdvancePlasmas();
                break;
            case TORPEDOES:
                AdvanceTorpedoes();
                break;
        }

        return true;
    }

    private void AdvanceArmor()
    {
        upgrades[ARMOR]++;
        guardSatelliteShip.Hull += 0.5f;
    }

    private bool AdvancePlating()
    {
        if (upgrades[ARMOR] < 5)
            return false;

        upgrades[PLATING]++;
        guardSatelliteShip.Protection = upgrades[PLATING] * 0.02f;
        return true;
    }

    private void AdvancePlasmas()
    {
        upgrades[PLASMAS]++;
        guardSatelliteShip.Firepower += 1.0f;
    }

    private void AdvanceTorpedoes()
    {
        guardSatelliteShip.Firepower -= upgrades[TORPEDOES] * 0.02f;
        upgrades[TORPEDOES]++;
        guardSatelliteShip.Firepower += upgrades[TORPEDOES] * 0.02f;
    }

    public override bool Unlock()
    {
        return true;
    }

    public override void Display()
    {
    }
}

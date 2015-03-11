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

    public override void UpgradeResearch(string researchName, int stations)
    {
        base.UpgradeResearch(researchName, stations);

        switch (researchName)
        {
            case ARMOR:
                AdvanceArmor();
                break;
            case PLATING:
                AdvancePlating();
                break;
            case PLASMAS:
                AdvancePlasmas();
                break;
            case TORPEDOES:
                AdvanceTorpedoes();
                break;
        }
    }

    private void AdvanceArmor()
    {
        upgrades[ARMOR]++;
        guardSatelliteShip.Hull += 0.5f;
    }

    private void AdvancePlating()
    {
        if (upgrades[ARMOR] < 5)
            return;

        upgrades[PLATING]++;
        guardSatelliteShip.Protection = upgrades[PLATING] * 0.02f;
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

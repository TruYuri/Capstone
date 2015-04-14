using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GuardSatelliteResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";

    private Ship guardSatelliteShip;

    public GuardSatelliteResearch(Ship ship, List<Research> prereqs) 
        : base(ship.Name, 3, prereqs)
    {
        this.guardSatelliteShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(TORPEDOES, 0);
    }

    public override void UpgradeResearch(string researchName)
    {
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

        guardSatelliteShip.RecalculateResources();
    }

    private void AdvanceArmor()
    {
        upgrades[ARMOR]++;
        guardSatelliteShip.Hull += 0.5f;
    }

    private void AdvancePlating()
    {
        upgrades[PLATING]++;
        guardSatelliteShip.Protection = upgrades[PLATING] * 0.02f;
        guardSatelliteShip.Plating++;
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
        guardSatelliteShip.Unlocked = true;
        return guardSatelliteShip.Unlocked;
    }

    public override void Display(GameObject panel, int stations) 
    {
        var items = new Dictionary<string, Transform>()
        {
            { TORPEDOES, panel.transform.FindChild("GuardTorpedoesButton") },
            { ARMOR, panel.transform.FindChild("GuardArmorButton") },
            { PLATING, panel.transform.FindChild("GuardAsterminiumButton") },
            { PLASMAS, panel.transform.FindChild("GuardPlasmasButton") }
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

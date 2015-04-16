using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// This class upgrades various concepts but also acts as the base for the Resource Transport, thus is a military script.
public class ComplexResearch : Research
{
    private const string DEFENSE = "Defense";
    private const string CAPACITY = "Capacity";

    private Dictionary<string, Structure> structureDefinitions;

    public ComplexResearch(Dictionary<string, Ship> shipDefinitions, List<Research> prereqs)
        : base("Complex", 3, prereqs)
    {
        structureDefinitions = new Dictionary<string, Structure>()
        {
            { "Gathering Complex", shipDefinitions["Gathering Complex"] as Structure },
            { "Research Complex", shipDefinitions["Research Complex"] as Structure },
            { "Military Complex", shipDefinitions["Military Complex"] as Structure },
            { "Base", shipDefinitions["Base"] as Structure }
        };

        upgrades.Add(DEFENSE, 0);
        upgrades.Add(CAPACITY, 0);
    }

    public override void UpgradeResearch(string name)
    {
        base.UpgradeResearch(name);

        switch (name)
        {
            case DEFENSE:
                UpgradeDefense();
                break;
            case CAPACITY:
                UpgradeCapacity();
                break;
        }

        foreach (var struc in structureDefinitions)
            struc.Value.RecalculateResources();
    }

    private void UpgradeDefense()
    {
        upgrades[DEFENSE]++;
    }

    private void UpgradeCapacity()
    {
        upgrades[CAPACITY]++;
    }

    public override bool CanUnlock(Dictionary<Resource, int> resources)
    {
        structureDefinitions["Gathering Complex"].Unlocked = true;
        structureDefinitions["Research Complex"].Unlocked = true;
        structureDefinitions["Military Complex"].Unlocked = true;
        structureDefinitions["Base"].Unlocked = true;
        return true;
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources)
    {
        var items = new Dictionary<string, Transform>()
        {
            { DEFENSE, panel.transform.FindChild("ComplexDefenseButton") },
            { CAPACITY, panel.transform.FindChild("ComplexCapacityButton") },
        };

        foreach (var item in items)
        {
            item.Value.FindChild("CountText").GetComponent<Text>().text = upgrades[item.Key].ToString() + "/10";
            if (CanUpgrade(item.Key, resources[Resource.Stations]) && CanUnlock(resources))
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }
    }
}


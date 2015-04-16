using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// This class upgrades various concepts but also acts as the base for the Resource Transport, thus is a military script.
public class WarpPortalResearch : Research
{
    private const string RANGE = "Range";
    private const string DEFENSE = "Defense";

    private Structure warpPortal;

    public WarpPortalResearch(Structure warpPortal, List<Research> prereqs)
        : base(warpPortal.Name, 3, prereqs)
    {
        this.warpPortal = warpPortal;
        upgrades.Add(RANGE, 0);
        upgrades.Add(DEFENSE, 0);
    }

    public override void UpgradeResearch(string name)
    {
        switch(name)
        {
            case RANGE:
                UpgradeRange();
                break;
            case DEFENSE:
                UpgradeDefense();
                break;
        }

        warpPortal.RecalculateResources();
    }

    private void UpgradeRange()
    {
        upgrades[RANGE]++;
        warpPortal.Range++;
    }

    private void UpgradeDefense()
    {
        upgrades[DEFENSE]++;
        warpPortal.Hull += 5.0f;
    }

    public override bool CanUnlock(Dictionary<Resource, int> resources)
    {
        warpPortal.Unlocked = true;
        return warpPortal.Unlocked;
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources)
    {
        var items = new Dictionary<string, Transform>()
        {
            { DEFENSE, panel.transform.FindChild("WarpDefenseButton") },
            { RANGE, panel.transform.FindChild("WarpRangeButton") },
        };

        var p2 = panel.transform.FindChild("WarpPortalUnlocked");
        var p1 = panel.transform.FindChild("WarpPortal");

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
    }
}

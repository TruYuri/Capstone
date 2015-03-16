using UnityEngine;
using System.Collections.Generic;

// This class upgrades various concepts but also acts as the base for the Resource Transport, thus is a military script.
public class WarpPortalResearch : Research
{
    private const string RANGE = "Range";
    private const string DEFENSE = "Defense";

    private WarpPortal warpPortal;

    public WarpPortalResearch(WarpPortal warpPortal) : base(warpPortal.Name, 3)
    {
        this.warpPortal = warpPortal;
        upgrades.Add(RANGE, 0);
        upgrades.Add(DEFENSE, 0);
    }

    public override bool UpgradeResearch(string name, int stations)
    {
        var meetsCriteria = base.UpgradeResearch(name, stations);

        if (!meetsCriteria)
            return false;

        switch(name)
        {
            case RANGE:
                UpgradeRange();
                break;
            case DEFENSE:
                UpgradeDefense();
                break;
        }

        return true;
    }

    private void UpgradeRange()
    {
        upgrades[RANGE]++;
        warpPortal.Range += 1;
    }

    private void UpgradeDefense()
    {
        upgrades[DEFENSE]++;
        warpPortal.Hull += 5.0f;
    }
}

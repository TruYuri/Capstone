using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// This class upgrades various concepts but also acts as the base for the Resource Transport, thus is a military script.
public class RelayResearch : Research
{
    private const string RANGE = "Range";
    private const string DEFENSE = "Defense";

    private Structure relay;

    public RelayResearch(Structure relay) : base(relay.Name, 3)
    {
        this.relay = relay;
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
    }

    private void UpgradeRange()
    {
        upgrades[RANGE]++;
        relay.Range += 1;
    }

    private void UpgradeDefense()
    {
        upgrades[DEFENSE]++;
        relay.Hull += 5.0f;
    }

    public override bool Unlock()
    {
        relay.Unlocked = true;
        return relay.Unlocked;
    }

    public override void Display(GameObject panel, int stations)
    {
        var items = new Dictionary<string, Transform>()
        {
            { DEFENSE, panel.transform.FindChild("RelayDefenseButton") },
            { RANGE, panel.transform.FindChild("RelayRangeButton") },
        };

        foreach (var item in items)
        {
            item.Value.FindChild("CountText").GetComponent<Text>().text = upgrades[item.Key].ToString() + "/10";
            if (CanUpgrade(item.Key, stations) && Unlock())
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }
    }
}

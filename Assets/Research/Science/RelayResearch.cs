using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// This class upgrades various concepts but also acts as the base for the Resource Transport, thus is a military script.
public class RelayResearch : Research
{
    private const string RANGE = "Range";
    private const string DEFENSE = "Defense";

    private Structure relay;

    public RelayResearch(Structure relay, List<Research> prereqs) 
        : base(relay.Name, 3, prereqs)
    {
        this.relay = relay;
        upgrades.Add(RANGE, 0);
        upgrades.Add(DEFENSE, 0);

        foreach (var upgrade in upgrades)
        {
            costs.Add(upgrade.Key, new Dictionary<Resource, int>()
                { 
                    { Resource.Asterminium, 0 },
                    { Resource.Forest, 0 },
                    { Resource.Oil, 0 },
                    { Resource.Ore, 0 },
                    { Resource.Stations, 0 }
                });
        }

        RecalculateResourceCosts();
    }

    public override Dictionary<Resource, int> UpgradeResearch(string name)
    {
        switch(name)
        {
            case RANGE:
                upgrades[RANGE]++;
                relay.Range++;
                break;
            case DEFENSE:
                upgrades[DEFENSE]++;
                relay.Hull += 5.0f;
                break;
        }

        relay.RecalculateResources();
        var r = costs[name];
        RecalculateResourceCosts();
        return r;
    }

    private void RecalculateResourceCosts()
    {
        costs[DEFENSE] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[DEFENSE] + 1) * relay.Hull * relay.Firepower / 2.0f) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[DEFENSE] + 1) * relay.Hull * relay.Firepower / 2.0f) }
        };

        costs[RANGE] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, (upgrades[RANGE] + 1) * relay.Range * 2 }
        };
    }

    public override Dictionary<Resource, int> Unlock()
    {
        base.Unlock();
        relay.Unlocked = true;
        return new Dictionary<Resource, int>();
    }

    public override bool CanUnlock(Dictionary<Resource, int> resources)
    {
        if (unlocked || relay.Unlocked)
            return true;

        bool unlock = true;

        unlock = unlock && relay.CanConstruct(resources, 5);

        relay.Unlocked = unlocked = unlock;
        return unlock;
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources)
    {
        var items = new Dictionary<string, Transform>()
        {
            { DEFENSE, panel.transform.FindChild("RelayDefenseButton") },
            { RANGE, panel.transform.FindChild("RelayRangeButton") },
        };

        var p2 = panel.transform.FindChild("RelayUnlocked");
        var p1 = panel.transform.FindChild("Relay");

        p1.gameObject.SetActive(false);
        p2.gameObject.SetActive(false);

        if (unlocked)
        {
            p2.gameObject.SetActive(true);
            p2.FindChild("StatsRangeText").GetComponent<Text>().text = "Range: " + relay.Range.ToString();
            p2.FindChild("StatsDefenseText").GetComponent<Text>().text = "Hull: " + relay.Hull.ToString();
        }
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

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

        RecalculateResourceCosts(0);
    }

    public override Dictionary<Resource, int> UpgradeResearch(string name, float reduction)
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
        RecalculateResourceCosts(reduction);
        return r;
    }

    private void RecalculateResourceCosts(float reduction)
    {
        costs[DEFENSE] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[DEFENSE] + 1) * relay.Hull * relay.Firepower / 2.0f * (1.0f - reduction)) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[DEFENSE] + 1) * relay.Hull * relay.Firepower / 2.0f * (1.0f - reduction)) }
        };

        costs[RANGE] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, Mathf.CeilToInt((upgrades[RANGE] + 1) * relay.Range * 2  * (1.0f - reduction))}
        };

        var types = new List<Resource>() { Resource.Asterminium, Resource.Ore, Resource.Oil, Resource.Forest };
        foreach (var c in costs)
        {
            foreach (var t in types)
                if (!c.Value.ContainsKey(t))
                    c.Value.Add(t, 0);
        }
    }

    public override Dictionary<Resource, int> Unlock(float reduction)
    {
        base.Unlock(reduction);
        relay.Unlocked = true;
        return relay.CanConstruct(null, 5, reduction).Value;
    }

    public override bool CanUnlock(Dictionary<Resource, int> resources, float reduction)
    {
        if (unlocked || relay.Unlocked)
            return true;

        bool unlock = true;

        unlock = relay.CanConstruct(resources, 5, reduction).Key;

        return unlock;
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources, float reduction)
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
            p1.GetComponentInChildren<Button>().interactable = CanUnlock(resources, reduction);
        }

        foreach (var item in items)
        {
            item.Value.FindChild("CountText").GetComponent<Text>().text = upgrades[item.Key].ToString() + "/10";
            if (CanUpgrade(item.Key, resources, reduction) && unlocked)
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }
    }
}

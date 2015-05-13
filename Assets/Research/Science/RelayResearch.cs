using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// This class upgrades and acts as the base for the Relay.
/// </summary>
/// 
public class RelayResearch : Research
{
    private const string RANGE = "Range";
    private const string DEFENSE = "Defense";

    private Structure relay;

    /// <summary>
    /// Relay Constructor
    /// </summary>
    /// <param name="relay"></param>
    /// <param name="prereqs"></param>
    public RelayResearch(Structure relay, List<Research> prereqs) 
        : base(relay.Name, 4, prereqs)
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

    /// <summary>
    /// Upgrades the level of the Research
    /// </summary>
    /// <param name="name"></param>
    /// <param name="reduction">Reduction in cost of current Research from completed Research</param>
    /// <returns></returns>
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

    /// <summary>
    /// Recalculates the costs after an upgrade
    /// </summary>
    /// <param name="reduction">Reduction in cost of current Research from completed Research</param>
    private void RecalculateResourceCosts(float reduction)
    {
        costs[DEFENSE] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[DEFENSE] + 1) * relay.Hull * (1.0f - reduction)) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[DEFENSE] + 1) * relay.Hull * (1.0f - reduction)) }
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

    /// <summary>
    /// Unlocks research
    /// </summary>
    /// <param name="reduction"></param>
    /// <returns></returns>
    public override Dictionary<Resource, int> Unlock(float reduction)
    {
        base.Unlock(reduction);
        relay.Unlocked = true;
        return relay.CanConstruct(null, 5, reduction).Value;
    }

    /// <summary>
    /// Determines if it can be unlocked
    /// </summary>
    /// <param name="resources">Resources needed to obtain</param>
    /// <param name="reduction">Reduction in cost of current Research from completed Research</param>
    /// <returns></returns>
    public override bool CanUnlock(Dictionary<Resource, int> resources, float reduction)
    {
        if (unlocked || relay.Unlocked)
            return true;

        bool unlock = true;

        unlock = relay.CanConstruct(resources, 5, reduction).Key;

        return unlock;
    }

    /// <summary>
    /// Change info displayed on the panels for the Research
    /// </summary>
    /// <param name="panel"></param>
    /// <param name="resources"></param>
    /// <param name="reduction">Reduction in cost of current Research from completed Research</param>
    public override void Display(GameObject panel, Dictionary<Resource, int> resources, float reduction)
    {
        RecalculateResourceCosts(reduction);

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
            var r = relay.CanConstruct(null, 5, reduction).Value;

            if (!costs.ContainsKey("Unlock"))
                costs.Add("Unlock", null);

            costs["Unlock"] = r;

            p1.gameObject.SetActive(true);
            p1.GetComponentInChildren<Button>().interactable = CanUnlock(resources, reduction);
            p1.FindChild("ForestText").GetComponent<Text>().text = costs["Unlock"][Resource.Forest].ToString();
            p1.FindChild("OreText").GetComponent<Text>().text = costs["Unlock"][Resource.Ore].ToString();
            p1.FindChild("OilText").GetComponent<Text>().text = costs["Unlock"][Resource.Oil].ToString();
            p1.FindChild("AsterminiumText").GetComponent<Text>().text = costs["Unlock"][Resource.Asterminium].ToString();
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

    /// <summary>
    /// Displays popups for the upgrade
    /// </summary>
    /// <param name="panel">The panel of the button hovered over</param>
    /// <param name="upgrade">Name of upgrade</param>
    /// <param name="reduction">Reduction in cost of current Research from completed Research</param>
    public override void DisplayPopup(GameObject panel, string upgrade, float reduction)
    {
        if (upgrade == "Unlock")
        {
            var r = relay.CanConstruct(null, 5, reduction).Value;

            if (!costs.ContainsKey("Unlock"))
                costs.Add("Unlock", null);

            costs["Unlock"] = r;
        }

        base.DisplayPopup(panel, upgrade, reduction);
    }
}

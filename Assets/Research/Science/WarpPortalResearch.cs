﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// This class upgrades and acts as the base for the Warp Portal.
/// </summary>
public class WarpPortalResearch : Research
{
    private const string RANGE = "Range";
    private const string DEFENSE = "Defense";

    private Structure warpPortal;

    /// <summary>
    /// Constructor for Warp Portal
    /// </summary>
    /// <param name="warpPortal"></param>
    /// <param name="prereqs">Research prerequisites</param>
    public WarpPortalResearch(Structure warpPortal, List<Research> prereqs)
        : base(warpPortal.Name, 5, prereqs)
    {
        this.warpPortal = warpPortal;
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
    }

    /// <summary>
    /// Upgrade
    /// </summary>
    /// <param name="name"></param>
    /// <param name="reduction">Reduction in cost from completed Research</param>
    /// <returns></returns>
    public override Dictionary<Resource, int> UpgradeResearch(string name, float reduction)
    {
        switch(name)
        {
            case RANGE:
                upgrades[RANGE]++;
                warpPortal.Range++;
                break;
            case DEFENSE:
                upgrades[DEFENSE]++;
                warpPortal.Hull += 5.0f;
                break;
        }

        warpPortal.RecalculateResources();
        var r = costs[name];
        RecalculateResourceCosts(reduction);
        return r;
    }

    /// <summary>
    /// Get the new cost of Upgrade and Unit
    /// </summary>
    /// <param name="reduction"></param>
    private void RecalculateResourceCosts(float reduction)
    {
        costs[DEFENSE] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[DEFENSE] + 1) * warpPortal.Hull * (1.0f - reduction)) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[DEFENSE] + 1) * warpPortal.Hull * (1.0f - reduction)) }
        };

        costs[RANGE] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, Mathf.CeilToInt((upgrades[RANGE] + 1) * warpPortal.Range * 5 * (1.0f - reduction)) }
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
    /// Unlock the Research
    /// </summary>
    /// <param name="reduction"></param>
    /// <returns></returns>
    public override Dictionary<Resource, int> Unlock(float reduction)
    {
        base.Unlock(reduction);
        warpPortal.Unlocked = true;
        return warpPortal.CanConstruct(null, 5, reduction).Value;
    }

    /// <summary>
    /// Able to Unlock Research
    /// </summary>
    /// <param name="resources"></param>
    /// <param name="reduction"></param>
    /// <returns></returns>
    public override bool CanUnlock(Dictionary<Resource, int> resources, float reduction)
    {
        if (unlocked || warpPortal.Unlocked)
            return true;

        bool unlock = true;

        foreach (var p in prereqs)
            unlock = unlock && p.Unlocked;
        unlock = unlock && warpPortal.CanConstruct(resources, 5, reduction).Key;

        return unlock;
    }

    /// <summary>
    /// Change info displayed on the panels for the Research
    /// </summary>
    /// <param name="panel"></param>
    /// <param name="resources"></param>
    /// <param name="reduction"></param>
    public override void Display(GameObject panel, Dictionary<Resource, int> resources, float reduction)
    {
        RecalculateResourceCosts(reduction);

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
        {
            p2.gameObject.SetActive(true);
            p2.FindChild("StatsRangeText").GetComponent<Text>().text = "Range: " + warpPortal.Range.ToString();
            p2.FindChild("StatsDefenseText").GetComponent<Text>().text = "Hull: " + warpPortal.Hull.ToString();
        }
        else
        {
            var r = warpPortal.CanConstruct(null, 5, reduction).Value;

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
    /// Display popup
    /// </summary>
    /// <param name="panel"></param>
    /// <param name="upgrade"></param>
    /// <param name="reduction"></param>
    public override void DisplayPopup(GameObject panel, string upgrade, float reduction)
    {
        if (upgrade == "Unlock")
        {
            var r = warpPortal.CanConstruct(null, 5, reduction).Value;

            if (!costs.ContainsKey("Unlock"))
                costs.Add("Unlock", null);

            costs["Unlock"] = r;
        }

        base.DisplayPopup(panel, upgrade, reduction);
    }
}

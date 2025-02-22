﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// This class upgrades and acts as the base for the Command Ship.
/// </summary>
public class CommandShipResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";
    private const string THRUSTERS = "Thrusters";

    Ship commandShip;

    /// <summary>
    /// Command Ship Constructor
    /// </summary>
    /// <param name="ship">The ship</param>
    /// <param name="prereqs"></param>
    public CommandShipResearch(Ship ship, List<Research> prereqs)
        : base(ship.Name, 1, prereqs)
    {
        this.commandShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(TORPEDOES, 0);
        upgrades.Add(THRUSTERS, 0);

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

        commandShip.Unlocked = unlocked = true;
        RecalculateResourceCosts(0);
    }

    /// <summary>
    /// Upgrade the level of the Research
    /// </summary>
    /// <param name="name"></param>
    /// <param name="reduction">Reduction in cost of current Research from completed Research</param>
    /// <returns></returns>
    public override Dictionary<Resource, int> UpgradeResearch(string name, float reduction)
    {
        switch (name)
        {
            case ARMOR:
                upgrades[ARMOR]++;
                commandShip.Hull += 3.0f;
                break;
            case PLATING:
                upgrades[PLATING]++;
                commandShip.Protection = upgrades[PLATING] * 0.02f;
                commandShip.Plating++;
                break;
            case PLASMAS:
                upgrades[PLASMAS]++;
                commandShip.Firepower += 2.0f;
                break;
            case TORPEDOES:
                commandShip.Firepower -= upgrades[TORPEDOES] * 0.02f;
                upgrades[TORPEDOES]++;
                commandShip.Firepower += upgrades[TORPEDOES] * 0.02f;
                break;
            case THRUSTERS:
                upgrades[THRUSTERS]++;
                commandShip.Speed += 2.0f;
                break;
        }

        commandShip.RecalculateResources();
        var r = costs[name];
        RecalculateResourceCosts(reduction);
        return r;
    }

    /// <summary>
    /// Recalculate the costs after upgrading
    /// </summary>
    /// <param name="reduction">Reduction in cost of current Research from completed Research</param>
    private void RecalculateResourceCosts(float reduction)
    {
        costs[ARMOR] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[ARMOR] + 1) * 3 * commandShip.Hull * (1.0f - reduction)) }
        };

        costs[PLATING] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, Mathf.CeilToInt((upgrades[PLATING] + 1) * commandShip.Hull * (1.0f - reduction)) }
        };

        costs[PLASMAS] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[PLASMAS] + 1) * 2 * commandShip.Firepower * (1.0f - reduction)) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[PLASMAS] + 1) * 2 * commandShip.Firepower * (1.0f - reduction)) }
        };

        costs[TORPEDOES] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, Mathf.CeilToInt((upgrades[TORPEDOES] + 1) * commandShip.Firepower * (1.0f - reduction)) }
        };

        costs[THRUSTERS] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[THRUSTERS] + 1) * 2f * commandShip.Speed * 10f * (1.0f - reduction)) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[THRUSTERS] + 1) * 2f * commandShip.Speed * 10f * (1.0f - reduction)) }
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
    /// Changes the display panels for the Research
    /// </summary>
    /// <param name="panel">Panel of button being hovered over</param>
    /// <param name="resources">Resources needed to obtain</param>
    /// <param name="reduction">Reduction in cost of current Research from completed Research</param>
    public override void Display(GameObject panel, Dictionary<Resource, int> resources, float reduction)
    {
        RecalculateResourceCosts(reduction);

        var items = new Dictionary<string, Transform>()
        {
            { TORPEDOES, panel.transform.FindChild("CommandTorpedoesButton") },
            { THRUSTERS, panel.transform.FindChild("CommandThrustersButton") },
            { ARMOR, panel.transform.FindChild("CommandArmorButton") },
            { PLATING, panel.transform.FindChild("CommandAsterminiumButton") },
            { PLASMAS, panel.transform.FindChild("CommandPlasmasButton") }
        };

        var p2 = panel.transform.FindChild("Command");

        p2.FindChild("StatsSpeedText").GetComponent<Text>().text = "Speed: " + commandShip.Speed.ToString();
        p2.FindChild("StatsFirepowerText").GetComponent<Text>().text = "Firepower: " + commandShip.Firepower.ToString();
        p2.FindChild("StatsHullText").GetComponent<Text>().text = "Hull: " + commandShip.Hull.ToString();
        p2.FindChild("StatsCapacityText").GetComponent<Text>().text = "Capacity: " + commandShip.Capacity.ToString();

        foreach (var item in items)
        {
            item.Value.FindChild("CountText").GetComponent<Text>().text = upgrades[item.Key].ToString() + "/10";
            if (CanUpgrade(item.Key, resources, reduction) && unlocked)
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }

        if (upgrades[ARMOR] < 5)
            items[PLATING].GetComponent<Button>().interactable = false;

        if (upgrades[PLASMAS] < 5)
            items[TORPEDOES].GetComponent<Button>().interactable = false;
    }
}

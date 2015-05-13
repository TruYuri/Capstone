using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Research Screens Overhead, which all Research inherits from
/// </summary>
public class Research
{
    protected string name;
    protected int level;
    protected Dictionary<string, int> upgrades;
    protected List<Research> prereqs;
    protected Dictionary<string, Dictionary<Resource, int>> costs;
    protected bool unlocked;

    public bool Unlocked { get { return unlocked; } }
    public string Name { get { return name; } }

    /// <summary>
    /// Constructor for Research Overhead
    /// </summary>
    /// <param name="name">Research Name</param>
    /// <param name="level">Research Level</param>
    /// <param name="prereqs">Research Prerequisites</param>
    public Research(string name, int level, List<Research> prereqs)
    {
        this.costs = new Dictionary<string, Dictionary<Resource, int>>();
        this.upgrades = new Dictionary<string, int>();
        this.prereqs = prereqs;
        this.level = level;
        this.name = name;
    }

    /// <summary>
    /// Determines if possible to upgrade to next level of Research
    /// </summary>
    /// <param name="name"></param>
    /// <param name="resources">Resources needed to obtain</param>
    /// <param name="reduction">Reduction amount from research already achieved</param>
    /// <returns></returns>
    protected bool CanUpgrade(string name, Dictionary<Resource, int> resources, float reduction)
    {
        var invalidLevel = upgrades[name] >= 10;
        var invalidStations = resources[Resource.Stations] < (upgrades[name] + 1) * (level);

        if (invalidLevel || invalidStations)
            return false;

        if (resources[Resource.Asterminium] >= costs[name][Resource.Asterminium] * (1.0f - reduction) &&
           resources[Resource.Ore] >= costs[name][Resource.Ore] * (1.0f - reduction) &&
           resources[Resource.Oil] >= costs[name][Resource.Oil] * (1.0f - reduction) &&
           resources[Resource.Forest] >= costs[name][Resource.Forest] * (1.0f - reduction))
            return true;

        return false;
    }

    /// <summary>
    /// Virtual for upgrading individual research
    /// </summary>
    /// <param name="name"></param>
    /// <param name="reduction">Reduction amount for Research already achieved</param>
    /// <returns></returns>
    public virtual Dictionary<Resource, int> UpgradeResearch(string name, float reduction) { return new Dictionary<Resource, int>(); }

    /// <summary>
    /// Virtual for unlocking new Research
    /// </summary>
    /// <param name="reduction">Reduction amount from Research already achieved</param>
    /// <returns></returns>
    public virtual Dictionary<Resource, int> Unlock(float reduction)
    {
        unlocked = true;
        return new Dictionary<Resource, int>();
    }

    /// <summary>
    /// Virtual for determining if new Research can be unlocked
    /// </summary>
    /// <param name="resources">Resources needed to obtain</param>
    /// <param name="reduction">Reduction amount from Research already achieved</param>
    /// <returns></returns>
    public virtual bool CanUnlock(Dictionary<Resource, int> resources, float reduction)
    {
        return true;
    }

    /// <summary>
    /// Virtual for displaying the current Research values
    /// </summary>
    /// <param name="panel"></param>
    /// <param name="resources">Resources needed to obtain</param>
    /// <param name="reduction">Reduction amount from Research already achieved</param>
    public virtual void Display(GameObject panel, Dictionary<Resource, int> resources, float reduction) { }

    /// <summary>
    /// Virtual for displaying popup with how much unlocking costs
    /// </summary>
    /// <param name="panel">Panel of button being hovered over</param>
    /// <param name="upgrade">Name of upgrade</param>
    /// <param name="reduction">Reduction amount from Research already achieved</param>
    public virtual void DisplayPopup(GameObject panel, string upgrade, float reduction) 
    {
        panel.transform.FindChild("ForestText").GetComponent<Text>().text = costs[upgrade][Resource.Forest].ToString();
        panel.transform.FindChild("OreText").GetComponent<Text>().text = costs[upgrade][Resource.Ore].ToString();
        panel.transform.FindChild("OilText").GetComponent<Text>().text = costs[upgrade][Resource.Oil].ToString();
        panel.transform.FindChild("AsterminiumText").GetComponent<Text>().text = costs[upgrade][Resource.Asterminium].ToString();

        if (upgrade != "Unlock")
            panel.transform.FindChild("BasesText").GetComponent<Text>().text = ((upgrades[upgrade] + 1) * (level)).ToString() + " Base or\nResearch Complex";
        else if (prereqs != null)
        {
            // show prereqs instead
            string pstr = string.Empty;
            foreach (var p in prereqs)
                pstr += p.Name;

            panel.transform.FindChild("BasesText").GetComponent<Text>().text = "Required research:\n" + pstr;
        }
        else
            panel.transform.FindChild("BasesText").GetComponent<Text>().text = "";
    }
}

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generic view of the Research Tree
/// </summary>
public class ResearchTree
{
    private int maxLevel;
    private Research[] tree;

    /// <summary>
    /// Constructor for ResearchTree
    /// </summary>
    /// <param name="nLevels">Number of Research Levels</param>
    public ResearchTree(int nLevels)
    {
        maxLevel = nLevels;
        tree = new Research[maxLevel];
    }

    /// <summary>
    /// Adds new Research
    /// </summary>
    /// <param name="level">Current level</param>
    /// <param name="research">Research being added to tree</param>
    public void AddResearch(int level, Research research)
    {
        tree[level - 1] = research;
    }

    /// <summary>
    /// Gets Research from current level
    /// </summary>
    /// <param name="level">Current level</param>
    /// <returns></returns>
    public Research GetResearch(int level)
    {
        if (level < 1 || level > maxLevel)
            return null;

        return tree[level - 1];
    }

    /// <summary>
    /// Gets Research of given name
    /// </summary>
    /// <param name="name">Research Name</param>
    /// <returns></returns>
    public Research GetResearch(string name)
    {
        for (int i = 0; i < maxLevel; i++)
            if (tree[i].Name == name)
                return tree[i];

        return null;
    }

}

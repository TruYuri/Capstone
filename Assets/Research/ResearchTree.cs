using UnityEngine;
using System.Collections.Generic;

public class ResearchTree
{
    private int maxLevel;
    private int currentLevel;
    private Research[] tree;

    public ResearchTree(int nLevels)
    {
        maxLevel = nLevels;
        tree = new Research[maxLevel];
    }

    public void AddResearch(int level, Research research)
    {
        tree[level - 1] = research;
    }

    public Research GetResearch(int level)
    {
        if (level < 1 || level > maxLevel)
            return null;

        return tree[level - 1];
    }

    public Research GetResearch(string name)
    {
        for (int i = 0; i < maxLevel; i++)
            if (tree[i].Name == name)
                return tree[i];

        return null;
    }

    public void Advance()
    {
        for (int i = 0; i < maxLevel; i++)
            tree[i].Unlock();
    }

    public void Display()
    {

    }
}

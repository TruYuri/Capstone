using UnityEngine;
using System.Collections;

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

    public bool Advance()
    {
        if(maxLevel > currentLevel && tree[currentLevel].Unlock())
        {
            currentLevel++;
            return true;
        }

        return false;
    }
}

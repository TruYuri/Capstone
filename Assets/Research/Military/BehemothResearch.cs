using UnityEngine;
using System.Collections;

public class BehemothResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";
    private const string THRUSTERS = "Thrusters";
    private const string CAPACITY = "Capacity";

    public BehemothResearch() : base("Heavy Fighter", 5, 20, 10, 1, 50)
    {
        researchables.Add(ARMOR, 0);
        researchables.Add(PLATING, 0);
        researchables.Add(PLASMAS, 0);
        researchables.Add(TORPEDOES, 0);
        researchables.Add(THRUSTERS, 0);
        researchables.Add(CAPACITY, 0);
    }

    public override void AdvanceResearchLevel(string researchName, int stations)
    {
        base.AdvanceResearchLevel(researchName, stations);

        switch (researchName)
        {
            case ARMOR:
                AdvanceArmor();
                break;
            case PLATING:
                AdvancePlating();
                break;
            case PLASMAS:
                AdvancePlasmas();
                break;
            case TORPEDOES:
                AdvanceTorpedoes();
                break;
            case THRUSTERS:
                AdvanceThrusters();
                break;
            case CAPACITY:
                AdvanceCapacity();
                break;
        }
    }

    private void AdvanceArmor()
    {
        researchables[ARMOR]++;
        hull += 3.0f;
    }

    private void AdvancePlating()
    {
        if (researchables[ARMOR] < 5)
            return;

        researchables[PLATING]++;
        protection = researchables[PLATING] * 0.02f;
    }

    private void AdvancePlasmas()
    {
        researchables[PLASMAS]++;
        firepower += 2.0f;
    }

    private void AdvanceTorpedoes()
    {
        if (researchables[TORPEDOES] < 5)
            return;

        firepower -= researchables[TORPEDOES] * 0.02f;
        researchables[TORPEDOES]++;
        firepower += researchables[TORPEDOES] * 0.02f;
    }

    private void AdvanceThrusters()
    {
        researchables[THRUSTERS]++;
        speed += 0.25f;
    }

    private void AdvanceCapacity()
    {
        researchables[CAPACITY]++;
        capacity += 50;
    }

    public override bool Unlock()
    {
        return true;
    }

    public override void Display()
    {
    }
}
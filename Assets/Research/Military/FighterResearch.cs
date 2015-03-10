using UnityEngine;
using System.Collections;

public class FighterResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string THRUSTERS = "Thrusters";

    public FighterResearch() : base("Fighter", 1, 1, 1, 5, 0)
    {
        researchables.Add(ARMOR, 0);
        researchables.Add(PLATING, 0);
        researchables.Add(PLASMAS, 0);
        researchables.Add(THRUSTERS, 0);
    }

    public override void AdvanceResearchLevel(string researchName, int stations) 
    {
        base.AdvanceResearchLevel(researchName, stations);

        switch(researchName)
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
            case THRUSTERS:
                AdvanceThrusters();
                break;
        }
    }

    private void AdvanceArmor()
    {
        researchables[ARMOR]++;
        hull += 0.25f;
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
        firepower += 0.25f;
    }

    private void AdvanceThrusters()
    {
        researchables[THRUSTERS]++;
        speed += 1.0f;
    }

    public override bool Unlock() 
    { 
        return true;
    }

    public override void Display() 
    {
    }
}

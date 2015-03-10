using UnityEngine;
using System.Collections;

public class GuardSatelliteResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";

    public GuardSatelliteResearch() : base("Guard Satellite", 3, 2, 3, 0, 0)
    {
        researchables.Add(ARMOR, 0);
        researchables.Add(PLATING, 0);
        researchables.Add(PLASMAS, 0);
        researchables.Add(TORPEDOES, 0);
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
        }
    }

    private void AdvanceArmor()
    {
        researchables[ARMOR]++;
        hull += 0.5f;
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
        firepower += 1.0f;
    }

    private void AdvanceTorpedoes()
    {
        firepower -= researchables[TORPEDOES] * 0.02f;
        researchables[TORPEDOES]++;
        firepower += researchables[TORPEDOES] * 0.02f;
    }

    public override bool Unlock()
    {
        return true;
    }

    public override void Display()
    {
    }
}

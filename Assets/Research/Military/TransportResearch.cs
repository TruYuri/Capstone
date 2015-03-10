using UnityEngine;
using System.Collections;

public class TransportResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string THRUSTERS = "Thrusters";
    private const string CAPACITY = "Capacity";

    public TransportResearch() : base("Transport", 2, 10, 0, 2, 100)
    {
        researchables.Add(ARMOR, 0);
        researchables.Add(PLATING, 0);
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
        hull += 2.0f;
    }

    private void AdvancePlating()
    {
        if (researchables[ARMOR] < 5)
            return;

        researchables[PLATING]++;
        protection = researchables[PLATING] * 0.02f;
    }

    private void AdvanceThrusters()
    {
        researchables[THRUSTERS]++;
        speed += 0.5f;
    }

    private void AdvanceCapacity()
    {
        researchables[CAPACITY]++;
        capacity += 100;
    }

    public override bool Unlock()
    {
        return true;
    }

    public override void Display()
    {
    }
}

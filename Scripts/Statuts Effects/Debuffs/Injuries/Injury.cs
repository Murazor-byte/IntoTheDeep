using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Injury : DebuffEffect
{
    public int healthCapDecrease;

    protected void DepleteHealthCap()
    {
        if(target.healthCap - healthCapDecrease <= 1)
        {
            target.healthCap = 1;
        }
        else
        {
            target.healthCap -= healthCapDecrease;
        }

        if (target.healthCap < target.health) target.SetHealth(target.healthCap);
    }

    //Applies a random injury to a character
    public static void SustainInjury(Character character)
    {
        float[] injuryProb = new float[] { 0.5f, 0.5f };
        ProbabilityGenerator injurySelector = new ProbabilityGenerator(injuryProb);
        int injury = injurySelector.GenerateNumber();

        switch (injury)
        {
            case 0:
                character.statusEffects.Add(new MaimedEye(character));
                Debug.Log("Hero has sustained a maimed eye");
                break;
            case 1:
                character.statusEffects.Add(new BrokenLeg(character));
                Debug.Log("Hero has sustained a broken leg");
                break;
            default:
                character.statusEffects.Add(new BrokenLeg(character));
                break;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenLeg : Injury
{
    public BrokenLeg() { }

    public BrokenLeg(Character target)
    {
        this.target = target;
        healthCapDecrease = 5;
        ApplyEffect();
    }

    public override void ApplyEffect()
    {
        DepleteHealthCap();
        int reducedSpeed = 2;
        if(target.speed - reducedSpeed <= 1)
        {
            target.speed = 1;
        }
        else
        {
            target.speed -= reducedSpeed;
        }
    }
}

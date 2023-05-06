using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//reduce health cap by 10%, speed by 2, and damage modifier by 1
public class Starving : Injury
{
    public Starving(Character target)
    {
        this.target = target;
        healthCapDecrease = (int)(target.healthCap * .10f);
        ApplyEffect();
        Debug.Log("Player is starving");
    }

    public override void ApplyEffect()
    {
        DepleteHealthCap();
        int reducedSpeed = 2;
        int damageModifierReduction = 1;

        if (target.speed - reducedSpeed <= 1) target.speed = 1;
        else target.speed -= reducedSpeed;

        target.ChangeDamageModifier(-damageModifierReduction);
    }
}

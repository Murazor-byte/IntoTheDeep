using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//reduce health cap by 25% and speed by 1
public class Poisoned : Injury
{
    public Poisoned(Character target)
    {
        this.target = target;
        healthCapDecrease = (int)(target.healthCap * 0.25f);
        ApplyEffect();
        Debug.Log("Player got poisoned");
    }

    public override void ApplyEffect()
    {
        DepleteHealthCap();
        int reducedSpeed = 1;
        if (target.speed - reducedSpeed <= 1) target.speed = 1;
        else target.speed -= reducedSpeed;
    }
}

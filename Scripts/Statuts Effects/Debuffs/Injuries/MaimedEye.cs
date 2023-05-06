using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaimedEye : Injury
{
    public MaimedEye() { }

    public MaimedEye(Character target)
    {
        this.target = target;
        healthCapDecrease = 4;
        ApplyEffect();
    }

    public override void ApplyEffect()
    {
        target.SetDamageModifier(0);
        DepleteHealthCap();
    }
}

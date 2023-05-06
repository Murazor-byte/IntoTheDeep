using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Dodge : BuffEffect
{
    public Effect_Dodge() { }

    public Effect_Dodge(Character target, int counter, float effectPercentage)
    {
        this.target = target;
        this.counter = counter;
        this.effectPercentage = effectPercentage;
        target.ChangeChanceToHit(-effectPercentage);
    }

    public override void ApplyEffect()
    {
        DecrementCounter();

        if (counter <= 0)
        {
            target.ChangeChanceToHit(effectPercentage);
            RemoveStatusEffect(this);
        }
    }

    protected override void RenewEffect()
    {
        counter = 1;
    }
}

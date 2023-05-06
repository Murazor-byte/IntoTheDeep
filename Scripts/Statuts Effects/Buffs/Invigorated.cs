using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invigorated : BuffEffect
{
    public Invigorated() { }

    public Invigorated(Character target, int counter)
    {
        this.target = target;
        this.counter = counter;

        target.ChangeDamageModifier(2);

        int healthGained = Random.Range(2, target.health / 5);
        target.Heal(healthGained);

        VFX = EffectInstantiator.InstantiateEffect("VFX/Effect/Buff/Invigorated", target.gameObject);
    }

    public override void ApplyEffect()
    {
        DecrementCounter();

        if (counter <= 0)
        {
            target.ChangeDamageModifier(-2);
            RemoveStatusEffect(this);
        }
    }

    protected override void RenewEffect()
    {
        counter = 3;
    }
}

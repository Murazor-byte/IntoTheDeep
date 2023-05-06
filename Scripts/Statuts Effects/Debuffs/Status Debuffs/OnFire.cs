using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFire : DebuffEffect
{
    private const int FIREDMAGE = 1;

    //default constructor for setting characters current effectTile
    public OnFire() { }

    public OnFire(Character target, int counter)
    {
        this.target = target;
        this.counter = counter;
    }

    public override void ApplyEffect()
    {
        target.TakeDamage(FIREDMAGE);

        DecrementCounter();

        if(counter <= 0)
        {
            RemoveStatusEffect(this);
        }
    }

    protected override void RenewEffect()
    {
        counter = 3;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wet : DebuffEffect
{
    //default constructor for setting characters current effectTile
    public Wet() { }

    public Wet(Character target, int counter)
    {
        this.target = target;
        this.counter = counter;
        target.speed -= 1;
    }

    public override void ApplyEffect()
    {
        DecrementCounter();

        if(counter <= 0)
        {
            target.speed += 1;
            RemoveStatusEffect(this);
        }
    }

    protected override void RenewEffect()
    {
        counter = 2;
    }

}

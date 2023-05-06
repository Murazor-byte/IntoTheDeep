using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect
{
    public int counter;
    public float effectPercentage;      //a percentage to be applied to the effect
    public Character target;
    public GameObject VFX;              //for applying visual effects to these effects

    public abstract void ApplyEffect();

    //reapplying effect to character
    public void ReapplyEffect()
    {
        RenewEffect();
        //Debug.Log("Theres already the same effect on this character, renewing effect");
        return;
    }

    //renews the effects counter
    protected abstract void RenewEffect();


    //removes a status effect from the target
    protected void RemoveStatusEffect(Effect effectType)
    {
        foreach(Effect effect in target.statusEffects)
        {
            if (effect.GetType() == effectType.GetType())
            {
                target.statusEffects.Remove(effect);
                Debug.Log("Removed a status effect, there are " + target.statusEffects.Count + " statuses on this character");
                if(VFX != null)
                {
                    EffectInstantiator.RemoveEffect(VFX);
                }
                return;
            }
        }
    }

    protected void DecrementCounter()
    {
        counter--;
    }
}

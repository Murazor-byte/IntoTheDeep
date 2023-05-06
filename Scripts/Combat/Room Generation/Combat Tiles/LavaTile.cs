using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaTile : EffectTile
{

    protected override void SetNewEffect()
    {
        //Debug.Log("Apllying lava effect onto character");
        characterOnTile.statusEffects.Add(new OnFire(characterOnTile, 3));
        SetCharacterTileEffect();
    }

    protected override bool CheckForDuplicateEffect(Effect effectToCheck)
    {
        if (effectToCheck is OnFire) return true;
        return false;
    }

    protected override void SetCharacterTileEffect()
    {
        characterOnTile.currentEffectTileOn = new OnFire();
    }

}

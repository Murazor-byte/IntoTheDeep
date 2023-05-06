using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTIle : EffectTile
{
    protected override void SetNewEffect()
    {
        Debug.Log("Apllying water effect onto character");
        characterOnTile.statusEffects.Add(new Wet(characterOnTile, 3));
        SetCharacterTileEffect();
    }

    protected override bool CheckForDuplicateEffect(Effect effectToCheck)
    {
        if (effectToCheck is Wet) return true;
        return false;
    }

    protected override void SetCharacterTileEffect()
    {
        characterOnTile.currentEffectTileOn = new Wet();
    }

}

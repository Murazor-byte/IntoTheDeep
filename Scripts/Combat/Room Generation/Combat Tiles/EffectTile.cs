using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectTile : MonoBehaviour
{
    public Character characterOnTile;
    protected Effect newEffectAdded;
    protected bool bridgeOnTile;
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bridge"))
        {
            //Debug.Log("A bridge is on this tile, removing this tiles effect");
            bridgeOnTile = true;
            return;
        }

        if (!other.gameObject.CompareTag("Character") || bridgeOnTile) return;

        characterOnTile = other.gameObject.GetComponent<Character>();

        //check if character already has this effect active and reset that counter & updating characterTileEffectOn
        for(int i = 0; i < characterOnTile.statusEffects.Count; i++)
        {
            if (CheckForDuplicateEffect(characterOnTile.statusEffects[i]))
            {
                characterOnTile.statusEffects[i].ReapplyEffect();
                return;
            }
        }
        //else add this effect to the character
        SetNewEffect();
        return;

    }

    //checks if the current character already has this effect
    protected abstract bool CheckForDuplicateEffect(Effect effectToCheck);

    //adds the specific Effect to the player on first entry
    protected abstract void SetNewEffect();

    //sets the characters currentTileOneffect to the default effect of that tile
    protected abstract void SetCharacterTileEffect();

}

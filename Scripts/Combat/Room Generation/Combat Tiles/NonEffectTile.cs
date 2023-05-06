using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonEffectTile : MonoBehaviour
{
    private Character characterOnTile;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EffectTile"))
        {
            Destroy(gameObject);
            return;
        }
        if (!other.gameObject.CompareTag("Character")) return;

        characterOnTile = other.gameObject.GetComponent<Character>();
        characterOnTile.currentEffectTileOn = null;
    }
}

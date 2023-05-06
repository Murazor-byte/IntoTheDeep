using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Instantiates and Destroys passed VFX effects from "Effects"
public class EffectInstantiator : MonoBehaviour
{
    //instantiates a VFX effect on that passed character
    //returning the spawned effect for reference when it's to be destroyed
    public static GameObject InstantiateEffect(string effectPath, GameObject character)
    {
        GameObject effectHolder = new GameObject();
        GameObject effect = Instantiate(Resources.Load<GameObject>(effectPath), Vector3.zero, Quaternion.identity) as GameObject;

        effectHolder.transform.parent = character.transform;
        effectHolder.transform.position = character.transform.position;
        effect.transform.parent = effectHolder.transform;
        effect.transform.position = effectHolder.transform.position;
        effect.transform.Rotate(-90f, 0f, 0f);

        return effectHolder;
    }

    public static void RemoveEffect(GameObject effect)
    {
        Destroy(effect);
    }
}

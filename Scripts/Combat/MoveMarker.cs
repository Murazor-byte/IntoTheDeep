using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMarker : MonoBehaviour
{
    //private float frequency = 0.5f;
    //private float speed = 2f;

    // Update is called once per frame
    void Update()
    {
        //float newYPos = Mathf.Sin(Time.deltaTime * speed);
        //transform.position = new Vector3(mainXPos, newYPos + 0.5f, mainZPos) * frequency;
        transform.Rotate(0,0,0.25f);
    }
}

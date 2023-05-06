using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private GameObject combatPlayer;
    private Transform target;

    private Vector3 newPosition;

    private Vector3 initialOffset = new Vector3(10, 12, 10);
    public const float DISTANCE = 15.2f;                    //how far the camera should be away from player at all times
    private float rotationSpeed = 30f;                      //how fast the camera roates around the target
    public float lookAtSpeed = 0.55f;                       //how fast the camera rotates to look at target
    public float smoothTime = 0.55f;                        //how fast the position of the camera lerps to its target
    private Vector3 velocity = Vector3.zero;

    private Vector3 direction;
    private Quaternion toRotation;

    private void Start()
    {
        combatPlayer = GameObject.Find("CombatPlayer");
        target = combatPlayer.transform;

        transform.position = target.position + initialOffset;
        newPosition = transform.position;

        transform.LookAt(target.transform.position);
    }


    private void FixedUpdate()
    {
        if(combatPlayer == null)
        {
            Debug.Log("Combat Player can't be found by the camera returning...");
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.RotateAround(combatPlayer.transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            transform.RotateAround(combatPlayer.transform.position, Vector3.down, rotationSpeed * Time.deltaTime);
        }

        //lerp the camera to move to the target
        newPosition = (transform.position - target.position).normalized * DISTANCE + target.transform.position;
        newPosition = new Vector3(newPosition.x, 12f, newPosition.z);

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);

        //lerp the camera to look at the target
        direction = target.transform.position - transform.position;
        toRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, lookAtSpeed * Time.deltaTime);

    }
}

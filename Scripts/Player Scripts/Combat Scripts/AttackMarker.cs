using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//grabs the character component script and passes to PlayerCombatMovement
public class AttackMarker : MonoBehaviour
{
    //I NEED TO CHANGE THIS FOR WHEN AN ATTACK MARKER IS SPAWNED AND COMPARE IT TO ENEMEY POSITIONS instead of trigger enter
        // I will also need to track if the same enemy has already been targeted (to not select the same enemy twice that takes up multiple tiles)
    //USE THE 'FINDENEMIESINPATH() method in Skill class to identify this!!
    private void OnTriggerEnter(Collider other)
    {
        Enemy enemySelected = other.gameObject.GetComponent<Enemy>();

        GameObject player = GameObject.Find("CombatPlayer");

        if (other.gameObject.Equals(player)) return;

        if (other.gameObject.CompareTag("Character"))
        {
            PlayerCombatMovement playerCombatMovement = player.GetComponent<PlayerCombatMovement>();
            playerCombatMovement.player.currentSkill.singleEnemySelected = enemySelected;
            playerCombatMovement.player.currentSkill.singelEnemySelected[playerCombatMovement.player.currentSkill.singelEnemySelected.Count - 1] = true;
        }
    }
}

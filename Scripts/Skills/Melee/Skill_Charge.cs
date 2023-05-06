using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Charge : Skill
{
    public Skill_Charge() { }

    public override void SetSkillDistance()
    {
        skillType = SkillType.Line;
        limitedUses = true;
        numberUses = 3;
        skillDistance = 4;
        skillDamage = 6;
        Vector3 position = character.transform.position;
        currentLineMove = new Vector3(position.x, CombatMovement.moveMarkerHieght, position.z);
        SkillDistance();
    }

    public override Texture GetSkillAsset()
    {
        return SkillAssets.Instance.charge;
    }

    //move the character up the first enemy encountered, and deal damage to them
    public override void UseSkill()
    {
        ResetSkillDistanceMarkers();
        playerMovement.StartMovePlayer();
    }

    //damage the first enemy stopped at
    public override void ContinueSkill()
    {
        foreach(Vector3 pos in selectedAttackTiles)
        {
            if(selectedCharacters[pos] != null)
            {
                Debug.Log("Charging into character at location: " + pos.x + ", " + pos.z);
                Attack(selectedCharacters[pos]);
                break;
            }
        }
    }

    public override void Attack(Character enemyToAttack)
    {
        DealDirectDamage(enemyToAttack, skillDamage, true);
    }
}

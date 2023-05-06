using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Cleave : Skill {

    private const int meleeCleaveRange = 2;

    public Skill_Cleave() { }

    public override void SetSkillDistance()
    {
        skillType = SkillType.Sweep;
        limitedUses = true;
        skillDistance = meleeCleaveRange;
        currentConeMove = new Vector3(character.transform.position.x, CombatMovement.moveMarkerHieght, character.transform.position.z);
        localPos = currentConeMove;
        SkillDistance();
    }

    public override Texture GetSkillAsset()
    {
        return base.GetSkillAsset();
    }

    public override void UseSkill()
    {
        throw new System.NotImplementedException();
    }

    public override void Attack(Character enemyToAttack)
    {
        base.Attack(enemyToAttack);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Strike : Skill
{
    public Skill_Strike() { }

    public override void SetSkillDistance()
    {
        skillType = SkillType.Direct;
        limitedUses = false;
        skillDistance = character.weaponRange;
        SkillDistance();
    }

    public override Texture GetSkillAsset()
    {
        return SkillAssets.Instance.strike;
    }

    public override void UseSkill()
    {
        if (singelEnemySelected.Count > 0 && singelEnemySelected[singelEnemySelected.Count - 1] == true)
        {
            Attack(singleEnemySelected);
        }
    }

    public override void Attack(Character enemyToAttack)
    {
        DealDirectDamage(enemyToAttack, character.damage, true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_CombatMove : Skill
{
    public Skill_CombatMove() { }

    public override void SetSkillDistance()
    {
        skillType = SkillType.Movement;
        skillDistance = character.speed - movesMade;
        SkillDistance();
    }

    public override Texture GetSkillAsset()
    {
        return SkillAssets.Instance.movement;
    }

    public override void UseSkill()
    {

    }
}

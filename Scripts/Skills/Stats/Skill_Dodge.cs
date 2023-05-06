using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Dodge : Skill
{
    private const float DODGE_PERCENTAGE = 0.5f;

    public Skill_Dodge() { }

    public override void SetSkillDistance()
    {
        skillType = SkillType.Self;
        limitedUses = false;
        skillDistance = 1;
        SkillDistance();
    }

    public override Texture GetSkillAsset()
    {
        return SkillAssets.Instance.dodge;
    }

    public override void UseSkill()
    {
        character.statusEffects.Add(new Effect_Dodge(character, 1, DODGE_PERCENTAGE));
    }
}

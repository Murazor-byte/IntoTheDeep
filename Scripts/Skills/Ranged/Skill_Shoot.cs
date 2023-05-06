using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Shoot : Skill
{
    public Skill_Shoot() { }

    public override void SetSkillDistance()
    {
        skillType = SkillType.Line;
        limitedUses = false;
        skillDistance = character.weaponRange;
        currentLineMove = new Vector3(character.transform.position.x, CombatMovement.moveMarkerHieght, character.transform.position.z);
        SkillDistance();
    }

    public override Texture GetSkillAsset()
    {
        return SkillAssets.Instance.shoot;
    }

    public override void UseSkill()
    {
        for(int i = 0; i < selectedAttackTiles.Count; i++)
        {
            if(playerMovement.tiles.traversability[(int)selectedAttackTiles[i].x][(int)selectedAttackTiles[i].z] != TileGenerator.Traversability.Walkable)
            {
                if (playerMovement.tiles.traversability[(int)selectedAttackTiles[i].x][(int)selectedAttackTiles[i].z] == TileGenerator.Traversability.EnemyOccupied)
                {
                    Attack(selectedCharacters[selectedAttackTiles[i]]);
                }
                else
                {
                    Debug.Log("Obstacle is in the line of fire");
                    break;
                }
            }
        }
    }

    public override void Attack(Character enemyToAttack)
    {
        DealDirectDamage(enemyToAttack, character.damage, true);
    }
}

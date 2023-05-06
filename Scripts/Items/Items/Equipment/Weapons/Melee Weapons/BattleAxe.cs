using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAxe : Weapon, IMeleeWeapon
{
    public BattleAxe()
    {
        weaponDamage = 25;
        weaponRange = 2;
        weight = 50;
        value = 125;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.battleaxe;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }
}

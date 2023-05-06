using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warhammer : Weapon, IMeleeWeapon
{
    public Warhammer()
    {
        weaponDamage = 25;
        weaponRange = 2;
        weight = 40;
        value = 100;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.warhammer;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorningStar : Weapon, IMeleeWeapon
{
    public MorningStar()
    {
        weaponDamage = 8;
        weaponRange = 1;
        weight = 20;
        value = 23;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.morningstar;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }
}

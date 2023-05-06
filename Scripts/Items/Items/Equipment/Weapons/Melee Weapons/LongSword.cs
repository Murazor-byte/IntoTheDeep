using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongSword : Weapon, IMeleeWeapon
{
    public LongSword()
    {
        weaponDamage = 17;
        weaponRange = 2;
        weight = 45;
        value = 175;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.longsword;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }
}

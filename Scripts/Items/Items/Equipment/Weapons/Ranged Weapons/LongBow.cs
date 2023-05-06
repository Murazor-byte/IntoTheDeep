using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongBow : Weapon, IRangeWeapon
{
    public LongBow()
    {
        weaponDamage = 15;
        weaponRange = 10;
        weight = 20;
        value = 30;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.longbow;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }
}

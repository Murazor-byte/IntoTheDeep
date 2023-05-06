using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortBow : Weapon, IRangeWeapon
{
    public ShortBow()
    {
        weaponDamage = 10;
        weaponRange = 5;
        weight = 10;
        value = 10;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.shortbow;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }

}

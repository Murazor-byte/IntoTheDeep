using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crossbow : Weapon, IRangeWeapon
{
    public Crossbow()
    {
        weaponDamage = 15;
        weaponRange = 8;
        weight = 30;
        value = 70;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.crossbow;
    }

    public override void UseItem()
    {
        Debug.Log("Using Crossbow");
        character.EquipNewWeapon(this);
        Debug.Log("Equipped new crossbow");
        character.inventory.RemoveItem(this);
    }
}

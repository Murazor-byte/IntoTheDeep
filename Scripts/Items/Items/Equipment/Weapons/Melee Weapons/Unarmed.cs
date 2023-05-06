using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unarmed : Weapon, IMeleeWeapon
{
    public Unarmed()
    {
        weaponDamage = 1;
        weaponRange = 1;
        weight = 0;
        value = 0;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.unarmed;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }

}

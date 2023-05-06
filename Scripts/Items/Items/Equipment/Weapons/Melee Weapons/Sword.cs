using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : Weapon, IMeleeWeapon
{
    public Sword()
    {
        weaponDamage = 7;
        weaponRange = 1;
        weight = 10;
        value = 22;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.sword;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mace : Weapon, IMeleeWeapon
{
    public Mace()
    {
        weaponDamage = 6;
        weaponRange = 1;
        weight = 20;
        value = 20;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.mace;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flail : Weapon, IMeleeWeapon
{
    public Flail()
    {
        weaponDamage = 5;
        weaponRange = 1;
        weight = 15;
        value = 18;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.flail;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }
}

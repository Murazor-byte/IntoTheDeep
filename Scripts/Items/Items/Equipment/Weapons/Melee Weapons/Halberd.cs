using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Halberd : Weapon, IMeleeWeapon
{
    public Halberd()
    {
        weaponDamage = 13;
        weaponRange = 2;
        weight = 35;
        value = 75;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.halberd;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }
}

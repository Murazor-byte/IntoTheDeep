using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maul : Weapon, IMeleeWeapon
{
    public Maul()
    {
        weaponDamage = 20;
        weaponRange = 2;
        weight = 20;
        value = 100;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.maul;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }
}

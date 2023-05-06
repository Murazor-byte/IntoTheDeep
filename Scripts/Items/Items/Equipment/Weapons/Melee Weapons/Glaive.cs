using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glaive : Weapon, IMeleeWeapon
{
    public Glaive()
    {
        weaponDamage = 10;
        weaponRange = 1;
        weight = 40;
        value = 50;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.glaive;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }
}

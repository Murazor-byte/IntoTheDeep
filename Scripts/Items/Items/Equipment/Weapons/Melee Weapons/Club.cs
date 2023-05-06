using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Club : Weapon, IMeleeWeapon
{
    public Club()
    {
        weaponDamage = 4;
        weaponRange = 1;
        weight = 5;
        value = 10;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.club;
    }

    public override void UseItem()
    {
        character.EquipNewWeapon(this);
        character.inventory.RemoveItem(this);
    }
}

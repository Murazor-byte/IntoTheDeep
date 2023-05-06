using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothShirt : Armor, IBodyArmor
{
    private static int armorStat = 0;
    private static int speedModifier = 10;

    public ClothShirt() : base(armorStat, speedModifier)
    {
        weight = 5;
        value = 50;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.cloth_Shirt;
    }

    public override void UseItem()
    {
        character.EquipNewArmor(this);
        character.inventory.RemoveItem(this);
    }
}

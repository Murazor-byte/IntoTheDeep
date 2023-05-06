using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothBoots : Armor, IBoots
{
    private static int armorStat = 0;
    private static int speedModifier = 5;

    public ClothBoots() : base(armorStat, speedModifier)
    {
        weight = 1;
        value = 5;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.cloth_Boots;
    }

    public override void UseItem()
    {
        character.EquipNewArmor(this);
        character.inventory.RemoveItem(this);
    }
}

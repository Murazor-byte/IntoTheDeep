using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothGreaves : Armor, IGreaves
{
    private static int armorStat = 0;
    private static int speedModifier = 5;

    public ClothGreaves() : base(armorStat, speedModifier)
    {
        weight = 5;
        value = 5;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.cloth_Greaves;
    }

    public override void UseItem()
    {
        character.EquipNewArmor(this);
        character.inventory.RemoveItem(this);
    }
}

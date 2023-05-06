using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothHelm : Armor, IHelm
{
    private static int armorStat = 0;
    private static int speedModifier = 5;

    public ClothHelm() : base(armorStat, speedModifier)
    {
        weight = 1;
        value = 5;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.cloth_Helm;
    }

    public override void UseItem()
    {
        character.EquipNewArmor(this);
        character.inventory.RemoveItem(this);
    }
}

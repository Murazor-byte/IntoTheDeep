using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeatherBreastPlate : Armor, IBodyArmor
{
    private static int armorStat = 7;
    private static int speedModifier = 7;

    public LeatherBreastPlate() : base(armorStat, speedModifier)
    {
        weight = 25;
        value = 25;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.leather_Breastplate;
    }

    public override void UseItem()
    {
        character.EquipNewArmor(this);
        character.inventory.RemoveItem(this);
    }
}

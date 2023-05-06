using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChainMail : Armor, IBodyArmor
{
    private static int armorStat = 10;
    private static int speedModifier = 5;

    public ChainMail() : base(armorStat, speedModifier)
    {
        weight = 50;
        value = 25;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.chainmail;
    }

    public override void UseItem()
    {
        character.EquipNewArmor(this);
        character.inventory.RemoveItem(this);
    }
}

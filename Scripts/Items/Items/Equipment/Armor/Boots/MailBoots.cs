using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailBoots : Armor, IBoots
{
    private static int armorStat = 10;
    private static int speedModifier = 3;

    public MailBoots() : base(armorStat, speedModifier)
    {
        weight = 30;
        value = 25;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.chainmail_Boots;
    }

    public override void UseItem()
    {
        character.EquipNewArmor(this);
        character.inventory.RemoveItem(this);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailGreaves : Armor, IGreaves
{
    private static int armorStat = 10;
    private static int speedModifier = 3;

    public MailGreaves() : base(armorStat, speedModifier)
    {
        weight = 40;
        value = 25;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.chainmail_Greaves;
    }

    public override void UseItem()
    {
        character.EquipNewArmor(this);
        character.inventory.RemoveItem(this);
    }

}

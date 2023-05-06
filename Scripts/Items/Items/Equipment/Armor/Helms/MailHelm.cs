using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailHelm : Armor, IHelm
{
    private static int armorStat = 10;
    private static int speedModifier = 3;

    public MailHelm() : base(armorStat, speedModifier)
    {
        weight = 10;
        value = 25;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.chainmail_Helm;
    }

    public override void UseItem()
    {
        character.EquipNewArmor(this);
        character.inventory.RemoveItem(this);
    }
}

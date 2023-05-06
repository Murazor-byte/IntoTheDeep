using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeatherHelm : Armor, IHelm
{
    private static int armorStat = 7;
    private static int speedModifier = 3;

    public LeatherHelm() : base(armorStat, speedModifier)
    {
        weight = 5;
        value = 25;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.leather_Helm;
    }

    public override void UseItem()
    {
        character.EquipNewArmor(this);
        character.inventory.RemoveItem(this);
    }
}


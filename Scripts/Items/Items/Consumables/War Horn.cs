using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarHorn : Item
{
    public WarHorn()
    {
        weight = 8;
        value = 125;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.war_Horn;
    }

    public override int ItemAmount()
    {
        itemAmount = 3;
        numProb = 1;
        denProb = 14;

        return GenerateItemAmount();
    }

    public override void UseItem()
    {
        character.inventory.RemoveItem(this);
    }
}

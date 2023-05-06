using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Arrow : Item
{
    public Arrow()
    {
        weight = 1;
        value = 2;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.arrows;
    }

    public override int ItemAmount()
    {
        itemAmount = 30;
        numProb = 25;
        denProb = 27;

        return GenerateItemAmount();
    }

    public override void UseItem()
    {
        character.inventory.RemoveItem(this);
    }
}

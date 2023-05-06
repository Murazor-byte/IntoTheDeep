using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candle : Item
{
    public Candle()
    {
        weight = 1;
        value = 5;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.candle;
    }

    public override int ItemAmount()
    {
        itemAmount = 10;
        numProb = 11;
        denProb = 15;

        return GenerateItemAmount();
    }

    public override void UseItem()
    {
        character.inventory.RemoveItem(this);
    }
}

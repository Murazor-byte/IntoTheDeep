using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : Item
{
    public Rope()
    {
        weight = 7;
        value = 10;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.rope;
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

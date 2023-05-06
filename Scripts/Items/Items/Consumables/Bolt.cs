using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolt : Item
{
    public Bolt()
    {
        weight = 2;
        value = 3;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.bolts;
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

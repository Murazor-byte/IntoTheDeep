using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : Item
{
    public Lamp()
    {
        weight = 13;
        value = 40;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.lamp;
    }

    public override int ItemAmount()
    {
        itemAmount = 3;
        numProb = 1;
        denProb = 13;

        return GenerateItemAmount();
    }

    public override void UseItem()
    {
        character.inventory.RemoveItem(this);
    }
}

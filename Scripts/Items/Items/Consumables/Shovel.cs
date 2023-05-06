using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shovel : Item
{
    public Shovel()
    {
        weight = 5;
        value = 15;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.shovel;
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

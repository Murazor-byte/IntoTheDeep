using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : Item
{
    public Torch()
    {
        weight = 2;
        value = 3;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.torch;
    }

    public override int ItemAmount()
    {
        itemAmount = 8;
        numProb = 11;
        denProb = 17;

        return GenerateItemAmount();
    }

    public override void UseItem()
    {
        character.inventory.RemoveItem(this);
    }
}

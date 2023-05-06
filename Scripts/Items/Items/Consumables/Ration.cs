using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ration : Item
{
    public Ration()
    {
        weight = 3;
        value = 5;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.rations;
    }

    public override int ItemAmount()
    {
        itemAmount = 5;
        numProb = 8;
        denProb = 15;

        return GenerateItemAmount();
    }

    public override void UseItem()
    {
        character.inventory.RemoveItem(this);
    }
}

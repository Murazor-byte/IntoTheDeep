using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lockpick : Item
{
    public Lockpick()
    {
        weight = 1;
        value = 15;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.lockpick;
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

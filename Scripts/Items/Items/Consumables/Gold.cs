using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gold : Item
{
    public Gold()
    {
        weight = 1;
        value = 1;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.gold;
    }
    public override int ItemAmount()
    {
        itemAmount = 100;
        numProb = 32;
        denProb = 33;

        return GenerateItemAmount();
    }

    public override void UseItem()
    {
        character.gold -= 1;
        character.inventory.RemoveItem(this);
    }
}

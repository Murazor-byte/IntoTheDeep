using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion_Speed : Item
{
    public Potion_Speed()
    {
        weight = 8;
        value = 200;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.potion_Speed;
    }

    public override int ItemAmount()
    {
        itemAmount = 3;
        numProb = 1;
        denProb = 15;

        return GenerateItemAmount();
    }

    public override void UseItem()
    {
        character.inventory.RemoveItem(this);
    }
}

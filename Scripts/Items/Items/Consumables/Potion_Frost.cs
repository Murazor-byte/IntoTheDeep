using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion_Frost : Item
{
    public Potion_Frost()
    {
        weight = 8;
        value = 200;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.potion_Frost;
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

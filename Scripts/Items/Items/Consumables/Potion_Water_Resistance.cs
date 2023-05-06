using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion_Water_Resistance : Item
{
    public Potion_Water_Resistance()
    {
        weight = 5;
        value = 70;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.potion_Water_Resistance;
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

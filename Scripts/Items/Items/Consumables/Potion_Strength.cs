using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion_Strength : Item
{
    public Potion_Strength()
    {
        weight = 50;
        value = 200;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.potion_Strength;
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

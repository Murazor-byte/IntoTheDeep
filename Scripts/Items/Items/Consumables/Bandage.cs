using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bandage : Item
{
    public Bandage()
    {
        weight = 5;
        value = 10;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.bandage;
    }

    public override int ItemAmount()
    {
        itemAmount = 5;
        numProb = 10;
        denProb = 14;

        return GenerateItemAmount();
    }

    public override void UseItem()
    {
        character.Heal(5);
        character.inventory.RemoveItem(this);
    }
}

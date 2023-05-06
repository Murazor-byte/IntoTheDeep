using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vial_Oil : Item
{
    public Vial_Oil()
    {
        weight = 5;
        value = 60;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.vial_Oil;
    }

    public override int ItemAmount()
    {
        itemAmount = 4;
        numProb = 1;
        denProb = 12;

        return GenerateItemAmount();
    }

    public override void UseItem()
    {
        character.inventory.RemoveItem(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camp : Item
{
    public Camp()
    {
        weight = 50;
        value = 100;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.camp;
    }

    public override int ItemAmount()
    {
        itemAmount = 3;
        numProb = 1;
        denProb = 14;

        return GenerateItemAmount();
    }

    public override void UseItem()
    {
        character.Heal(character.healthCap);
        character.inventory.RemoveItem(this);
    }
}

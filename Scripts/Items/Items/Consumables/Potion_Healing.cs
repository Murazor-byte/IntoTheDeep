using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion_Healing : Item
{
    public Potion_Healing()
    {
        weight = 50;
        value = 50;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.potion_Healing;
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
        character.Heal(10);
        character.inventory.RemoveItem(this);
    }
}

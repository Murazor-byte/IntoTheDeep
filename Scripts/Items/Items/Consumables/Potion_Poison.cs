using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion_Poison : Item
{
    private const int POISONDAMAGE = 10;

    public Potion_Poison()
    {
        weight = 50;
        value = 50;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.potion_Poison;
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
        character.TakeDamage(POISONDAMAGE);
        character.inventory.RemoveItem(this);
    }
}

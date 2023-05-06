using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion_Superior_Healing : Item
{
    public Potion_Superior_Healing()
    {
        weight = 50;
        value = 100;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.potion_Superior_Healing;
    }

    public override int ItemAmount()
    {
        itemAmount = 3;
        numProb = 1;
        denProb = 17;

        return GenerateItemAmount();
    }

    public override void UseItem()
    {
        character.Heal(30);
        character.inventory.RemoveItem(this);
    }
}

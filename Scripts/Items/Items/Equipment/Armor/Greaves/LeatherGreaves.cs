using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeatherGreaves : Armor, IGreaves
{
    private static int armorStat = 7;
    private static int speedModifier = 3;

    public LeatherGreaves() : base(armorStat, speedModifier)
    {
        weight = 10;
        value = 25;
    }

    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.leather_Greaves;
    }

    public override void UseItem()
    {
        character.EquipNewArmor(this);
        character.inventory.RemoveItem(this);
    }
}

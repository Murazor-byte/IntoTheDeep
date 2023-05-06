using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class to represent an empty inventory slot
public class EmptySlot : Item
{
    public override Texture GetItemImage()
    {
        return ItemAssets.Instance.emptySlot;
    }

    public override void UseItem()
    {
        return;
    }

    public override bool GetIsEmpty()
    {
        return true;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonShopInventory : ShopInventory
{
    protected override void FillUpShop()
    {
        AddItemToShop(new Arrow());
        AddItemToShop(new Bandage());
        AddItemToShop(new Camp());
        AddItemToShop(new Ration());
        AddItemToShop(new Rope());
        AddItemToShop(new Shovel());
        AddItemToShop(new Torch());
    }
}

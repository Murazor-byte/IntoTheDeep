using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSmithInventory : ShopInventory
{
    protected override void FillUpShop()
    {
        AddItemToShop(new BattleAxe());
        AddItemToShop(new Club());
        AddItemToShop(new Crossbow());
        AddItemToShop(new Flail());
        AddItemToShop(new Glaive());
        AddItemToShop(new Halberd());
        AddItemToShop(new LongBow());
        AddItemToShop(new LongSword());
        AddItemToShop(new Mace());
        AddItemToShop(new Maul());
        AddItemToShop(new MorningStar());
        AddItemToShop(new ShortBow());
        AddItemToShop(new Sword());
        AddItemToShop(new Unarmed());
        AddItemToShop(new Warhammer());
    }
}

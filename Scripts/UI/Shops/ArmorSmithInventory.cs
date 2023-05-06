using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmorSmithInventory : ShopInventory
{
    protected override void FillUpShop()
    {
        AddItemToShop(new ClothHelm());
        AddItemToShop(new ClothShirt());
        AddItemToShop(new ClothGreaves());
        AddItemToShop(new ClothBoots());
        AddItemToShop(new LeatherHelm());
        AddItemToShop(new LeatherBreastPlate());
        AddItemToShop(new LeatherGreaves());
        AddItemToShop(new LeatherBoots());
        AddItemToShop(new MailHelm());
        AddItemToShop(new ChainMail());
        AddItemToShop(new MailGreaves());
        AddItemToShop(new MailBoots());
    }
}

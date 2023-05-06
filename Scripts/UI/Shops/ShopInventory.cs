using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopInventory : MonoBehaviour
{
    public List<Item> shopItemInventory;    //holds actual items stored in this shop inventory
    protected GameObject shopList;          //Shop Item List Container to hold each ShopItem in a list
    protected GameObject shopItem;          //current ShopItem to add to shopInventory
    protected BuyShopItem buyShopItem;      //current ShopItem Button to add to shopInventory
    protected Player player;

    public void SetUpShop(Player player)
    {
        this.player = player;
        shopItemInventory = new List<Item>();

        shopList = transform.Find("Shop Inventory Container").gameObject;
        FillUpShop();
    }

    protected virtual void FillUpShop() { }

    public void AddItemToShop(Item item)
    {
        shopItemInventory.Add(item);
        shopItem = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Town/ShopItem"));
        shopItem.transform.SetParent(shopList.transform);
        shopItem.transform.position = shopList.transform.position;
        shopItem.transform.localScale = new Vector3(0.2831275f, 0.3059375f, 1f);

        shopItem.transform.Find("ShopItem Image Border/ShopItem Image").GetComponent<RawImage>().texture = item.GetItemImage();       //update ShopItem Image
        shopItem.transform.Find("ShopItem Text/ShopItem Description Background/ShopItem Name").GetComponent<Text>().text = "Name: " + item.GetType();  //update ShopItem Name
        shopItem.transform.Find("ShopItem Text/ShopItem Description Background/ShopItem Cost").GetComponent<Text>().text = "Cost: " + item.GetValue();        //update ShopItem Cost

        buyShopItem = shopItem.transform.Find("Buy ShopItem Button").GetComponent<BuyShopItem>();
        buyShopItem.SetUpButtonListener(item, player);
    }

    //clears all items out of the shops inventory
    public void ClearInventory()
    {
        shopItemInventory.Clear();
        foreach(Transform child in shopList.transform)
        {
            Destroy(child.gameObject);
        }
    }
}

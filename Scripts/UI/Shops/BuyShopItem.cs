using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyShopItem : MonoBehaviour
{
    private Item item;
    private Player player;

    public void SetUpButtonListener(Item item, Player player)
    {
        this.item = item;
        this.player = player;
        gameObject.GetComponent<Button>().onClick.AddListener(SetUpListener);
    }

    //Add the Shop item to the players inventory if they have enough gold
    private void SetUpListener()
    {
        //if player doesn't have enough gold to buy the item
        if(player.gold - item.GetValue() < 0)
        {
            Debug.Log("Player doesn't have enough gold");
        }
        else
        {
            //find the gold item in the players inventory and remove that amount
            for (int i = 0; i < player.inventory.inventory.Count; i++)
            {
                if (player.inventory.inventory[i] is Gold)
                {
                    for(int j = 0; j < item.GetValue(); j++)
                    {
                        player.inventory.inventory[i].UseItem();
                    }
                }
            }
            player.inventory.AddItem(item, player);
        }
    }
}

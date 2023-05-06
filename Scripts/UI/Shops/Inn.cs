using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inn : ShopInventory 
{
    private Button restButton;
    private int innCost = 50;
    private int innHeal = 45;


    protected override void FillUpShop()
    {
        restButton = shopList.transform.Find("Rest").gameObject.GetComponent<Button>();
        restButton.onClick.AddListener(SetUpInnRest);
    }

    private void SetUpInnRest()
    {

        //if player doesn't have enough gold to buy the item
        if (player.gold - innCost < 0)
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
                    for (int j = 0; j < innCost; j++)
                    {
                        player.inventory.inventory[i].UseItem();
                    }
                }
            }
            player.Heal(innHeal);
        }
    }
}

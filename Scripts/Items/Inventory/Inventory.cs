using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//inventory class that holds each players inventory,
//inluding a displayed UI inventory on the bottom-left screen
public class Inventory : MonoBehaviour
{

    public List<Item> inventory;
    public List<GameObject> inventorySlots;
    private int carryLoad;
    private int carryCapacity;

    private GameObject inventoryUISlot;
    private GameObject inventorySlot;
    
    //initalize the new inventory
    public Inventory(int carryLoad, int carryCapacity)
    {
        this.carryLoad = carryLoad;
        this.carryCapacity = carryCapacity;

        inventory = new List<Item>();
        inventorySlots = new List<GameObject>();

        for (int i = 0; i < carryLoad; i++)
        {
            inventory.Add(new EmptySlot());
        }
    }

    //Fills the players inventory UI with empty inventory slots
    //This should only be called once for whatever player is showing their inventory on the UI
    public void CreateInventorySlots(int carryLoad)
    {
        inventoryUISlot = GameObject.Find("Inventory UI");
        inventorySlot = GameObject.Find("InventoryItemSlot");

        float cellSize = 40f;
        int x = 0;
        int y = 0;

        for (int i = 0; i < carryLoad; i++)
        {
            GameObject itemInventorySlot = Instantiate(Resources.Load<GameObject>("Prefabs/UI/InventorySlots/InventoryItemSlot"));
            itemInventorySlot.transform.SetParent(inventoryUISlot.transform);
            itemInventorySlot.transform.localScale = inventorySlot.transform.localScale;

            itemInventorySlot.gameObject.SetActive(true);

            RectTransform itemInventorySlotTransform = itemInventorySlot.GetComponent<RectTransform>();
            itemInventorySlotTransform.anchoredPosition = new Vector3(-323 + x * cellSize, -83 + y * cellSize, 0);
            itemInventorySlotTransform.localPosition = new Vector3(-373 + x * cellSize, -133 + y * cellSize, 0);    //when in town

            inventorySlots.Add(itemInventorySlot.gameObject);

            x++;
            if (x == 8)                     //set there to be 8 slots per row, CAN BE CHANGED LATER IF NEEDED
            {
                x = 0;
                y--;
            }
        }

        //inventorySlot.SetActive(false);     //hide the original emptyItemSlot prefab
    }

    //transfers the inventory from the Town to Dungeon Scene and diplaying to Dungeon UI
    //should be called for each player to transfer each player's inventory
    public void TransferInventory()
    {
        for(int i = 0; i < inventorySlots.Count; i++)
        {
            Transform image = inventorySlots[i].transform.Find("Image");
            image.GetComponentInChildren<RawImage>().texture = inventory[i].GetItemImage();     //update slot image
            image.GetComponentInChildren<Button>().onClick.RemoveAllListeners();                //remove old eventLisenters
            image.GetComponentInChildren<Button>().onClick.AddListener(inventory[i].UseItem);   //add new eventListener

            if (!inventory[i].GetIsEmpty()) inventory[i].inventorySlot = inventorySlots[i];     //Assign each items InventorySlot GameObjet

            if (inventory[i].quantity > 999)        //items over 9999 will be represneted with "k" 
            {
                int thousands = inventory[i].quantity / 1000;
                int hundreds = (inventory[i].quantity - (thousands * 1000)) / 100;
                inventorySlots[i].transform.Find("Counter").GetComponent<TMP_Text>().text = thousands + "." + hundreds + "k";
            }
            else if (inventory[i].quantity > 99)
            {
                inventorySlots[i].transform.Find("Counter").GetComponent<TMP_Text>().text = "  " + inventory[i].quantity;
            }
            else if (inventory[i].quantity > 9)
            {
                inventorySlots[i].transform.Find("Counter").GetComponent<TMP_Text>().text = "   " + inventory[i].quantity;
            }
            else if(inventory[i].quantity == 1 || inventory[i].quantity == 0)
            {
                inventorySlots[i].transform.Find("Counter").GetComponent<TMP_Text>().text = "";
            }
            else
            {
                inventorySlots[i].transform.Find("Counter").GetComponent<TMP_Text>().text = "     " + inventory[i].quantity;
            }
        }
    }

    //Adding starting Items to the players inventory when they're first created
    /*public void AddStartingItem(Item item, Character character)
    {
        if (character.inventoryCarryWeight + item.GetWeight() > character.carryCapacity)
        {
            Debug.Log("You are carrying too much!");
            return;
        }

        character.inventoryCarryWeight += item.GetWeight();

        //If the character is adding a currency item update their total gold amount
        if (item is Gold)
        {
            character.gold += item.GetValue();
        }

        //Item's already in inventory
        for (int i = 0; i < inventory.Count; i++)
        {
            if(inventory[i].GetType() == item.GetType())
            {
                inventory[i].quantity++;
                return;
            }
        }

        //if item is NOt in inventory
        for(int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].GetIsEmpty())
            {
                item.character = character;
                item.inventorySlotIndex = i;
                inventory[i] = item;
                inventory[i].quantity++;
                return;
            }
        }
    }*/

    //Adds an item to the first available slot to the inventory, updating UI
    //and updating the button listener on that inventory slot
    public void AddItem(Item item, Character character)
    {
        if (character.inventoryCarryWeight + item.GetWeight() > character.carryCapacity)
        {
            Debug.Log("You are carrying too much!");
            return;
        }

        character.inventoryCarryWeight += item.GetWeight();
        
        //If the character is adding a currency item update their total gold amount
        if (item is Gold){
            character.gold += item.GetValue();
        }

        //if the item is already in the character inventory, increment the quantity of that item and display quanitity
        for (int i = 0; i < inventory.Count; i++)
        {
            if(inventory[i].GetType() == item.GetType())
            {
                inventory[i].quantity++;

                if(inventory[i].quantity > 999)        //items over 9999 will be represneted with "k" 
                {
                    int thousands = inventory[i].quantity / 1000;
                    int hundreds = (inventory[i].quantity - (thousands * 1000)) / 100;
                    inventorySlots[i].transform.Find("Counter").GetComponent<TMP_Text>().text = thousands + "." + hundreds + "k";
                }
                else if(inventory[i].quantity > 99)
                {
                    inventorySlots[i].transform.Find("Counter").GetComponent<TMP_Text>().text = "  " + inventory[i].quantity;
                }
                else if(inventory[i].quantity > 9)
                {
                    inventorySlots[i].transform.Find("Counter").GetComponent<TMP_Text>().text = "   " + inventory[i].quantity;
                }
                else
                {
                    inventorySlots[i].transform.Find("Counter").GetComponent<TMP_Text>().text = "     " + inventory[i].quantity;
                }
                return;
            }
        }

        //if the item is not already in the character inventory, add it to the first empty slot
        for(int i = 0; i < inventory.Count; i++)
        {
            if(inventory[i].GetIsEmpty())
            {
                item.character = character;
                item.inventorySlotIndex = i;
                item.inventorySlot = inventorySlots[i];
                item.quantity++;

                inventory[i] = item;
                Transform image = inventorySlots[i].transform.Find("Image");
                image.GetComponentInChildren<RawImage>().texture = item.GetItemImage();     //update slot image
                image.GetComponentInChildren<Button>().onClick.RemoveAllListeners();        //remove old eventLisenters
                image.GetComponentInChildren<Button>().onClick.AddListener(item.UseItem);   //add new eventListener

                inventorySlots[i].transform.Find("Counter").GetComponent<TMP_Text>().text = "";
                return;
            }
        }

        //else all Item slots are full
        Debug.Log("Can't Carry any more Items!");
    }

    //removes an item from the characters inventory
    public void RemoveItem(Item item)
    {
        item.character.inventoryCarryWeight -= item.GetWeight();

        item.quantity--;
        //if there are multiple of this item in the character inventory, reduce its quanity by one
        if (item.quantity > 0)
        { 
            if (item.quantity > 999)
            {
                int thousands = item.quantity / 1000;
                int hundreds = (item.quantity - (thousands*1000)) / 100;
                item.inventorySlot.transform.Find("Counter").GetComponent<TMP_Text>().text = thousands + "." + hundreds + "k";
            }
            else if (item.quantity > 99)
            {
                item.inventorySlot.transform.Find("Counter").GetComponent<TMP_Text>().text = "  " + item.quantity;
            }
            else if (item.quantity > 9)
            {
                item.inventorySlot.transform.Find("Counter").GetComponent<TMP_Text>().text = "   " + item.quantity;
            }
            else
            {
                item.inventorySlot.transform.Find("Counter").GetComponent<TMP_Text>().text = "     " + item.quantity;
            }
            return;
        }

        item.character.inventory.inventory[item.inventorySlotIndex] = new EmptySlot();    //remove item from inventory
        item.inventorySlot.transform.Find("Counter").GetComponent<TMP_Text>().text = "";

        Transform slot =item.character.inventory.inventorySlots[item.inventorySlotIndex].transform.Find("Image");

        slot.GetComponentInChildren<RawImage>().texture = ItemAssets.Instance.emptySlot;
        slot.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
    }

    //reassigns all the "characters" fields of inventory items to either the combat or dungoen player
    public void ReassignInventoryCharacters(Character player)
    {
        for(int i = 0; i < inventory.Count; i++)
        {
            inventory[i].character = player;
        }
    }

}

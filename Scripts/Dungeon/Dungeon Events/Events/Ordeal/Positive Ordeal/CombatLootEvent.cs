using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatLootEvent : Ordeal
{
    private CombatManager combatManager;
    private GameObject combatPlayer;

    private int lootQuantity;
    private int lootSeen = 0;
    private Item itemToGive;
    private int itemAmount;

    private int timesInSetUp = 0;

    public CombatLootEvent(GameObject player, GameObject combatPlayer, ScenesManager sceneManager, CombatManager combatManager) : base (player, sceneManager)
    {
        this.combatPlayer = combatPlayer;
        this.combatManager = combatManager;

        switch (combatManager.encounterRating)
        {
            case int rating when rating <= 7:
                lootQuantity = Random.Range(1, 2); break;
            case int rating when rating <= 10:
                lootQuantity = Random.Range(1, 3); break;
            case int rating when rating <= 15:
                lootQuantity = Random.Range(2, 3); break;
            case int rating when rating < 20:
                lootQuantity = Random.Range(2, 4); break;
            case int rating when rating >= 20:
                lootQuantity = Random.Range(2, 6); break;
        }
        Debug.Log("Combat Encounter Rating: " + combatManager.encounterRating + " Loot Quantity: " + lootQuantity);

        SetUpEvent();
        SetUIActive();
    }


    public override void SetUpEvent()
    {
        Debug.Log("Setting up Combat Loot Event");
        int itemToGiveIndex = Random.Range(0, GainItemEvent.itemSelection.Count - 1);
        itemToGive = GainItemEvent.itemSelection[itemToGiveIndex];
        itemAmount = itemToGive.ItemAmount();

        if(lootSeen >= lootQuantity)
        {
            SetEndUIActive();
            UpdateEndEventText();
            UpdateEndButtonText(UIManager.Instance.eventButton1Object);
            UpdateEndEventButton();
        }
        else
        {
            SetUIActive();
            UpdateEventText();
            UpdateButtonText(UIManager.Instance.eventButton1Object);
            UpdateEventButton();
        }
    }

    public override void SetUIActive()
    {
        timesInSetUp++;
        if (timesInSetUp == 1) return;  //this prevents Ordeal constructor from setting UI before this constructor 

        UIManager.Instance.eventUIHolder.SetActive(true);
        UIManager.Instance.eventButton1Object.SetActive(true);
        UIManager.Instance.eventButton2Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(-20f, -30f, 0f);
        UIManager.Instance.eventButton2Object.GetComponent<RectTransform>().localPosition = new Vector3(20f, -30f, 0f);
    }

    //last UI for Event
    private void SetEndUIActive()
    {
        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);

        UIManager.Instance.eventUIHolder.SetActive(true);
        UIManager.Instance.eventButton1Object.SetActive(true);
        UIManager.Instance.eventButton2Object.SetActive(false);
    }

    protected override void UpdateEventText()
    {
        string eventText = "You Loot";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    //last Event Text for Encounter
    private void UpdateEndEventText()
    {
        string eventText = "Nothing else could be found";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void UpdateButtonText(GameObject eventButtonObject)
    {
        if (itemAmount == 1)
        {
            eventButtonObject.GetComponentInChildren<Text>().text = "Pick up a " + itemToGive.GetType();
        }
        else
        {
            eventButtonObject.GetComponentInChildren<Text>().text = "Pick up  " + itemAmount + " " + itemToGive.GetType() + "s";
        }

        UIManager.Instance.eventButton2Object.GetComponentInChildren<Text>().text = "Keep Looting";
    }

    //last Event Button Text
    private void UpdateEndButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Leave";
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);

        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, UpdateEventButtonLootListener, true);
    }

    //Updating only last button for Event
    private void UpdateEndEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, EndEvent, true);
    }

    public override void UpdateEventButtonListener()
    {
        lootSeen++;
        Inventory characterInventory = combatPlayer.GetComponent<Character>().inventory;

        for (int i = 0; i < itemAmount; i++)
        {
            characterInventory. AddItem(itemToGive, combatPlayer.GetComponent<Character>());
        }

        SetUpEvent();
    }

    //Player doesn't pick up the item button listener
    private void UpdateEventButtonLootListener()
    {
        lootSeen++;
        SetUpEvent();
    }

    protected override void SetUIInactive()
    {
        UIManager.Instance.eventUIHolder.SetActive(false);
        UIManager.Instance.eventButton1Object.SetActive(false);
        UIManager.Instance.eventButton2Object.SetActive(false);
    }

    //Ends the combat through CombatManager and leaves the combat room
    protected override void EndEvent()
    {
        //since no event was every actually added to dungeon, no need to remove one from SceneManager
        SetUIInactive();
        UnblockPlayerMovement();

        combatManager.EndCombat();
    }

}

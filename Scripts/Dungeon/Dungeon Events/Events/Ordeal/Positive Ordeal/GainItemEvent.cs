using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GainItemEvent : PositiveOrdeal
{
    private Item itemToGive;
    private int itemAmount;

    public static List<Item> itemSelection = new List<Item>()
                { new Arrow(), new Bandage(), new Bolt(), new Camp(), new Potion_Fire_Resistance(),
                new Candle(), new Gold(), new Lamp(), new Lockpick(), new Potion_Frost(),
                new Potion_Greater_Healing(), new Potion_Healing(), new Potion_Superior_Healing(),
                new Potion_Poison(), new Potion_Protection(), new Potion_Speed(), new Potion_Superior_Healing(),
                new Potion_Strength(), new Potion_Water_Resistance(), new Ration(), new Rope(),
                new Shovel(), new Torch(), new Vial_Frost(), new Vial_Oil(), new Vial_Poison(), new WarHorn()};

    public GainItemEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        
        ProbabilityGenerator itemSelector = new ProbabilityGenerator(new float[] { 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f,
                                                                                    0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, 0.037f, });
        int itemSelected = itemSelector.GenerateNumber();
        itemToGive = itemSelection[itemSelected];
        itemAmount = itemToGive.ItemAmount();

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object);
        UpdateEventButton();
    }

    protected override void UpdateEventText()
    {
        string eventText = "An item lays glimmering in the shadow before you confident its previous owner wont have need of it any time soon.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void UpdateButtonText(GameObject eventButtonObject)
    {
        if(itemAmount == 1)
        {
            eventButtonObject.GetComponentInChildren<Text>().text = "Pick up a " + itemToGive.GetType();
        }
        else
        {
            eventButtonObject.GetComponentInChildren<Text>().text = "Pick up  " + itemAmount + " " + itemToGive.GetType() + "s";
        }
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
    }

    public override void UpdateEventButtonListener()
    {
        Inventory characterInventory = player.GetComponent<Character>().inventory;

        for(int i = 0; i < itemAmount; i++)
        {
            characterInventory.AddItem(itemToGive, player.GetComponent<Character>());
        }
        EndEvent();
    }
}

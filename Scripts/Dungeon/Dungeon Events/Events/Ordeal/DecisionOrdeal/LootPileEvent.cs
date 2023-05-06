using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootPileEvent : DecisionOrdeal
{
    private bool successLoot;
    private Item itemToGive;
    private int itemAmount;

    int fearGained;

    public LootPileEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        decisionProb = 0.65f;

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object);
        UpdateButton2Text(UIManager.Instance.eventButton2Object);
        UpdateEventButton();
    }

    private void ContinueEvent()
    {
        //player finds a random item
        if (successLoot)
        {
            fearGained = Random.Range(2, 6);
            playerScript.AddFear(-fearGained);

            int itemToGiveIndex = Random.Range(0, GainItemEvent.itemSelection.Count - 1);
            itemToGive = GainItemEvent.itemSelection[itemToGiveIndex];
            itemAmount = itemToGive.ItemAmount();

            UpdateSuccessText();
            UpdateSuccessButtonText(UIManager.Instance.eventButton1Object);
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateSuccessButtonListener, true);
        }
        else
        {
            fearGained = Random.Range(1, 5);
            playerScript.AddFear(fearGained);

            UIManager.Instance.eventButton2Object.SetActive(false);
            UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);

            UpdateFailText();
            UpdateButton2Text(UIManager.Instance.eventButton1Object);
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, EndEvent, true);
        }
    }

    protected override void UpdateEventText()
    {
        string eventText = "A pile of carcasses, bones and debris scatter the floor. The sounds of unsettling silence fills the air as you shift your way through the rubble. Although this scene of desolation may turn a profit for passersby.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    //player finds loot
    private void UpdateSuccessText()
    {
        string eventText = "Scouring through the mass you come across something malleable, perhaps useful?";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    //player doesn't find loot
    private void UpdateFailText()
    {
        string eventText = "Sifting through the carnage in hopes for something of use, nothing of value could be found. Leaving the scene in vain and in the hands of the restless dead";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void UpdateButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Loot";
    }

    private void UpdateButton2Text(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Move On";
    }

    private void UpdateSuccessButtonText(GameObject eventButtonObject)
    {
        if (itemAmount == 1)
            eventButtonObject.GetComponentInChildren<Text>().text = "Pick up a " + itemToGive.GetType();
        else
            eventButtonObject.GetComponentInChildren<Text>().text = "Pick up  " + itemAmount + " " + itemToGive.GetType() + "s";
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, EndEvent, true);
    }

    //if player successfully found loot
    private void UpdateSuccessButtonListener()
    {
        for(int i = 0; i < itemAmount; i++)
        {
            playerScript.inventory.AddItem(itemToGive, playerScript);
        }
        EndEvent();
    }

    public override void UpdateEventButtonListener()
    {
        if(Random.value <= decisionProb)
        {
            successLoot = true;
        }
        ContinueEvent();
    }
}

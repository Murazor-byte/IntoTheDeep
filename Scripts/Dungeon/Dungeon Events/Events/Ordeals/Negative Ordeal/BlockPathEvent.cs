using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockedPathEvent : NegativeOrdeal
{
    private int fearGained;
    private bool hasShovel;
    private bool usedShovel;

    public BlockedPathEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Excavate (Shovel)");
        UpdateButtonText(UIManager.Instance.eventButton2Object, "Unearth");
        UpdateEventButton();

        for (int i = 0; i < playerScript.inventory.inventorySlots.Count; i++)
        {
            if (playerScript.inventory.inventory[i] is Shovel) hasShovel = true;
        }

        if (!hasShovel) UIManager.Instance.eventButton1.interactable = false;
    }

    public override void SetUIActive()
    {
        UIManager.Instance.eventUIHolder.SetActive(true);
        UIManager.Instance.eventButton1Object.SetActive(true);
        UIManager.Instance.eventButton2Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(-20f, -30f, 0f);
        UIManager.Instance.eventButton2Object.GetComponent<RectTransform>().localPosition = new Vector3(20f, -30f, 0f);
    }

    private void ContinueEvent()
    {
        UIManager.Instance.eventButton1.interactable = true;
        UIManager.Instance.eventButton2Object.SetActive(false);
        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);

        playerScript.AddFear(fearGained);

        if (usedShovel)
        {
            UpdateShovelEventText();
        }
        else
        {
            UpdateHandsEventText();
        }

        UpdateButtonText(UIManager.Instance.eventButton1Object, "Gain " + fearGained + " Fear");
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, EndEvent, true);
    }

    protected override void UpdateEventText()
    {
        string eventText = "Along the twisting corridors you come across what seems to be a dead end. Although strange as if the continuation of the path follows right through," +
            " you are certain more lies beyond this thin layer of rubble. Excavation would be swift, if one came prepared.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateShovelEventText()
    {
        string eventText = "With swiftness the debris becomes cleared, paving the way forward at the cost of your equipment.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateHandsEventText()
    {
        string eventText = "Through heavy labor the rubble begins deconstruction one stone at a time. Your work ultimately unfruitful as the path opens" +
            " yet furhter into the abyss";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public void UpdateButtonText(GameObject eventButtonObject, string buttonText)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = buttonText;
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, UseHands, true);
    }

    public override void UpdateEventButtonListener()
    {
        fearGained = Random.Range(1, 4);
        usedShovel = true;

        for(int i = 0; i < playerScript.inventory.inventorySlots.Count; i++)
        {
            if (playerScript.inventory.inventory[i] is Shovel) playerScript.inventory.inventory[i].UseItem();
        }
        ContinueEvent();
    }

    private void UseHands()
    {
        fearGained = Random.Range(15, 25);
        ContinueEvent();
    }
}

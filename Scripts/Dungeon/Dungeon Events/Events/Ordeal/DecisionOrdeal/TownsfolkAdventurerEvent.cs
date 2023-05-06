using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//event where player crosses an adventurer willing to sell their items
public class TownsfolkAdventurerEvent : DecisionOrdeal
{
    public TownsfolkAdventurerEvent(GameObject player, ScenesManager sceneManager) : base (player, sceneManager) { }

    public override void SetUpEvent()
    {
        Debug.Log("Setting up Townsfolk Adventurer Event");

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Trade");
        UpdateButtonText(UIManager.Instance.eventButton2Object, "Move On");
        UpdateEventButton();
        UIManager.Instance.SetUpDungeonShop(playerScript);
    }

    protected override void UpdateEventText()
    {
        string eventText = "Along your path you notice movement, eratic but planned as if it doesn't want to be seen. With closer inspection you realize it to be a lost but adventurous " +
            "townsfolk. With introductions he's willing to trade goods for some coin.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void ChangeEventTextAfterShop()
    {
        string eventText = "He looks at you in a puzzled and concerned manner, then asks if you want anything else.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public void UpdateButtonText(GameObject eventButtonObject, string buttonText)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = buttonText;
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateShopEventButtonListener, true);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, LeaveEvent, true);
    }

    private void UpdateShopEventButtonListener()
    {
        ChangeEventTextAfterShop();
        UIManager.Instance.SetDungeonShopActive(true);
    }

    private void LeaveEvent()
    {
        UIManager.Instance.dungeonShop.ClearInventory();    //clear shop inventory before leaving
        EndEvent();
    }
}

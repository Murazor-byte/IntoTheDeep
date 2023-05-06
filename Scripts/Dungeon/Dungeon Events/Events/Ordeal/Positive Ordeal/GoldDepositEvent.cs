using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldDepositEvent : PositiveOrdeal
{
    private int fearLost;
    private bool hasShovel;
    private bool usedShovel;
    private int goldGained;

    public GoldDepositEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Mine (Shovel)");
        UpdateButtonText(UIManager.Instance.eventButton2Object, "Remove");
        UpdateEventButton();

        for(int i = 0; i < playerScript.inventory.inventorySlots.Count; i++)
        {
            if (playerScript.inventory.inventory[i] is Shovel) hasShovel = true;
        }

        if (!hasShovel) UIManager.Instance.eventButton1.interactable = false;
    }

    private void ContinueEvent()
    {
        UIManager.Instance.eventButton1.interactable = true;
        UIManager.Instance.eventButton2Object.SetActive(false);
        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);

        fearLost = Random.Range(2, 6);
        playerScript.AddFear(-fearLost);

        if (usedShovel)
        {
            UpdateShovelEventText();
            goldGained = Random.Range(20, 100);
        }
        else
        {
            UpdateHandsEventText();
            goldGained = Random.Range(5, 40);
        }

        UpdateButtonText(UIManager.Instance.eventButton1Object, "Gain " + goldGained + " Gold");
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, GiveGold, true);
    }

    protected override void UpdateEventText()
    {
        string eventText = "The dim shimmers of pocketed wealth encompass this room, lining the walls with gold. " +
            "Despite the dungeon dwellers desctrution of life, a rich mineral deposit lies untouched.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateShovelEventText()
    {
        string eventText = "With preperations met, excavation is started. Stuffing pouches with renewed affluence until tools " +
            "become blunted and the mineral vein hardly altered.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateHandsEventText()
    {
        string eventText = "Despite the lack of equipment, manual extraction is possible with wealth to be obtained through the scraps. " +
            "With the loose rocks procured, the remains of the lode is left to the prepared.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateButtonText(GameObject eventButtonObject, string buttonText)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = buttonText;
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, ContinueEvent, true);
    }

    public override void UpdateEventButtonListener()
    {
        usedShovel = true;

        for (int i = 0; i < playerScript.inventory.inventorySlots.Count; i++)
        {
            if (playerScript.inventory.inventory[i] is Shovel) playerScript.inventory.inventory[i].UseItem();
        }
        ContinueEvent();
    }

    private void GiveGold()
    {
        for(int i = 0; i < goldGained; i++)
        {
            playerScript.inventory.AddItem(new Gold(), playerScript);
        }

        EndEvent();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EatFoodEvent : NegativeOrdeal
{
    private bool hasRations;
    private int fearGained;
    private const int rationsNeeded = 2;

    public EatFoodEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Eat");
        UpdateButtonText(UIManager.Instance.eventButton2Object, "Starve");
        UpdateEventButton();

        for(int i = 0; i < playerScript.inventory.inventorySlots.Count; i++)
        {
            if(playerScript.inventory.inventory[i] is Ration && playerScript.inventory.inventory[i].quantity >= rationsNeeded)
            {
                hasRations = true;
            }
        }
        if (!hasRations) UIManager.Instance.eventButton1.interactable = false;
    }

    public override void SetUIActive()
    {
        UIManager.Instance.eventUIHolder.SetActive(true);
        UIManager.Instance.eventButton1Object.SetActive(true);
        UIManager.Instance.eventButton2Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(-20f, -30f, 0f);
        UIManager.Instance.eventButton2Object.GetComponent<RectTransform>().localPosition = new Vector3(20f, -30f, 0f);
    }

    protected override void UpdateEventText()
    {
        string eventText = "Prologned exploring prefaces hunger, where an empty stomach leads to an empty mind desperate for food.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateButtonText(GameObject eventButtonObject, string buttonText)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = buttonText;
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, Starve, true);
    }

    public override void UpdateEventButtonListener()
    {
        for(int i = 0; i < playerScript.inventory.inventorySlots.Count; i++)
        {
            if(playerScript.inventory.inventory[i] is Ration)
            {
                for (int j = 0; j < rationsNeeded; j++)
                    playerScript.inventory.inventory[i].UseItem();
            }
        }
        UIManager.Instance.eventButton1.interactable = true;
        EndEvent();
    }

    private void Starve()
    {
        fearGained = Random.Range(5, 12);
        playerScript.statusEffects.Add(new Starving(playerScript));
        playerScript.AddFear(fearGained);
        UIManager.Instance.eventButton1.interactable = true;
        EndEvent();
    }
}

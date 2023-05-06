using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeadAnimalEvent : DecisionOrdeal
{
    private int fearGained;
    private int rationsGiven;

    public DeadAnimalEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        decisionProb = 0.7f;

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Consume");
        UpdateButtonText(UIManager.Instance.eventButton2Object, "Move On");
        UpdateEventButton();
    }

    private void ContinueEvent()
    {
        UIManager.Instance.eventButton2Object.SetActive(false);
        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);

        if (succeeded)
        {
            fearGained = Random.Range(1, 5);
            rationsGiven = Random.Range(2, 6);

            UpdateSuccessText();
            UpdateButtonText(UIManager.Instance.eventButton1Object, "Gain " + rationsGiven + " Rations");
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, SafeMeat, true);
        }
        else
        {
            fearGained = Random.Range(4, 8);
            UpdateFailText();
            UpdateButtonText(UIManager.Instance.eventButton1Object, "Poisoned!");
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, Poisoned, true);
        }
    }

    protected override void UpdateEventText()
    {
        string eventText = "Even though the smell of the dungeon is as volatile as its tunnels, a new scent emerges. An animals" +
            " carcass lies ahead. Unkown if its demise was an accident or intentional, it seems fresh. Although eating meat can be dangerous to consume when uncured, it's a risk some will take.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateSuccessText()
    {
        string eventText = "With a handful of meat eaten, you don't feel any worse for wear. Although usually unsafe, this seems edibale to take.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateFailText()
    {
        string eventText = "Confidence in your instincts is crucial for survival, although the gut may disagree. With undue side effects, your stomach clenches and a sweat starts to form. " +
            "Your body begins to feel weak as you surpress the urge to disgorge";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateButtonText(GameObject eventButtonObject, string buttonText)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = buttonText;
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, EndEvent, true);
    }

    public override void UpdateEventButtonListener()
    {
        if (Random.value <= decisionProb) succeeded = true;
        ContinueEvent();
    }

    private void SafeMeat()
    {
        for(int i = 0; i < rationsGiven; i++)
        {
            playerScript.inventory.AddItem(new Ration(), playerScript);
        }
        playerScript.AddFear(-fearGained);
        EndEvent();
    }

    private void Poisoned()
    {
        playerScript.statusEffects.Add(new Poisoned(playerScript));
        playerScript.AddFear(fearGained);
        EndEvent();
    }
}

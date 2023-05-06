using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MysteriousFungusEvent : DecisionOrdeal
{
    private int fearGained;
    private int rationsGiven;
    private int healthLost;

    public MysteriousFungusEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        decisionProb = 0.6f;

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

        //fungus is safe to eat
        if (succeeded)
        {
            rationsGiven = Random.Range(1, 3);
            fearGained = Random.Range(1, 3);

            UpdateSuccessEventText();
            if(rationsGiven == 1) UpdateButtonText(UIManager.Instance.eventButton1Object, "Gain " + rationsGiven + " Ration");
            else UpdateButtonText(UIManager.Instance.eventButton1Object, "Gain " + rationsGiven + " Rations");

            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, GainRations, true);
        }
        else
        {
            float poisoned = 0.35f;
            //player only gets hurt
            if(Random.value > poisoned)
            {
                healthLost = Random.Range(2, 5);
                fearGained = Random.Range(2, 6);

                UpdatePainEventText();
                UpdateButtonText(UIManager.Instance.eventButton1Object, "Lose " + healthLost + " Health");
                UIManager.Instance.AddListener(UIManager.Instance.eventButton1, HurtConsumption, true);
            }
            //gets poisoned
            else
            {
                fearGained = Random.Range(4, 8);
                UpdatePoisonEventText();
                UpdateButtonText(UIManager.Instance.eventButton1Object, "Poisoned!");
                UIManager.Instance.AddListener(UIManager.Instance.eventButton1, Poisoned, true);
            }
        }
    }

    protected override void UpdateEventText()
    {
        string eventText = "The continuation of forming fungus becomes more present down this path, creating small clusters large enough for consumption. " +
            "Unsure if these are safe to ingest, it can be a quick relief to staving death.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateSuccessEventText()
    {
        string eventText = "Enough parcels can be obtained to carry for future use. Although not satisfying, it is edible.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdatePainEventText()
    {
        string eventText = "After a few bites, the immediate onset of distress becomes prevalent. Although managable, you're certain any more will bring additional harm.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdatePoisonEventText()
    {
        string eventText = "An immediate arrival of pain emerges as your stomach begins to tremble. It may have " +
            "been wise to have left these be, lest you should succumb to poison.";
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

    private void GainRations()
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

    private void HurtConsumption()
    {
        playerScript.Heal(-healthLost);
        playerScript.AddFear(fearGained);
        EndEvent();
    }
}

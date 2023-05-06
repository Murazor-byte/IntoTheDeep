using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NaturalPitEvent : DecisionOrdeal
{
    private bool hasRope;
    private bool successManeuver;
    private int fearGained;
    private int healthLost;
    
    public NaturalPitEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        decisionProb = 0.6f;

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object);
        UpdateButton2Text(UIManager.Instance.eventButton2Object);
        UpdateEventButton();

        for (int i = 0; i < playerScript.inventory.inventorySlots.Count; i++)
        {
            if (playerScript.inventory.inventory[i] is Rope) hasRope = true;
        }
        if (!hasRope) UIManager.Instance.eventButton1.interactable = false;
    }

    private void ContinueEvent()
    {
        if (successManeuver)
        {
            UpdateSuccessEventText();
            UpdateButtonEndText(UIManager.Instance.eventButton1Object);
        }
        else
        {
            fearGained = Random.Range(4, 9);
            healthLost = Random.Range(3, 12);
            playerScript.AddFear(fearGained);
            playerScript.TakeDamage(healthLost);
            UpdateFailEventText();
            UpdateButtonFailEndText(UIManager.Instance.eventButton1Object);
        }

        UIManager.Instance.eventButton1.interactable = true;
        UIManager.Instance.eventButton2Object.SetActive(false);
        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, EndEvent, true);
    }

    protected override void UpdateEventText()
    {
        string eventText = "A piercing breeze halts your progress as a natural crevasse lies beyond. Unsure of how deep " +
            "certain portions of the cave exists sharp stalagmites await anyone for a sharp surprise. Certain some rope can make a safe passage across, or finding a tight squeeze near the slick walls may be the better option.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateSuccessEventText()
    {
        string eventText = "Slowly making headway around the pit, grasping at the slick rock to maintain a resemblence of footing, the other side is soon safely under your feet.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateFailEventText()
    {
        string eventText = "Creeping around the pit using only the slicked walls for balance the opposite side seems in sight." +
            "Unsure whether the wet footing underneath you, a rock that was bumped, or the abyss drawing you in, you hit a freefall dropping to the bottom of the crevasse meeting bone with rock. " +
            "Although the pain is hard to shake, escaping back out the pit was just as difficult.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void UpdateButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Use Rope";
    }

    private void UpdateButton2Text(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Attempt Crossing";
    }

    private void UpdateButtonEndText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Move on";
    }

    private void UpdateButtonFailEndText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Lose " + healthLost + " Health";
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, UpdateEventButton2Listener, true);
    }

    //using a rope to pass the natural pit
    public override void UpdateEventButtonListener()
    {
        fearGained = Random.Range(2, 8);
        for (int i = 0; i < playerScript.inventory.inventorySlots.Count; i++)
        {
            if (playerScript.inventory.inventory[i] is Rope) playerScript.inventory.inventory[i].UseItem();
        }
        playerScript.AddFear(-fearGained);
        EndEvent();
    }

    //tyring to maneuver around the pit near the side cave walls
    private void UpdateEventButton2Listener()
    {
        if(Random.value < decisionProb)
        {
            successManeuver = true;
        }
        ContinueEvent();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FogEvent : NegativeOrdeal
{
    private int fearGained;
    private float success;
    private bool succeeded;
    
    public FogEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        success = 0.55f;

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Continue");
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
    }

    private void ContinueEvent()
    {
        if (succeeded)
        {
            UpdateSuccesText();
            UpdateButtonText(UIManager.Instance.eventButton1Object, "Move On");
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, EndEvent, true);
        }
        else
        {
            fearGained = Random.Range(2, 6);
            UpdateFailText();
            UpdateButtonText(UIManager.Instance.eventButton1Object, "Gain " + fearGained + " Fear");
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEndEvent, true);
        }
    }

    protected override void UpdateEventText()
    {
        string eventText = "A mysterious fog appears abruptly from the walls and beyond the cave, " +
            "holding an unnatural thickness as it looms in the vicinity.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateSuccesText()
    {
        string eventText = "Strangely, It dissipates as quick as it came, leaving nothing but the slick rocks beside you.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateFailText()
    {
        string eventText = "Its unsettling presence brings more than just wet percipitation. This dungeon inhabits more than just wandering creatures.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateButtonText(GameObject eventButtonObject, string buttonText)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = buttonText;
    }

    public override void UpdateEventButtonListener()
    {
        if (Random.value <= success) succeeded = true;
        ContinueEvent();
    }

    private void UpdateEndEvent()
    {
        playerScript.AddFear(fearGained);
        EndEvent();
    }
}

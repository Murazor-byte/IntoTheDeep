using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundEvent : NegativeOrdeal
{
    private int fearGained = 0;

    public SoundEvent(GameObject player, ScenesManager sceneManage) : base(player, sceneManage) { }

    public override void SetUpEvent()
    {
        Debug.Log("Setting up Sound Event");
        fearGained = Random.Range(0, 5);

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object);
        UpdateEventButton();
    }

    public override void SetUIActive()
    {
        UIManager.Instance.eventUIHolder.SetActive(true);
        UIManager.Instance.eventButton1Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);
    }

    protected override void UpdateEventText()
    {
        string eventText = "Strange chatters and screeches echo off the walls. Perhaps they are nearby.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void UpdateButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Gain " + fearGained + " fear";
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
    }

    public override void UpdateEventButtonListener()
    {
        playerScript.AddFear(fearGained);
        EndEvent();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighHorrorEvent : NegativeOrdeal
{
    private int fearGained;

    public HighHorrorEvent(GameObject player, ScenesManager scenemanager) : base (player, scenemanager) { }

    public override void SetUpEvent()
    {
        switch (playerScript.horror)
        {
            case int horror when horror <= 20:
                fearGained = Random.Range(0, 5);
                break;
            case int horror when horror <= 40:
                fearGained = Random.Range(6, 10);
                break;
            case int horror when horror <= 60:
                fearGained = Random.Range(11, 16);
                break;
            case int horror when horror <= 80:
                fearGained = Random.Range(17, 23);
                break;
            case int horror when horror <= 100:
                fearGained = Random.Range(24, 30);
                break;
        }

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
        string eventText = "This strange mutation is beginning to take hold. You feel your blood boiling underneath your skin, your eyes filling with anger, and a strange malevolant feeling looming over you. It's getting hard to shake it off.";
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

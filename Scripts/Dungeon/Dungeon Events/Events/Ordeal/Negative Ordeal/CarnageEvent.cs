using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarnageEvent : NegativeOrdeal
{
    private int fearGained = 0;

    public CarnageEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        Debug.Log("Setting up carnage event");
        fearGained = Random.Range(3, 10);

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
        string eventText = "The stench of death, the view of carnage. There is nothing but blood and bodies that pattern these walls. Nothing escapes this place.";
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

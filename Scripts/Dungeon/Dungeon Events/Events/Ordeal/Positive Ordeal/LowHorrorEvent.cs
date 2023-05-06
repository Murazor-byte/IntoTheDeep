using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LowHorrorEvent : PositiveOrdeal
{
    private int fearLost;

    public LowHorrorEvent(GameObject player, ScenesManager sceneManager) : base (player, sceneManager) { }

    public override void SetUpEvent()
    {
        switch (playerScript.horror)
        {
            case int horror when horror <= 20:
                fearLost = Random.Range(10, 15);
                break;
            case int horror when horror <= 40:
                fearLost = Random.Range(7, 10);
                break;
            case int horror when horror <= 60:
                fearLost = Random.Range(5, 7);
                break;
            case int horror when horror <= 80:
                fearLost = Random.Range(3, 5);
                break;
            case int horror when horror <= 100:
                fearLost = Random.Range(0, 3);
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
        string eventText = "Unkown if this affliction will ever take hold, you can surely set yourself apart from the horrors that walk these tunnels.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void UpdateButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Lose " + fearLost + " fear";
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
    }

    public override void UpdateEventButtonListener()
    {
        playerScript.AddFear(-fearLost);
        EndEvent();
    }
}

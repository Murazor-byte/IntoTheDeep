using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LowFightingEvent : PositiveOrdeal
{
    private int fearLost;

    public LowFightingEvent(GameObject player, ScenesManager sceneManager) : base (player, sceneManager) { }

    public override void SetUpEvent()
    {
        int movesSince = playerDungeonMovement.movesSinceCombat;

        switch (movesSince)
        {
            case int moves when moves <= 4:
                fearLost = 0;
                break;
            case int moves when moves <= 8:
                fearLost = Random.Range(1, 4);
                break;
            case int moves when moves <= 10:
                fearLost = Random.Range(4, 8);
                break;
            case int moves when moves <= 13:
                fearLost = Random.Range(8, 10);
                break;
            case int moves when moves <= 15:
                fearLost = Random.Range(10, 13);
                break;
            case int moves when moves <= 20:
                fearLost = Random.Range(13, 15);
                break;
            case int moves when moves <= 30:
                fearLost = Random.Range(15, 20);
                break;
            case int moves when moves > 30:
                fearLost = Random.Range(20, 30);
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
        string eventText = "There is a small victory in escaping the evil presence in here. It's been some time since your last encounter with danger, unkowning of how long this streak will last.";
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LowMonstersKilledEvent : NegativeOrdeal
{
    private int fearGained;

    public LowMonstersKilledEvent(GameObject player, ScenesManager sceneManager) : base (player, sceneManager) { }

    public override void SetUpEvent()
    {
        float number = playerScript.layerNumber * 0.35f;

        switch (playerScript.enemiesKilled)
        {
            case int killed when killed <= 0 * number:
                fearGained = Random.Range(14, 17);
                break;
            case int killed when killed <= 5 * number:
                fearGained = Random.Range(11, 13);
                break;
            case int killed when killed <= 10 * number:
                fearGained = Random.Range(9, 11);
                break;
            case int killed when killed <= 15 * number:
                fearGained = Random.Range(7, 9);
                break;
            case int killed when killed <= 20 * number:
                fearGained = Random.Range(5, 7);
                break;
            case int killed when killed <= 30 * number:
                fearGained = Random.Range(3, 5);
                break;
            case int killed when killed > 30 * number:
                fearGained = Random.Range(0, 3);
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
        string eventText = "The bodies of those slain fill the halls behind you. But is it enough? Perhaps the screeching and moving shadows are more gathering around the corner. You can't be certain.";
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

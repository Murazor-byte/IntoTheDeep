using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighMonstersKilledEvent : PositiveOrdeal
{
    private int fearLost;

    public HighMonstersKilledEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        float number = playerScript.layerNumber * 0.35f;

        switch (playerScript.enemiesKilled)
        {
            case int killed when killed > 30 * number:
                fearLost = Random.Range(12, 15);
                break;
            case int killed when killed >= 30 * number:
                fearLost = Random.Range(9, 11);
                break;
            case int killed when killed >= 20 * number:
                fearLost = Random.Range(7, 9);
                break;
            case int killed when killed >= 15 * number:
                fearLost = Random.Range(5, 7);
                break;
            case int killed when killed >= 10 * number:
                fearLost = Random.Range(3, 5);
                break;
            case int killed when killed >= 5 * number:
                fearLost = Random.Range(1, 3);
                break;
            case int killed when killed >= 0 * number:
                fearLost = 0;
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
        string eventText = "Uncertain whether there are more dangers looming in the distant shadows, you can be certain by your hand there are less roaming these caves, for the time being.";
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

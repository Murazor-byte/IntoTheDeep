using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrokenLightEvent : PositiveOrdeal
{
    private int fearLost;
    
    public BrokenLightEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        fearLost = Random.Range(3, 7);

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Lose " + fearLost + " Fear");
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
    }

    public override void SetUIActive()
    {
        UIManager.Instance.eventUIHolder.SetActive(true);
        UIManager.Instance.eventButton1Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);
    }

    protected override void UpdateEventText()
    {
        string eventText = "The appearance of something concealed for some time presents itself. A streak of light that has found " +
            "its way down this depth, quietly illuminating the shadows around you. A subtle reminder that there is more than just this dark and temporary place.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateButtonText(GameObject eventButtonObject, string buttonText)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = buttonText;
    }

    public override void UpdateEventButtonListener()
    {
        playerScript.AddFear(-fearLost);
        EndEvent();
    }
}

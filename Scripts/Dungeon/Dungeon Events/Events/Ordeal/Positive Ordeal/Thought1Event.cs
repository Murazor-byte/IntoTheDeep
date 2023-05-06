using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Thought1Event : PositiveOrdeal
{
    public Thought1Event(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Move On");
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, EndEvent, true);
    }

    public override void SetUIActive()
    {
        UIManager.Instance.eventUIHolder.SetActive(true);
        UIManager.Instance.eventButton1Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);
    }

    protected override void UpdateEventText()
    {
        string eventText = "The repeated cycles of corridors leaves the mind to wander alone. To think where you are amid the endless dungeon, and how far to the nearest light. " +
            "Perhaps the next bend can clear the desired path, or untold dangers lying in wait. Maybe best not think of it.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateButtonText(GameObject eventButtonObject, string buttonText)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = buttonText;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PositiveOrdeal : Ordeal
{
    public PositiveOrdeal(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    //if it's not a choiced event, hero can pick up the item or leave it
    public override void SetUIActive()
    {
        UIManager.Instance.eventUIHolder.SetActive(true);
        UIManager.Instance.eventButton1Object.SetActive(true);
        UIManager.Instance.eventButton2Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(-20f, -30f, 0f);
        UIManager.Instance.eventButton2Object.GetComponent<RectTransform>().localPosition = new Vector3(20f, -30f, 0f);

        SetUpEventButton2();
    }

    private void SetUpEventButton2()
    {
        Debug.Log("Setting Button2 listener");

        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, EndEvent, true);

        UIManager.Instance.eventButton2.GetComponentInChildren<Text>().text = "Move on";
    }

    public override void SetUpEvent() { }

    protected override void UpdateEventText() { }

    public override void UpdateButtonText(GameObject eventButtonObject) { }

    protected override void UpdateEventButton() { }

    public override void UpdateEventButtonListener() { }
}

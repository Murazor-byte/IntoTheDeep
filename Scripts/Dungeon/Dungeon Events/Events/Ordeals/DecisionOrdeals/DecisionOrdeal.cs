using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//event type that has exactly two options to choose from, where the attempt will be probablistically rolled
public class DecisionOrdeal : Ordeal
{
    protected float decisionProb;
    protected bool succeeded;

    public DecisionOrdeal(GameObject player, ScenesManager sceneManager) : base (player, sceneManager) { }

    //set up positioning of buttons for two choiced event
    public override void SetUIActive()
    {
        UIManager.Instance.eventUIHolder.SetActive(true);
        UIManager.Instance.eventButton1Object.SetActive(true);
        UIManager.Instance.eventButton2Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(-20f, -30f, 0f);
        UIManager.Instance.eventButton2Object.GetComponent<RectTransform>().localPosition = new Vector3(20f, -30f, 0f);
    }

    public override void SetUpEvent() { }

    protected override void UpdateEventText() { }

    public override void UpdateButtonText(GameObject eventButtonObject) { }

    protected override void UpdateEventButton() { }

    public override void UpdateEventButtonListener() { }
}

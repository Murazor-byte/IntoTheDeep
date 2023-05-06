using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NegativeOrdeal : Ordeal
{

    public NegativeOrdeal(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    //if it's not a choiced event, hero must take the negative ordeal
    public override void SetUIActive()
    {
        UIManager.Instance.eventButton1.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);

        UIManager.Instance.eventUIHolder.SetActive(true);

        UIManager.Instance.eventButton1Object.SetActive(true);
    }

    public override void SetUpEvent() {}

    protected override void UpdateEventText() {}

    public override void UpdateButtonText(GameObject eventButtonObject) {}

    protected override void UpdateEventButton() {}

    public override void UpdateEventButtonListener() {}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrevasseEvent : NegativeOrdeal
{
    private int fearGained = 0;
    private int healthLost = 0;
    private const float SUCCESSPROB = 0.55f;
    private bool evaded;

    public CrevasseEvent(GameObject player, ScenesManager sceneManager) : base (player, sceneManager) { }

    public override void SetUpEvent()
    {
        Debug.Log("Setting up Fall Event");
        healthLost = Random.Range(3, (int)(playerScript.health * 0.35f));

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object);
        UpdateEventButton();
    }

    private void ContinueEvent()
    {
        if (!evaded) UpdateFailEventText();
        else UpdateSuccessEventText();

        UpdateEndButtonText(UIManager.Instance.eventButton1Object);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEndEventButtonListener, true);
    }

    public override void SetUIActive()
    {
        UIManager.Instance.eventUIHolder.SetActive(true);
        UIManager.Instance.eventButton1Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);
    }

    protected override void UpdateEventText()
    {
        string eventText = "The air feels fresh in this region, as if a quick breeze is in constant blow " +
                            "Unbeknownst to those not looking, an unseen crevasse lies before your taken step as you attempt to recoil.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateSuccessEventText()
    {
        string eventText = "The distraction quickly abates as you regather your footing";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateFailEventText()
    {
        string eventText = "Unable to reposition in time your momentum takes you forward into the hole as the jagged rocks and sharp stones bruise your exterior. The slopes aren't as steep as you presumed while climbing out.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void UpdateButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Attempt Evade";
    }

    private void UpdateEndButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Gain " + fearGained + " fear";
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
    }

    public override void UpdateEventButtonListener()
    {
        if(Random.value <= SUCCESSPROB)
        {
            evaded = true;
            fearGained = Random.Range(0, 5);
        }
        else
        {
            fearGained = Random.Range(5, 20);
        }
        ContinueEvent();
    }

    private void UpdateEndEventButtonListener()
    {
        if (!evaded) playerScript.TakeDamage(healthLost);

        playerScript.AddFear(fearGained);
        EndEvent();
    }
}

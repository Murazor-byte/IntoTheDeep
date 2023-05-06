using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaveInEvent : NegativeOrdeal
{
    private int healthLost = 0;
    private int fearGained = 0;
    private const float SUCCESSPROB = 0.45f;
    private bool evaded = false;

    public CaveInEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        Debug.Log("Setting up Cave In event");
        healthLost = Random.Range(2,(int)(playerScript.health * 0.5f));

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
        string eventText = "The crackling of rock, the falling of stone, the weight of death. You attempt to escape it.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateSuccessEventText()
    {
        string eventText = "Without a thought you sidestep the falling debris and leave the rubble in its wake. A disaster closely avoided.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateFailEventText()
    {
        string eventText = "A wet thud followed by a crack and the weight of the walls are atop you. You manage to climb out and regather yourself.";
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

    //inital button listener to attempt to evade cave in
    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
    }

    //inital listener to roll for evasion
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

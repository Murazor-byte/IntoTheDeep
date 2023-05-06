using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscapeCursedPuddleEvent : NegativeOrdeal
{
    private int fearGained;
    private int horrorGained;
    private const float SUCCESSPROB = 0.65f;
    private bool escaped;

    public EscapeCursedPuddleEvent(GameObject player, ScenesManager sceneManager) : base (player, sceneManager) { }

    public override void SetUpEvent()
    {
        Debug.Log("Setting up Escape Cursed Puddle Event");

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object);
        UpdateEventButton();
    }

    private void ContinueEvent()
    {
        if (!escaped)
        {
            UpdateFailEventText();
            UpdateEndButtonText(UIManager.Instance.eventButton1Object,"Gain " + horrorGained + " Horror");
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEndFailEventButtonListener, true);
        }
        else
        {
            UpdateSuccessEventText();
            UpdateEndButtonText(UIManager.Instance.eventButton1Object, "Move On");
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, EndEvent, true);
        }
        playerScript.AddFear(fearGained);
    }

    public override void SetUIActive()
    {
        UIManager.Instance.eventUIHolder.SetActive(true);
        UIManager.Instance.eventButton1Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);
    }

    protected override void UpdateEventText()
    {
        string eventText = "Although the atmosphere carries a constant thickness to it, it's uneasy to feel it in your very step. " +
            "But it's not slugishness keeping you down, some black ichorus seems to be under your boots finding yourself in a puddle of this as you recoil from its smell of death.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateSuccessEventText()
    {
        string eventText = "With a few quick steps and some careful movement you leave its strange enveloping grasp.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateFailEventText()
    {
        string eventText = "Its warmth encapsulates you while seeping in, initially a nuisance but now slowly enveloping your feet then up" +
            " the legs. Pain emerges as if being injected with venom by thousands of needles, gasping for air away form the torched smell of flesh you release outwards from its grasp.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void UpdateButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Escape!";
    }

    private void UpdateEndButtonText(GameObject eventButtonObejct, string buttonText)
    {
        eventButtonObejct.GetComponentInChildren<Text>().text = buttonText;
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
    }

    public override void UpdateEventButtonListener()
    {
        if(Random.value <= SUCCESSPROB)
        {
            escaped = true;
            fearGained = Random.Range(2, 4);
        }
        else
        {
            fearGained = Random.Range(4, 18);
            horrorGained = Random.Range(1, 3);
        }
        ContinueEvent();
    }

    private void UpdateEndFailEventButtonListener()
    {
        playerScript.AddHorror(horrorGained);
        EndEvent();
    }
}

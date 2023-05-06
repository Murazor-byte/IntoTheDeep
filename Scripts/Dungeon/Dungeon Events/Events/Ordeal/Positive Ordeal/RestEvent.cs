using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestEvent : PositiveOrdeal
{
    private int fearLost;
    private int healthGained;
    private bool hasCampFire;
    private bool usedCamp;

    public RestEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        Debug.Log("Setting up Rest Event");

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object);
        UpdateEventButton();

        for (int i = 0; i < playerScript.inventory.inventorySlots.Count; i++)
        {
            if (playerScript.inventory.inventory[i] is Camp) hasCampFire = true;
        }
        if (!hasCampFire) UIManager.Instance.eventButton1.interactable = false;
    }

    private void ContinueEvent()
    {
        UIManager.Instance.eventButton2Object.SetActive(false);
        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);
    }

    //player used campfire
    private void UpdateCampEventText()
    {
        string eventText = "The crackling of fire and the warmth of the flames are a gentle reminder to relax your thoughts. Feeling well rested, you gather your thoughts and supplies and are reinvigorated for a new journey.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    //player didn't use campfire
    private void UpdateRestEventText()
    {
        string eventText = "Amid the dampness of your sourroundings, you find solace in the comfort of your thoughts. A rally is in order to head forth once more.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void SetUIActive()
    {
        UIManager.Instance.eventUIHolder.SetActive(true);
        UIManager.Instance.eventButton1Object.SetActive(true);
        UIManager.Instance.eventButton2Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(-20f, -30f, 0f);
        UIManager.Instance.eventButton2Object.GetComponent<RectTransform>().localPosition = new Vector3(20f, -30f, 0f);
    }

    protected override void UpdateEventText()
    {
        string eventText = "The sound of silence overcomes you in this restless place. The presence of danger feels farfetched and the comfort of rest well needed. Unpacking your supplies you make camp.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void UpdateButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Make Camp";
        UIManager.Instance.eventButton2.GetComponentInChildren<Text>().text = "Rest";
    }

    private void UpdateEndButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Lose " + fearLost + " fear";
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, UpdateEventButton2Listener, true);
    }

    //use camp
    public override void UpdateEventButtonListener()
    {
        usedCamp = true;
        fearLost = Random.Range(10, 25);
        healthGained = playerScript.healthCap;
        ContinueEvent();
        UpdateCampEventText();
        UpdateEndButtonText(UIManager.Instance.eventButton1Object);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEndEventButtonListener, true);
    }

    //don't use camp
    private void UpdateEventButton2Listener()
    {
        fearLost = Random.Range(5, 15);
        healthGained = Random.Range(5, (int)(playerScript.healthCap * .5f));
        ContinueEvent();
        UpdateRestEventText();
        UpdateEndButtonText(UIManager.Instance.eventButton1Object);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEndEventButtonListener, true);
        UIManager.Instance.eventButton1.interactable = true;
    }

    private void UpdateEndEventButtonListener()
    {
        if (usedCamp)
        {
            for (int i = 0; i < playerScript.inventory.inventorySlots.Count; i++)
            {
                if (playerScript.inventory.inventory[i] is Camp) playerScript.inventory.inventory[i].UseItem();
            }
        }

        playerScript.AddFear(-fearLost);
        playerScript.Heal(healthGained);
        EndEvent();
    }
}

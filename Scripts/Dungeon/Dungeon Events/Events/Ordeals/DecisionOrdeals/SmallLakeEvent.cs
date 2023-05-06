using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmallLakeEvent : DecisionOrdeal
{
    private int fearLost;
    private int healthRecovered;
    private bool caught;
    private int numberRations;

    public SmallLakeEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        decisionProb = 0.55f;

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Fish");
        UpdateButtonText(UIManager.Instance.eventButton2Object, "Rest");
        UpdateEventButton();
    }

    private void ContinueEvent()
    {
        UIManager.Instance.eventButton2Object.SetActive(false);
        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);

        if (caught)
        {
            fearLost = Random.Range(15, 25);
            healthRecovered = Random.Range(9, 15);
            numberRations = Random.Range(1, 3);

            UpdateSuccessText();
            if(numberRations == 1) UpdateButtonText(UIManager.Instance.eventButton1Object, "Gain " + numberRations + " Ration");
            else UpdateButtonText(UIManager.Instance.eventButton1Object, "Gain " + numberRations + " Rations");
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, Catch, true);
        }
        else
        {
            fearLost = Random.Range(3, 5);
            healthRecovered = Random.Range(2, 5);

            UpdateFailText();
            UpdateButtonText(UIManager.Instance.eventButton1Object, "Recover " + healthRecovered + " Health");
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, DontCatch, true);
        }
    }

    protected override void UpdateEventText()
    {
        string eventText = "Amid the constant cramped wall interior, the formations open to a shimmer of water. It seems" +
            " a steady flow rests here in the form of a small lake under these caverns. A nice respite to regather your strength " +
            "or catch what remains.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateSuccessText()
    {
        string eventText = "With the tools available, you fasten a net for some quick game and in luck snag a creature that found its way " +
            "into this lake. Although not much to eat, a small victory is well deserved.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateFailText()
    {
        string eventText = "With food at the forethought, a net is quickly assembled from your gear, in hopes for a meal to return. As time passes you determine what remains in the lake will be left unstirred. " +
            "You pack your wet tools and reflect on what's to come.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateButtonText(GameObject eventButtonObject, string buttonText)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = buttonText;
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, Rest, true);
    }

    public override void UpdateEventButtonListener()
    {
        if (Random.value <= decisionProb) caught = true;
        ContinueEvent();
    }

    private void Rest()
    {
        fearLost = Random.Range(10, 15);
        healthRecovered = Random.Range(7, 14);
        playerScript.AddFear(-fearLost);
        playerScript.Heal(healthRecovered);
        EndEvent();
    }

    private void Catch()
    {
        for(int i = 0; i < numberRations; i++)
        {
            playerScript.inventory.AddItem(new Ration(), playerScript);
        }
        playerScript.AddFear(-fearLost);
        playerScript.Heal(healthRecovered);
        EndEvent();
    }

    private void DontCatch()
    {
        playerScript.AddFear(-fearLost);
        playerScript.Heal(healthRecovered);
        EndEvent();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeadTownsfolkEvent : DecisionOrdeal
{
    private bool foundLoot;
    private int fearGained;
    private Item itemToGive;
    private int itemAmount;

    public DeadTownsfolkEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        decisionProb = 0.5f;

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Scavange");
        UpdateButtonText(UIManager.Instance.eventButton2Object, "Move On");
        UpdateEventButton();
    }

    private void ContinueEvent()
    {
        playerScript.AddFear(fearGained);

        if (foundLoot)
        {
            UpdateSuccessEventText();

            int itemToGiveIndex = Random.Range(0, GainItemEvent.itemSelection.Count - 1);
            itemToGive = GainItemEvent.itemSelection[itemToGiveIndex];
            itemAmount = itemToGive.ItemAmount();

            if (itemAmount == 1) UpdateButtonText(UIManager.Instance.eventButton1Object, "Pick up a" + itemToGive.GetType());
            else UpdateButtonText(UIManager.Instance.eventButton1Object, "Pick up " + itemAmount + " " + itemToGive.GetType() + "s");
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateSuccessButtonListener, true);

            UpdateButtonText(UIManager.Instance.eventButton2Object, "Move On");
            UIManager.Instance.AddListener(UIManager.Instance.eventButton2, EndEvent, true);
        }
        else
        {
            UIManager.Instance.eventButton2Object.SetActive(false);
            UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);

            UpdateFailEventText();
            UpdateButtonText(UIManager.Instance.eventButton1Object, "Move On");
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, EndEvent, true);
        }
    }

    protected override void UpdateEventText()
    {
        string eventText = "A corpse lies thrown on the floor ahead, holding wounds unkown to you. Although something to grow accustomed to, " +
            "the decomposition has well taken its course where its stench rightfully follows." + " Although there may be of some use to their belongings.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateSuccessEventText()
    {
        string eventText = "Rummaging through the torn pouches and sundered clothing you come across an item of some value";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateFailEventText()
    {
        string eventText = "Searching throughout the corpse, identifying anything of worth, you come up empty. Hopeless in this endeavor. Just one more thing to tally on.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public void UpdateButtonText(GameObject eventButtonObject, string buttonText)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = buttonText;
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, EndEvent, true);
    }

    public override void UpdateEventButtonListener()
    {
        if (Random.value <= decisionProb)
        {
            foundLoot = true;
            fearGained = Random.Range(2, 5);
        }
        else
        {
            fearGained = Random.Range(4, 10);
        }
        ContinueEvent();
    }

    private void UpdateSuccessButtonListener()
    {
        for(int i = 0; i < itemAmount; i++)
        {
            playerScript.inventory.AddItem(itemToGive, playerScript);
        }
        EndEvent();
    }
}

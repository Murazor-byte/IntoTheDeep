using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//player can attempt to loot dead horror, which they can find loot
    //or it's alive and combat is started
public class LootHorrorEvent : DecisionOrdeal
{
    private bool combat;
    private int fearGained;
    private Item itemToGive;
    private int itemAmount;

    public LootHorrorEvent(GameObject player, ScenesManager sceneManager) : base (player, sceneManager) { }

    public override void SetUpEvent()
    {
        decisionProb = 0.7f;    //prob of finding loot over combat

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Loot");
        UpdateButtonText(UIManager.Instance.eventButton2Object, "Move On");
        UpdateEventButton();
    }

    private void ContinueEvent()
    {      
        if (combat)
        {
            UIManager.Instance.eventButton2Object.SetActive(false);
            UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);

            fearGained = Random.Range(1, 4);
            UpdateCombatEventText();
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, StartCombat, true);
            UpdateButtonText(UIManager.Instance.eventButton1Object, "Fight!");
        }
        else
        {
            fearGained = Random.Range(1, 4);

            int itemToGiveIndex = Random.Range(0, GainItemEvent.itemSelection.Count - 1);
            itemToGive = GainItemEvent.itemSelection[itemToGiveIndex];
            itemAmount = itemToGive.ItemAmount();

            UpdateLootEventText();
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, Loot, true);
            if (itemAmount == 1) UpdateButtonText(UIManager.Instance.eventButton1Object, "Pick up a" + itemToGive.GetType());
            else UpdateButtonText(UIManager.Instance.eventButton1Object, "Pick up " + itemAmount + " " + itemToGive.GetType() + "s");

            UIManager.Instance.AddListener(UIManager.Instance.eventButton2, EndEvent, true);
            UpdateButtonText(UIManager.Instance.eventButton2Object, "Move On");
        }
    }

    protected override void UpdateEventText()
    {
        string eventText = "With death filling the interior of these caverns, this place is no different. The scattered bodies " +
            "of horrors rest here, doubtful of what may have brought them. Although fortune may be gained from what's left on their remains.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateCombatEventText()
    {
        string eventText = "The plundering of small trinkets is quickly stiffened as their stillness turns to movement. The misjudgement of open welath " +
            "turns to a rapid action for survival.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateLootEventText()
    {
        string eventText = "Not much is to be gained from what these creatures may hold, but some valuables can be scavanged.";
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
        if (Random.value > decisionProb) combat = true;

        ContinueEvent();
    }

    private void StartCombat()
    {
        playerScript.AddFear(fearGained);
        playerDungeonMovement.inEvent = false;

        SetUIInactive();
        List<EventType> currentEvent;

        if (playerDungeonMovement.inCorridor)
        {
            currentEvent = playerDungeonMovement.currentCorridor.possibleEvents[playerDungeonMovement.currentCorridorTile];
        }
        else
        {
            currentEvent = playerDungeonMovement.currentRoom.possibleEvents;
        }

        int indexOfEvent = 0;
        for(int i = 0; i < currentEvent.Count; i++)
        {
            if (currentEvent[i] == EventType.LootHorror) indexOfEvent = i;
        }

        playerDungeonMovement.ChangeToCombatScene(ref currentEvent, indexOfEvent);
    }

    private void Loot()
    {
        playerScript.AddFear(-fearGained);

        for(int i = 0; i < itemAmount; i++)
        {
            playerScript.inventory.AddItem(itemToGive, playerScript);
        }
        EndEvent();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvadeDangerEvent : DecisionOrdeal
{

    public EvadeDangerEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { }

    public override void SetUpEvent()
    {
        decisionProb = 0.5f;

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object);
        UpdateButton2Text(UIManager.Instance.eventButton2Object);
        UpdateEventButton();
    }

    private void ContinueEvent()
    {
        UIManager.Instance.eventButton2Object.SetActive(false);
        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);

        if (succeeded)
        {
            UpdateSuccessEventText();
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateSuccessEventButton, true);
            UpdateSuccessButtonText(UIManager.Instance.eventButton1Object);
        }
        else
        {
            UpdateFailEventText();
            UIManager.Instance.AddListener(UIManager.Instance.eventButton1, StartCombat, true);
            UpdateButtonText(UIManager.Instance.eventButton1Object);
        }
    }

    protected override void UpdateEventText()
    {
        string eventText = "The quite shuffling of movement followed by a distant shrill scream, certain it's getting louder. With time and the sounds reaching closer you make a decision.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateSuccessEventText()
    {
        string eventText = "With haste and a careful scan of the environment, you reposition yourself into the deepest shadows, listening as the noises become gradually softer. An encounter nearly avoided.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateFailEventText()
    {
        string eventText = "The screams become louder as you attempt to locate a passage to evade the horrors. But too soon they come upon you, finding yourself surrounded.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void UpdateButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "FIGHT";
    }

    private void UpdateButton2Text(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Evade Danger";
    }

    private void UpdateSuccessButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Move on";
    }

    //update both the event button listeners
    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, StartCombat, true);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, UpdateEventButtonListener, true);
    }

    private void UpdateSuccessEventButton()
    {
        EndEvent();
    }

    //Rolling to evade combat
    public override void UpdateEventButtonListener()
    {
        //success
        if(Random.value <= decisionProb)
        {
            succeeded = true;
        }
        ContinueEvent();
    }

    //starts combat from this event referencing the player dungeon movement script to load combat
    private void StartCombat()
    {
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
            if (currentEvent[i] == EventType.EvadeDanger) indexOfEvent = i;
        }

        playerDungeonMovement.ChangeToCombatScene(ref currentEvent, indexOfEvent);
    }
}

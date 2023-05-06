using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Ordeal : Event
{
    protected GameObject player;
    protected Player playerScript;
    protected PlayerDungeonMovement playerDungeonMovement;
    protected ScenesManager sceneManager;

    protected string eventText;        //the string to pass as the new event text

    protected Ordeal(GameObject player, ScenesManager sceneManager)
    {
        this.player = player;
        //playerDungeonMovement = player.GetComponent<PlayerDungeonMovement>();
        playerDungeonMovement = sceneManager.GetDungeonPlayer().GetComponent<PlayerDungeonMovement>();
        this.sceneManager = sceneManager;

        GetPlayerScript();
    }

    protected abstract void UpdateEventText();
    protected abstract void UpdateEventButton();
    public abstract void UpdateButtonText(GameObject eventButtonObject);    //public for choicedOrdeal to update button text
    public abstract void UpdateEventButtonListener();                       //is public for choicedOrdeal to update its buttons

    //if it's not a choiced event, display two buttons,
    //one to pick up the item and the other to leave it
    public virtual void SetUIActive() { }

    //accesses the most recent player stats
    protected void GetPlayerScript()
    {
        playerScript = player.GetComponent<Player>();
    }

    protected virtual void EndEvent()
    {
        Debug.Log("Ending Event");
        SetUIInactive();
        DestoryEventFromDungeon();
        UnblockPlayerMovement();
    }

    //allow player to move once button has been clicked
    protected void UnblockPlayerMovement()
    {
        playerDungeonMovement.inEvent = false;
    }

    protected virtual void SetUIInactive()
    {
        UIManager.Instance.eventUIHolder.SetActive(false);
        UIManager.Instance.DeactivateAllButton();
        //UIManager.Instance.EnableDungeonRetreat();
        UIManager.Instance.SetDungeonInteractivePlayerUI(true);
    }

    protected void DestoryEventFromDungeon()
    {
        sceneManager.RemoveDungeonTileEvent();
    }

}

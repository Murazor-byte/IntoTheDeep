using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenesManager : MonoBehaviour
{
    [SerializeField] private GameObject dungeonMainCamera;
    [SerializeField] private GameObject boardHolder;
    [SerializeField] private GameObject lightSource;
    [SerializeField] private GameObject dungeonPlayer;
    [SerializeField] private GameObject combatPauseMenu;

    public List<Event.EventType> currentTileEvents;             //accessing the current combat event that initiated this combat
    public int indexOfEvent;                                    //the index associated with that current combat Event Tile

    private DungeonManager dungeonManager;

    private void Start()
    {
        dungeonManager = GameObject.Find("DungeonManager").GetComponent<DungeonManager>();
    }


    public void SetDungeonMainCamera(GameObject dungeonMainCamera)
    {
        this.dungeonMainCamera = dungeonMainCamera;
    }

    public void SetBoardHolder(GameObject boardHolder)
    {
        this.boardHolder = boardHolder;
    }

    public void SetLightSource(GameObject lightSource)
    {
        this.lightSource = lightSource;
    }

    public void SetCombatPauseMenu(GameObject combatPauseMenu)
    {
        this.combatPauseMenu = combatPauseMenu;
    }

    //sets the dungeon player and reflects player stats to UI
    public void SetDungeonPlayer(GameObject dungeonPlayer)
    {
        this.dungeonPlayer = dungeonPlayer;
    }

    public GameObject GetDungeonMainCamera()
    {
        return dungeonMainCamera;
    }

    public GameObject GetBoardHolder()
    {
        return boardHolder;
    }

    public GameObject GetLightSource()
    {
        return lightSource;
    }

    public GameObject GetDungeonPlayer()
    {
        return dungeonPlayer;
    }

    public GameObject GetCombatPuaseMenu()
    {
        return combatPauseMenu;
    }

    //references the room/corridors events
    public void SetDungeonTileEvent(ref List<Event.EventType> currentTileEvents, ref int indexOfEvent)
    {
        this.currentTileEvents = currentTileEvents;
        this.indexOfEvent = indexOfEvent;
    }

    //removes the current room/corridor event at 'indexEvent'
    public void RemoveDungeonTileEvent()
    {
        currentTileEvents.RemoveAt(indexOfEvent);
        Debug.Log("Event has been removed from the dungeon");
        UIManager.Instance.DeactivateAllButton();
        dungeonManager.CheckExitCondition();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDungeonMovement : MonoBehaviour
{
    public bool gamePaused;
    public bool inTown = true;

    private int playerXPos;
    private int playerYPos;

    private Room[] rooms;
    private bool[] roomsVisited;                                //tracks which rooms in the dungeon have been explored
    private int roomsExplored;                                  //how many rooms have many explored
    public Room currentRoom { get; private set; }
    public Corridor currentCorridor { get; private set; }
    public int currentCorridorTile { get; private set; }
    private List<Corridor> allCorridors;
    public bool inCorridor { get; private set; }    
    private bool inRoom = true;
    public bool inEvent = false;                                //tracks if the player is deciding an event
    public int moves;                                           //number of moves made in the dungeon
    public int movesSinceCombat { get; private set; }           //unique moves made without triggerring a comabt encounter

    private bool[][] occupiedTiles;                             //keep track of floor tiles
    private DungeonGenerator.FloorType[][] floorType;           //keep track if each occupied tile is a corridor or room
    private int cameraOffset = -25;

    private ScenesManager sceneManager;
    private EventManager eventManager;
    private GameManager gameManager;

    //[SerializeField] private Player player;
    private UIPlayerStats UIPlayerStats;

    private GameObject townPauseMenu;
    private Button townExitMenu;

    private GameObject dungeonPauseMenu;
    private Button dungeonExitMenu;

    private GameObject combatPauseMenu;

    //sets up the player from within the DungeonManager script
    public void TheStart()
    {
        ResetRoomsExplored();
        currentRoom = rooms[0];
        CheckAllRoomsExplored(0);
        playerXPos = rooms[0].xPos + 2;
        playerYPos = rooms[0].yPos + 2;
        transform.position = new Vector3(playerXPos, playerYPos, -0.5f);
        Camera.main.transform.position = new Vector3(playerXPos, playerYPos, cameraOffset);

        sceneManager = GameObject.Find("SceneManager").GetComponent<ScenesManager>();
        eventManager = GameObject.Find("Event Manager").GetComponent<EventManager>();
        UIPlayerStats = GameObject.Find("UIPlayerStats").GetComponent<UIPlayerStats>();

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Player able to move");
        if (Input.GetKeyDown(KeyCode.Escape) && !gamePaused)
        {
            gamePaused = true;
            if (inTown)
            {
                townPauseMenu.SetActive(true);
                gameManager.DeactivateAllTownEntrances();
            }
            else
            {
                dungeonPauseMenu.SetActive(true);
                UIManager.Instance.SetDungeonInteractivePlayerUI(false);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && gamePaused)
        {
            gamePaused = false;
            if (inTown)
            {
                townPauseMenu.SetActive(false);
                gameManager.ReactivateAllTownEntrances();
            }
            else
            {
                dungeonPauseMenu.SetActive(false);
                UIManager.Instance.SetDungeonInteractivePlayerUI(true);
            }
        }
        else if (!gamePaused && !inTown && !inEvent)
        {
            if (inCorridor)
            {
                PlayerCorridorMovement();
            }
            else if (inRoom)
            {
                PlayerRoomMovement();
            }
        }
        
    }

    //Sets up the Pause Menus
    public void SetUpPauseMenu(GameManager gameManager)
    {
        inTown = true;
        this.gameManager = gameManager;
        townPauseMenu = GameObject.Find("Pause Menu");
        townExitMenu = GameObject.Find("Exit Puase Menu").GetComponent<Button>();
        townExitMenu.onClick.AddListener(SetUpTownExitMenuListener);
        townPauseMenu.SetActive(false);

        dungeonPauseMenu = GameObject.Find("Dungeon Pause Menu");
        dungeonExitMenu = GameObject.Find("Exit Dungeon Menu").GetComponent<Button>();
        dungeonExitMenu.onClick.AddListener(SetUpDungeonExitMenuListener);
        dungeonPauseMenu.SetActive(false);

        combatPauseMenu = GameObject.Find("Combat Pause Menu");
        combatPauseMenu.SetActive(false);
    }

    private void SetUpTownExitMenuListener()
    {
        gamePaused = false;
        townPauseMenu.SetActive(false);
        gameManager.ReactivateAllTownEntrances();
    }

    private void SetUpDungeonExitMenuListener()
    {
        gamePaused = false;
        dungeonPauseMenu.SetActive(false);
        UIManager.Instance.SetDungeonInteractivePlayerUI(true);
    }

    public void SetUp(Room[] rooms, List<Corridor> allCorridors, bool[][] occupiedTiles, DungeonGenerator.FloorType[][] floorType)
    {
        this.rooms = rooms;
        roomsVisited = new bool[rooms.Length];
        this.allCorridors = allCorridors;
        this.occupiedTiles = occupiedTiles;
        this.floorType = floorType;
        currentRoom = rooms[0];
    }

    //if the player moves out of the room and into a corridor
    //Player can only move into a corridor from a room
    private void PlayerRoomMovement()
    {
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && currentRoom.corridorNorth)
        {
            SetCorridorValue();
            playerYPos += 3;
            SetTransform();
            FindCurrentCorridor();
        }
        else if((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && currentRoom.corridorEast)
        {
            SetCorridorValue();
            playerXPos += 3;
            SetTransform();
            FindCurrentCorridor();
        }
        else if((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && currentRoom.corridorSouth)
        {
            SetCorridorValue();
            playerYPos -= 3;
            SetTransform();
            FindCurrentCorridor();
        }
        else if((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && currentRoom.corridorWest)
        {
            SetCorridorValue();
            playerXPos -= 3;
            SetTransform();
            FindCurrentCorridor();
        }
    }

    //if the player moves through a corridor or into a room
    //player can either keep moving through the same corridor
    //or can move to the center of a room if there is a room in that inputted direction
    private void PlayerCorridorMovement()
    {
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && occupiedTiles[playerXPos][playerYPos + 1] == true)
        {
            //test if this is a floorType.Corridor
            if(floorType[playerXPos][playerYPos + 1] == DungeonGenerator.FloorType.CorridorTile)
            {
                SetCorridorValue();
                playerYPos += 1;
                SetTransform();
                FindCurrentCorridorTile();
            }
            //test if this is a floorType.Room
            else if (floorType[playerXPos][playerYPos + 1] == DungeonGenerator.FloorType.RoomTile)
            {
                SetRoomValue();
                playerYPos += 3;
                SetTransform();
                FindCurrentRoom();
            }
        }
        else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && occupiedTiles[playerXPos + 1][playerYPos] == true)
        {
            //test if this is a floorType.Corridor
            if (floorType[playerXPos + 1][playerYPos] == DungeonGenerator.FloorType.CorridorTile)
            {
                SetCorridorValue();
                playerXPos += 1;
                SetTransform();
                FindCurrentCorridorTile();
            }
            //test if this is a floorType.Room
            else if (floorType[playerXPos + 1][playerYPos] == DungeonGenerator.FloorType.RoomTile)
            {
                SetRoomValue();
                playerXPos += 3;
                SetTransform();
                FindCurrentRoom();
            }
        }
        else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && occupiedTiles[playerXPos][playerYPos - 1] == true)
        {
            //test if this is a floorType.Corridor
            if (floorType[playerXPos][playerYPos - 1] == DungeonGenerator.FloorType.CorridorTile)
            {
                SetCorridorValue();
                playerYPos -= 1;
                SetTransform();
                FindCurrentCorridorTile();
            }
            //test if this is a floorType.Room
            else if (floorType[playerXPos][playerYPos - 1] == DungeonGenerator.FloorType.RoomTile)
            {
                SetRoomValue();
                playerYPos -= 3;
                SetTransform();
                FindCurrentRoom();
            }
        }
        else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && occupiedTiles[playerXPos - 1][playerYPos] == true)
        {
            //test if this is a floorType.Corridor
            if (floorType[playerXPos - 1][playerYPos] == DungeonGenerator.FloorType.CorridorTile)
            {
                SetCorridorValue();
                playerXPos -= 1;
                SetTransform();
                FindCurrentCorridorTile();
            }
            //test if this is a floorType.Room
            else if (floorType[playerXPos - 1][playerYPos] == DungeonGenerator.FloorType.RoomTile)
            {
                SetRoomValue();
                playerXPos -= 3;
                SetTransform();
                FindCurrentRoom();
            }
        }
    }

    //sets the player to be at the moved unit and increases number moves
    private void SetTransform()
    {
        transform.position = new Vector3(playerXPos, playerYPos, -0.5f);
        moves++;
    }

    //finds the currentRoom we are currently in and sets currentRoom that found rooms[i] room
    private void FindCurrentRoom()
    {
        for(int i = 0; i < rooms.Length; i++)
        {
            if(rooms[i] != null)
            {
                if ((rooms[i].xPos + 2) == playerXPos && (rooms[i].yPos + 2) == playerYPos)
                {
                    currentRoom = rooms[i];

                    if (!currentRoom.discovered)
                    {
                        currentRoom.discovered = true;
                        movesSinceCombat++;
                    }

                    CheckAllRoomsExplored(i);
                    CheckForRoomEvent();
                }
            }            
        }
    }

    //finds the currentCorridor the player is currently in by matching start&end positions
    //sets global currentCorridor to that found corridors[i] corridor
    private void FindCurrentCorridor()
    {
        for(int i = 0; i < allCorridors.Count; i++)
        {
            //match player Pos with start of the corridor
            if(playerXPos == allCorridors[i].corridorXUnits[0] && playerYPos == allCorridors[i].corridorYUnits[0])
            {
                currentCorridor = allCorridors[i];
                FindCurrentCorridorTile();
            }
            //match player pos with end of the corridor
            else if(playerXPos == allCorridors[i].corridorXUnits[allCorridors[i].corridorLength-1] && playerYPos == allCorridors[i].corridorYUnits[allCorridors[i].corridorLength - 1])
            {
                currentCorridor = allCorridors[i];
                FindCurrentCorridorTile();
            }

        }
    }

    //finds what corridor tile the player is on within the currentCorridor
    //and calling for checkForCorridorEvent
    private void FindCurrentCorridorTile()
    {
        for(int i = 0; i < currentCorridor.corridorLength; i++)
        {
            if(playerXPos == currentCorridor.corridorXUnits[i] && playerYPos == currentCorridor.corridorYUnits[i])
            {
                //check if this corridor tile has already been discored or not
                if (!currentCorridor.discovered[i])
                {
                    currentCorridor.discovered[i] = true;
                    movesSinceCombat++;
                }

                currentCorridorTile = i;
                CheckForCorridorEvent();
            }
        }
    }

    //sets the player to be in a corridor to true, room to false
    private void SetCorridorValue()
    {
        inRoom = false;
        inCorridor = true;
    }

    //sets the player to be in a room to true, corridor to false
    private void SetRoomValue()
    {
        inRoom = true;
        inCorridor = false;
    }

    //Checks if there is an event in this current room
    private void CheckForRoomEvent()
    {
        for(int i = 0; i < currentRoom.possibleEvents.Count; i++)
        {
            Event.EventType eventType = currentRoom.possibleEvents[i];
            if (eventType == Event.EventType.GruntCombat || eventType == Event.EventType.MiniBoss || eventType == Event.EventType.Boss)
            {
                ChangeToCombatScene(ref currentRoom.possibleEvents, i);
            }
            else
            {
                inEvent = true;

                sceneManager.SetDungeonTileEvent(ref currentRoom.possibleEvents, ref i);
                //if it's not a combat event, then set up the event screen
                currentRoom.SetUpEvent(i, eventManager, sceneManager);
            }
        }
    }

    //Checks if there is an event in this current corridor tile, while the player is already in the corridor
    //check what type of event it is
    private void CheckForCorridorEvent()
    {
        //create a List<Event> variable that holds a list of events for what is at this tile within the currentCorridor
        List<Event.EventType> currentEvent = currentCorridor.possibleEvents[currentCorridorTile];

        for(int i = 0; i< currentEvent.Count; i++)
        {
            if (currentEvent[i] == Event.EventType.GruntCombat || currentEvent[i] == Event.EventType.MiniBoss || currentEvent[i] == Event.EventType.Boss)
            {
                ChangeToCombatScene(ref currentEvent, i);
            }
            else
            {
                inEvent = true;

                sceneManager.SetDungeonTileEvent(ref currentEvent, ref i);
                //if it's not a combat event, then set up the event screen
                currentCorridor.SetUpEvent(currentCorridorTile, i, eventManager, sceneManager);
            }
        }
    }

    //changes from dungeon scene to combat scene
    public void ChangeToCombatScene(ref List<Event.EventType> eventsInTile, int indexOfEvent)
    {
        sceneManager.SetDungeonTileEvent(ref eventsInTile, ref indexOfEvent);
        //set the Combat Pause Menu for the combat player to use
        sceneManager.SetCombatPauseMenu(combatPauseMenu);

        //untick all the gameobjects from the first scene so they are not viewable in transition scene
        sceneManager.GetBoardHolder().SetActive(false);
        sceneManager.GetLightSource().SetActive(false);

        //load the new combat scene once reached a combat event
        SceneManager.LoadScene("Combat", LoadSceneMode.Additive);
        sceneManager.GetDungeonMainCamera().SetActive(false);

        UIManager.Instance.DeactivateDungeonRetreat();
        UIManager.Instance.ActivateCombatRetreat();
    }

    //checks if all rooms in the dungeon have been explored, returning to town if so
    private void CheckAllRoomsExplored(int roomIndex)
    {
        if (roomsVisited[roomIndex] == false) roomsExplored++;
        roomsVisited[roomIndex] = true;

        if (roomsExplored == rooms.Length && currentRoom.possibleEvents.Count == 0)
        {
            Debug.Log("ALL ROOMS HAVE BEEN EXPLORED WITH LAST ROOM NOT HAVING AN EVENT");
            GameObject.Find("DungeonManager").GetComponent<DungeonManager>().ExitDungeon(Dungeon.Run.Successful, false);
        }
    }

    public bool CheckExploredRoomsExitCondition()
    {
        if(roomsExplored >= rooms.Length)
        {
            return true;
        }
        return false;
    }

    private void ResetRoomsExplored()
    {
        roomsExplored = 0;
    }

    //once the player re-enters a new dungeon, reset them to start in a room (NOT A CORRIDOR)
    public void ResetRoomPlacement()
    {
        inCorridor = false;
        inRoom = true;
    }

    public void ResetMovesSinceCombat()
    {
        movesSinceCombat = 0;
    }
}

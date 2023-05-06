using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManager : MonoBehaviour
{
    public int rows;
    public int columns;
    public int minNumberRooms;
    public int maxNumberRooms;
    public GameObject floorTiles;
    public GameObject wallTiles;
    private DungeonGenerator dungeonGenerator;
    private GameObject eventManager;
    private GameManager gameManager;

    private ScenesManager sceneManager;             //references to set up scenemanager
    private GameObject dungeonMainCamera;
    public GameObject boardHolder;
    public GameObject lightSource;
    public GameObject dungeonPlayer;
    private Player dungeonPlayerStats;

    public bool playerDeath = false;                //if the player dropped to 0 within the dungeon (don't check for exit condition, since it's already set)
    private readonly int numberEnemiesToKill = 7;
    private bool bossInLayer = false;               //if the boss can spawn in this dungeon layer

    //creates a new dungeon given the new parameters from GameManager when player enters from the Town
    public void CreateDungeon(int rows, int columns, int minNumberRooms, int maxNumberRooms, GameObject dungeonPlayer, bool bossInLayer)
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        this.rows = rows;
        this.columns = columns;
        this.minNumberRooms = minNumberRooms;
        this.maxNumberRooms = maxNumberRooms;
        this.dungeonPlayer = dungeonPlayer;
        this.bossInLayer = bossInLayer;

        dungeonGenerator = new DungeonGenerator(rows, columns, minNumberRooms, maxNumberRooms, wallTiles, floorTiles);
        dungeonGenerator.Start();

        SetUpEventManager();
        SetUpSceneManager();

        SetUpPlayer();
    }

    //Initializes playerMovement variables with dungeonGenerator variables
    private void SetUpPlayer()
    {
        sceneManager.GetDungeonPlayer().GetComponent<PlayerDungeonMovement>().SetUp(dungeonGenerator.rooms, dungeonGenerator.allCorridors, dungeonGenerator.occupiedTiles, dungeonGenerator.floorType);
        sceneManager.GetDungeonPlayer().GetComponent<PlayerDungeonMovement>().TheStart();
        dungeonPlayer.GetComponent<PlayerDungeonMovement>().inTown = false;
    }

    //Initializes the EventManager variables with dungeonGenerator variables
    private void SetUpEventManager()
    {
        eventManager = GameObject.Find("Event Manager");
        eventManager.GetComponent<EventManager>().SetUp(dungeonGenerator.rooms, dungeonGenerator.allCorridors, dungeonGenerator.occupiedTiles, dungeonGenerator.floorType, bossInLayer);
        eventManager.GetComponent<EventManager>().TheStart();
    }

    private void SetUpSceneManager()
    {
        sceneManager = GameObject.Find("SceneManager").GetComponent<ScenesManager>();
        dungeonMainCamera = GameObject.Find("Main Camera");
        boardHolder = GameObject.Find("BoardHolder");
        lightSource = GameObject.Find("Directional Light");
        dungeonPlayer = GameObject.Find("Player");
        dungeonPlayerStats = dungeonPlayer.GetComponent<Player>();

        sceneManager.SetDungeonMainCamera(dungeonMainCamera);
        sceneManager.SetBoardHolder(boardHolder);
        sceneManager.SetLightSource(lightSource);
        sceneManager.SetDungeonPlayer(dungeonPlayer);
    }

    //checks if the player has met the Dungeon Exit condition
    public void CheckExitCondition()
    {
        if (playerDeath) return;
        Debug.Log("Checking Exit Condition");

        if(dungeonPlayerStats.enemiesKilled >= numberEnemiesToKill)
        {
            Debug.Log("Player has met the Dungeon Exit Condition, Leaving Dungeon...");
            ExitDungeon(Dungeon.Run.Successful, false);
        }
        //if player has explored every room in the dungeon
        else if (dungeonPlayer.GetComponent<PlayerDungeonMovement>().CheckExploredRoomsExitCondition())
        {
            Debug.Log("Explored every room, leaving dungoen");
            ExitDungeon(Dungeon.Run.Successful, false);
        }
    }

    public void ExitDungeon(Dungeon.Run run, bool playerDeath)
    {
        dungeonMainCamera.transform.parent = null;          //set the dungeon camera to heirarchy and back to Dungeon Scene
        SceneManager.MoveGameObjectToScene(dungeonMainCamera, SceneManager.GetSceneByName("Dungeon"));

        Destroy(boardHolder);                               //destroy created dungeon gameObjects
        Destroy(dungeonGenerator.instantiateHolder);

        gameManager.DepleteLayers();                        //deplete the layer the Hero was on
        SceneManager.UnloadSceneAsync("Dungeon");
        gameManager.ReloadTown(run, playerDeath);
    }

}

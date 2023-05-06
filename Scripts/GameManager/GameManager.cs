using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

//General Game Manager script between Dungeon runs and the town
//MIGHT WANT TO CHANGE THIS TO "TOWN MANAGER" INSTEAD AS IT REALLY ONLY WORKS WITH THE TOWN AND NOT THE ENTIRE GAME
public class GameManager : MonoBehaviour
{
    private GameObject playerObject;
    private Player player;
    private PlayerDungeonMovement playerMovement;
    private Dungeon dungeon;
    private DungeonMeter dungeonMeter;
    private GameObject dungeonMeterUI;

    private GameObject mainCamera;      //town camera game object
    private Camera townCamera;          //town camera
    private GameObject lighting;
    private GameObject townUI;
    private Canvas townUICanvas;
    private GameObject dungeonEntrance;
    private Button enterDungeonButton;
    public GameObject pauseMenu;
    private GameObject dungeonPauseMenu;

    private GameObject innEntrance;
    private Button innButton;
    private GameObject innBackground;
    private GameObject exitInn;
    private ShopInventory innSelection;

    private GameObject weaponsmithEntrance;
    private Button weaponsmithButton;
    private GameObject weaponsmithBackground;
    private GameObject exitWeaponsmith;
    private ShopInventory weaponsmithInventory;

    private GameObject armorsmithEntrance;
    private Button armorsmithButton;
    private GameObject armorsmithBackground;
    private GameObject exitArmorsmith;
    private ShopInventory armorsmithShopInventory;

    private GameObject dungeonMainCamera;
    private GameObject dungeonLayersCamera;
    private GameObject eventUIHolder;

    private GameObject endGameDefeat;
    private GameObject endGameVictory;

    private void Awake()
    {
        mainCamera = GameObject.Find("Town Camera");
        townCamera = mainCamera.GetComponent<Camera>();
        lighting = GameObject.Find("Town Light");
        townUI = GameObject.Find("TownUI");
        townUICanvas = GameObject.Find("PlayerUI").GetComponent<Canvas>();
        eventUIHolder = GameObject.Find("Event UI");
        pauseMenu = GameObject.Find("Pause Menu");
        eventUIHolder.SetActive(false);
        dungeon = GameObject.Find("Dungeon").GetComponent<Dungeon>();
        dungeonMeterUI = GameObject.Find("Dungeon Meter");
        dungeonMeter = dungeon.gameObject.GetComponent<DungeonMeter>();

        //Set all Item & Skill Image Assets (as to not race in Awake)
        GameObject.Find("Item Image Assets").GetComponent<ItemAssets>().CreateInstance();
        GameObject.Find("Skill Image Assets").GetComponent<SkillAssets>().CreateInstance();

        SpawnPlayer();
        SetUpDungeonEntrance();
        SetUpInn();
        SetUpWeaponsmith();
        SetUpArmorsmith();
        AssignEndGameUIButtons();
    }


    //----------------SETTING UP INITIAL SCENE ASSETS----------------

    private void SpawnPlayer()
    {
        playerObject = Instantiate(Resources.Load<GameObject>("Prefabs/Player/player"), new Vector3(100f, 100f, 100f), Quaternion.identity);
        playerObject.gameObject.name = "Player";
        player = playerObject.GetComponent<Player>();

        player.inventory.CreateInventorySlots(player.inventoryCarryLoad);               //instantiate the player Inventory to show on UI

        playerMovement = playerObject.GetComponent<PlayerDungeonMovement>();
        playerMovement.SetUpPauseMenu(this);
        playerMovement.inTown = true;

        //ADDING UIPLAYERCOMPONENET, GIVEN MULTIPLE PLAYERS, I CAN ADD AND REMOVE FOR WHICH ONE TO DISPLAY
        player.playerUI = GameObject.Find("UIPlayerStats").GetComponent<UIPlayerStats>();

        //give UIManager reference to its player after spawning 
        UIManager.Instance.ReferenceInventoryUI(player, true);
    }

    private void SetUpInn()
    {
        innEntrance = GameObject.Find("Inn Entrance");
        innButton = innEntrance.GetComponent<Button>();
        innBackground = GameObject.Find("Inn Background");
        exitInn = GameObject.Find("Exit Inn");

        innButton.onClick.AddListener(EnterInn);
        exitInn.GetComponent<Button>().onClick.AddListener(ExitInn);

        innSelection = GameObject.Find("Inn Selection").GetComponent<Inn>();
        innSelection.SetUpShop(player);

        innBackground.SetActive(false);
    }

    private void SetUpWeaponsmith()
    {
        weaponsmithEntrance = GameObject.Find("Weaponsmith Entrance");
        weaponsmithButton = weaponsmithEntrance.GetComponent<Button>();
        weaponsmithBackground = GameObject.Find("Weaponsmith Background");
        exitWeaponsmith = GameObject.Find("Exit Weaponsmith");

        weaponsmithButton.onClick.AddListener(EnterWeaponsmith);
        exitWeaponsmith.GetComponent<Button>().onClick.AddListener(ExitWeaponsmith);

        weaponsmithInventory = GameObject.Find("Weaponsmith Shop Inventory").GetComponent<WeaponSmithInventory>();
        weaponsmithInventory.SetUpShop(player);

        weaponsmithBackground.SetActive(false);
    }

    private void SetUpArmorsmith()
    {
        armorsmithEntrance = GameObject.Find("Armorsmith Entrance");
        armorsmithButton = armorsmithEntrance.GetComponent<Button>();
        armorsmithBackground = GameObject.Find("Armorsmith Background");
        exitArmorsmith = GameObject.Find("Exit Armorsmith");

        armorsmithButton.onClick.AddListener(EnterArmorsmith);
        exitArmorsmith.GetComponent<Button>().onClick.AddListener(ExitArmorsmith);

        armorsmithShopInventory = GameObject.Find("Armorsmith Shop Inventory").GetComponent<ArmorSmithInventory>();
        armorsmithShopInventory.SetUpShop(player);

        armorsmithBackground.SetActive(false);
    }

    private void SetUpDungeonEntrance()
    {
        dungeonEntrance = GameObject.Find("Dungeon Entrance");
        enterDungeonButton = dungeonEntrance.GetComponent<Button>();

        enterDungeonButton.onClick.AddListener(EnterDungeonLayers);
    }


    //----------------CHANGING SCENES----------------

    private IEnumerator ChangeToDungeonLayersScene()
    {
        SaveManager.SaveGame();

        AsyncOperation loaded = SceneManager.LoadSceneAsync("Dungeon Layers", LoadSceneMode.Additive);
        loaded.allowSceneActivation = false;

        //turn off Town UI
        townUI.SetActive(false);
        mainCamera.SetActive(false);
        lighting.SetActive(false);

        while (!loaded.isDone)
        {
            if (loaded.progress >= 0.9f)
                loaded.allowSceneActivation = true;
            yield return null;
        }

        GameObject layersUI = GameObject.Find("DungeonLayersUI");                       //assign layers background and camera to UI canvas
        layersUI.transform.parent = townUICanvas.transform;
        layersUI.transform.SetAsFirstSibling();
        layersUI.transform.position = townUICanvas.transform.position;
        layersUI.transform.localScale = new Vector3(0.906315f, 0.906315f, 0.906315f);   //set predefined local scale for layersUI

        dungeonLayersCamera = GameObject.Find("Main Camera");
        townUICanvas.worldCamera = dungeonLayersCamera.GetComponent<Camera>();

        Destroy(GameObject.Find("DungeonLayersCanvas"));                                //remove dungeonLayer canvas

        StartCoroutine(AssignLayerEntrances());
        Debug.Log("Finsihed loading Dungeon Layers Scene and assigning Dungeon Layers");
        yield return null;
    }

    //Additevly load the dungeon scene from the "Dungeon Layers" Scene
    public IEnumerator ChangeToDungeonScene(int rows, int columns, int minRooms, int maxRooms, bool bossLayer)
    {
        SaveManager.SaveGame();                                //Save the Game once the player enters the dungeon

        //unload the "Dungeon Layers" scene and remove all associated UI
        SceneManager.UnloadSceneAsync("Dungeon Layers");
        Destroy(GameObject.Find("DungeonLayersUI"));

        AsyncOperation loaded = SceneManager.LoadSceneAsync("Dungeon", LoadSceneMode.Additive);
        loaded.allowSceneActivation = false;

        dungeonMeterUI.SetActive(false);                            //hide the dungeon meter in the dungeon
        foreach (Transform element in eventUIHolder.transform)      //set each EventUI gameObject active so EventManager can reference
        {
            element.gameObject.SetActive(true);
        }
        eventUIHolder.SetActive(true);

        while (!loaded.isDone)
        {
            if (loaded.progress >= 0.9f)
            {
                loaded.allowSceneActivation = true;
            }
            yield return null;
        }

        //once the scene has completely loaded set up the Dungeon scene
        AssignDungeonCamera();
        CreateDungeon(rows, columns, minRooms, maxRooms, bossLayer);

        yield return null;
    }


    //----------------HELPER FUNCTIONS ----------------

    //Assigns the player dungeon movement script to player object
    public void ActivatePlayerMovementScript()
    {
        playerObject.GetComponent<PlayerDungeonMovement>().enabled = true;
    }

    private void DeactivatePlayerMovementScript()
    {
        playerObject.GetComponent<PlayerDungeonMovement>().enabled = false;
    }

    //Attach the Dungeon Camera to follow the Player
    private void AssignDungeonCamera()
    {
        //townUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dungeonMainCamera = GameObject.Find("Main Camera");
        dungeonMainCamera.transform.parent = playerObject.transform;
        Vector3 playerObjPosition = playerObject.transform.position;
        dungeonMainCamera.transform.position = new Vector3(playerObjPosition.x + 140, playerObjPosition.y + 100, playerObjPosition.z - 35);
    }

    //assigns each layer in the dungeon the each layer entrance in "Dungeon Layers" scene
    //including updating the UI for the layers inside here
    private IEnumerator AssignLayerEntrances()
    {
        for (int i = 0; i < dungeon.layers.Count; i++)
        {

            dungeon.layers[i].AssignLayerEntrance();
            dungeon.layers[i].CheckForBoss();           //sets each layer if its a boss layer or not

            //check if the layer is accessible to the player, else don't assign listeners
            if (!dungeon.LayerAccessible(i)) continue;

            dungeon.layers[i].layerEnemyPool.layerEntrace.onClick.AddListener(dungeon.layers[i].CreateLayer);
            dungeon.layers[i].layerEnemyPool.layerEntrace.onClick.AddListener(dungeon.layers[i].SetPlayerCurrentLayer);
            dungeon.layers[i].layerEnemyPool.layerEntrace.onClick.AddListener(UIManager.Instance.ActivateDungeonRetreat);
        }
        GameObject.Find("Back To Town").GetComponent<Button>().onClick.AddListener(BackToTown);
        yield return null;
    }

    //Creates the dungoen from "Layer" script when the button in "DungeonLayers" scene is clicked
    public void CreateDungeon(int rows, int columns, int minRooms, int maxRooms, bool bossLayer)
    {
        StartCoroutine(SpawnDungeon(rows, columns, minRooms, maxRooms, bossLayer));
    }

    //Called from "CreateDungeon" above and calls DungeonManager to create the dungoen
    //and allows player movement to activate after its completion
    private IEnumerator SpawnDungeon(int rows, int columns, int minRooms, int maxRooms, bool bossLayer)
    {

        GameObject dungeonManagerObject = GameObject.Find("DungeonManager");
        DungeonManager dungeonManager = dungeonManagerObject.GetComponent<DungeonManager>();

        dungeonManager.CreateDungeon(rows, columns, minRooms, maxRooms, playerObject, bossLayer);   //200,200,25,40
        yield return new WaitForFixedUpdate();
    }

    //when the player has succefully finished a run, deplete the layer the player was in
    //based on how many enemies killed in that run
    public void DepleteLayers()
    {
        //layerNumber -1, because dungoen.layers is indexed at 0 and player.layerNumber is assigned by actual layer #
        dungeon.layers[player.layerNumber - 1].DepleteLayer(player.enemiesKilled);
    }

    //returns back to town from the DungeonLayers Scene
    private void BackToTown()
    {
        //unload the "Dungeon Layers" scene and remove all associated UI
        SceneManager.UnloadSceneAsync("Dungeon Layers");
        Destroy(GameObject.Find("DungeonLayersUI"));

        ReloadTown(Dungeon.Run.NoRun, false);
    }

    public void ReloadTown(Dungeon.Run run, bool playerDeath)
    {
        //UIManager.Instance.DeactivateDungeonRetreat();
        UIManager.Instance.DisableRetreats();
        UIManager.Instance.SetInventoryButtons(true);

        mainCamera.SetActive(true);
        townUICanvas.renderMode = RenderMode.ScreenSpaceCamera;
        townUICanvas.worldCamera = townCamera;
        lighting.SetActive(true);

        townUI.SetActive(true);             //turn on appropriate UI             
        dungeonMeterUI.SetActive(true);

        playerMovement.inTown = true;

        if (playerDeath) return;

        UpdateDungeonMeter(run);

        player.enemiesKilled = 0;           //reset player stats
        player.layerNumber = 0;
        player.fear = 0;
        playerMovement.ResetRoomPlacement();
        playerMovement.moves = 0;
        playerMovement.ResetMovesSinceCombat();

        SaveManager.SaveGame();
    }



    //----------------ADDING BUTTON LISTENERS----------------

    //when a player exits out of a shop menu reactive all Button Listeners
    public void ReactivateAllTownEntrances()
    {
        dungeonEntrance.SetActive(true);
        innEntrance.SetActive(true);
        weaponsmithEntrance.SetActive(true);
        armorsmithEntrance.SetActive(true);
        UIManager.Instance.SetDungeonInteractivePlayerUI(true);
    }

    public void DeactivateAllTownEntrances()
    {
        dungeonEntrance.SetActive(false);
        innEntrance.SetActive(false);
        weaponsmithEntrance.SetActive(false);
        armorsmithEntrance.SetActive(false);
        UIManager.Instance.SetDungeonInteractivePlayerUI(false);
    }

    //When the player enters the dungeon, set up a player hero and create a new dungeon
    public void EnterDungeon(int rows, int columns, int minRooms, int maxRooms, bool bossLayer)
    {
        StartCoroutine(ChangeToDungeonScene(rows, columns, minRooms, maxRooms, bossLayer));
    }

    private void EnterDungeonLayers()
    {
        StartCoroutine(ChangeToDungeonLayersScene());
    }

    private void EnterInn()
    {
        innBackground.SetActive(true);
        DeactivateAllTownEntrances();
    }

    private void ExitInn()
    {
        innBackground.SetActive(false);
        ReactivateAllTownEntrances();
    }

    private void EnterWeaponsmith()
    {
        weaponsmithBackground.SetActive(true);
        DeactivateAllTownEntrances();
    }

    private void ExitWeaponsmith()
    {
        weaponsmithBackground.SetActive(false);
        ReactivateAllTownEntrances();
    }

    private void EnterArmorsmith()
    {
        armorsmithBackground.SetActive(true);
        DeactivateAllTownEntrances();
    }

    private void ExitArmorsmith()
    {
        armorsmithBackground.SetActive(false);
        ReactivateAllTownEntrances();
    }

    private void AssignEndGameUIButtons()
    {
        endGameDefeat = GameObject.Find("End Game - Defeat");
        endGameVictory = GameObject.Find("End Game - Victory");

        GameObject.Find("End Game Defeat Button").GetComponent<Button>().onClick.AddListener(EndGameButtonListener);
        GameObject.Find("End Game Victory Button").GetComponent<Button>().onClick.AddListener(EndGameButtonListener);

        endGameDefeat.SetActive(false);
        endGameVictory.SetActive(false);
    }

    //adds the End Game function to the end game buttons when meter has reached either end (NOT in Dungeon)
    private void EndGameButtonListener()
    {
        EndGame();
    }


    //----------------GAME SYSTEM METHODS----------------

    //if this is a new game set all new game data
    public IEnumerator SetNewGameAttributes()
    {
        player.GiveStartingItems();        //give the player their starting items & skills only on a new game
        player.GiveStartingSkills();
        dungeon.CreateNewLayers();         //set the initial dungeon layer attributes
        dungeonMeter.CreateDungeonMeter(dungeon.dungeonHealthPool);
        yield return null;
    }

    public void UpdateDungeonMeter(Dungeon.Run run)
    {
        dungeonMeter.UpdateDungeonMeter(run, player, dungeon);
    }

    //sets the end game UI to active for player to interactive with end game screen
    public void DungeonMeterEndGame(bool dungeonCleared)
    {
        DeactivatePlayerMovementScript();       //don't allow the player to interact with pause menus
        DeactivateAllTownEntrances();

        if (dungeonCleared)
        {
            endGameVictory.SetActive(true);
        }
        else
        {
            endGameDefeat.SetActive(true);
        }
    }

    //if player dies, end the game through this function
    //all scenes should have been unloaded and are in the Town scene, calls "EndGame"
    public void PlayerDeathEndGame(bool inCombat)
    {
        if (inCombat)
        {
            gameObject.GetComponent<SaveManager>().RetreatFromCombat(true);
        }
        else
        {
            //gameObject.GetComponent<SaveManager>().RetreatFromDungeon();
            GameObject.Find("DungeonManager").GetComponent<DungeonManager>().ExitDungeon(Dungeon.Run.Fail, true);
        }

        DeactivatePlayerMovementScript();       //don't allow the player to interact with pause menus
        DeactivateAllTownEntrances();
        endGameDefeat.SetActive(true);
        GameObject.Find("End Game Defeat Button").GetComponent<Button>().onClick.AddListener(EndGameButtonListener);
    }

    //If the game is to end, delete the save and return to title screen
    public void EndGame()
    {
        Debug.Log("Deleting save file");
        File.Delete(Application.persistentDataPath + "/saves/DungeonSave.save");

        SceneManager.LoadScene("Title Screen");

    }
}

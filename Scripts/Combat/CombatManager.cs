using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//controls the transition between combat scenes
//sets up and orders combat at runtime
public class CombatManager : MonoBehaviour
{
    //tracking all character movement and weapon tile placement
    public enum Moves
    {
        Forward, Right, Left, Back
    }

    private RoomGenerator roomGenerator;
    private TileGenerator tileGenerator;

    private UIPlayerStats uiPlyaerStats;

    private PlayerDungeonMovement playerDungeonMovement;
    private PlayerCombatMovement playerMovement;        //Combat Player movementCounter
    public GameObject player;                           //combat player gameobject
    private Player dungeonPlayerStats;                  //dungeon player stats referenced from dungeon Player
    public Player playerStats;                          //Combat player stats to access

    private ScenesManager sceneManager;
    private GameObject findCamera;                      //combat camera

    private EventManager eventManager;                  //Use for CombatLootEvent

    private Enemy currentEnemy;                         //Game component of the current enemy on this turn

    public List<GameObject> enemiesInEncounter = new List<GameObject>();    //tracks how many enemies are in combat
    private List<string> enemiesPrefabPath;                                 //holds each of the enemies in the encounter prefab-path
    private List<GameObject> heroesInEncounter = new List<GameObject>();    //tracks how many allies/heroes are in combat

    public List<GameObject> initiativeOrder = new List<GameObject>();       //holds the gameobjects to access during initiative corresponding to 'initiative'
    public List<int> initiative = new List<int>();                          //holds the list of speeds in descending order corresponding gameobjects 'initiativeOrder'

    public int currentTurn = 0;
    public int encounterRating;                                             //total rating of combat encounter

    public static bool looting = false;                 //if the player is still looting after combat
    private bool bossEncounter;

    // Start is called before the first frame update
    void Start()
    {
        sceneManager = GameObject.Find("SceneManager").GetComponent<ScenesManager>();
        uiPlyaerStats = GameObject.Find("UIPlayerStats").GetComponent<UIPlayerStats>();

        eventManager = GameObject.Find("Event Manager").GetComponent<EventManager>();

        Debug.Log("Starting Combat");
        roomGenerator = gameObject.GetComponent<RoomGenerator>();
        tileGenerator = roomGenerator.tileGenerator;

        UIManager.Instance.ActivateCombatPlayerUI(true);
        UIManager.Instance.ActivateCombatUI(true);

        SpawnPlayer();

        //test what type of combat encounter right now only a basic one
        CreateCombatEncounter();

        Initiative();

        UIManager.Instance.SetTurnOrderUI(initiativeOrder);

        StartCombat();
    }

    private void SpawnPlayer()
    {
        dungeonPlayerStats = sceneManager.GetDungeonPlayer().GetComponent<Player>();
        playerDungeonMovement = sceneManager.GetDungeonPlayer().GetComponent<PlayerDungeonMovement>();

        //creates a combat representation of the player
        player = Instantiate(player, FindSpawnLocationInRoom(0.5f), Quaternion.identity) as GameObject;

        player.name = "CombatPlayer";
        playerStats = player.AddComponent<Player>();

        AssignValuesAndEquipment();

        findCamera = GameObject.Find("Main Camera");                                   //combat camera
        findCamera.AddComponent<CameraFollow>();

        //gives the player movement
        player.AddComponent<PlayerCombatMovement>();
        playerMovement = player.GetComponent<PlayerCombatMovement>();

        playerStats.currentSkill.SetSkillReferences(playerStats, playerMovement);       //set references to the players skills to be used in combat
        for (int i = 0; i < playerStats.skillSet.Count; i++) playerStats.skillSet[i].SetSkillReferences(playerStats, playerMovement);    //WILL NEED TO DO THIS IN ENEMY SCRIPTS TOO
        UIManager.Instance.SetPlayerSkillSlotImages(playerStats);

        heroesInEncounter.Add(player);
    }

    //gives the combat player the same stats dungeon player has
    //I CAN MAKE THIS AND REASSIGNEQUIPMENT INTO ONE METHOD BY PASSING TWO PARAMETERS TO ASSIGN STATS
    private void AssignValuesAndEquipment()
    {
        playerStats.skillSet = dungeonPlayerStats.skillSet;
        playerStats.currentSkill = dungeonPlayerStats.currentSkill;

        playerStats.SetDamage(dungeonPlayerStats.damage - dungeonPlayerStats.damageModifier);
        //playerStats.damageModifier = dungeonPlayerStats.damageModifier;
        playerStats.SetDamageModifier(dungeonPlayerStats.damageModifier);
        playerStats.armor = dungeonPlayerStats.armor;
        playerStats.SetHealth(dungeonPlayerStats.health);
        playerStats.deathRoll = dungeonPlayerStats.deathRoll;
        playerStats.speed = dungeonPlayerStats.speed;

        playerStats.fear = dungeonPlayerStats.fear;
        playerStats.horror = dungeonPlayerStats.horror;

        playerStats.helmArmor = dungeonPlayerStats.helmArmor;
        playerStats.bodyArmor = dungeonPlayerStats.bodyArmor;
        playerStats.greavesArmor = dungeonPlayerStats.greavesArmor;
        playerStats.bootArmor = dungeonPlayerStats.bootArmor;

        //assign all the dugeon player stats to the combat player stats
        playerStats.inventoryCarryLoad = dungeonPlayerStats.inventoryCarryLoad;
        playerStats.inventoryCarryWeight = dungeonPlayerStats.inventoryCarryWeight;
        playerStats.carryCapacity = dungeonPlayerStats.carryCapacity;
        playerStats.inventory = dungeonPlayerStats.inventory;
        playerStats.inventory.ReassignInventoryCharacters(playerStats);

        playerStats.weapon = dungeonPlayerStats.weapon;
        playerStats.weaponRange = dungeonPlayerStats.weaponRange;
        playerStats.enemiesKilled = dungeonPlayerStats.enemiesKilled;

        playerStats.gold = dungeonPlayerStats.gold;

        dungeonPlayerStats.SetUpPlayerUI(ref playerStats.playerUI);
        uiPlyaerStats.SetNewPlayer(playerStats, true);

        //after we are done transitioning values, deactive original player
        sceneManager.GetDungeonPlayer().SetActive(false);
    }


    private void CreateCombatEncounter()
    {
        Debug.Log("Event Tile index: " + sceneManager.indexOfEvent + " Size of tile events: " + sceneManager.currentTileEvents.Count);
        if (sceneManager.currentTileEvents[sceneManager.indexOfEvent] == Event.EventType.Boss)
        {
            bossEncounter = true;
            CreateBossEncounter();
        }
        else if(sceneManager.currentTileEvents[sceneManager.indexOfEvent] == Event.EventType.MiniBoss)
        {
            CreateMiniBossEncounter();
        }
        else
        {
            CreateGruntEncounter();
        }

        InstantiateEnemiesInEncounter();
        SetEnemySpawnPositions();
    }

    /*creates a random BasicCombatEncounter
     * basicCombatEncounter indexes:
     *          0: BasicEncounter
     *          1: BasicHeavyEncounter
     *          2: BasicWeakSwarmEncounter
    */
    private void CreateGruntEncounter()
    {
        //prob. of type of BasicCombatEncounter chosen, indexed from method hierarchy in "CombatEvent script"
        float[] typeOfBasicEncounter = new float[] { 0.33f, 0.33f, 0.34f};

        ProbabilityGenerator basicCombatEncounterSelected = new ProbabilityGenerator(typeOfBasicEncounter);
        int encounterChosen = basicCombatEncounterSelected.GenerateNumber();

        switch (encounterChosen)
        {
            case 0:
                enemiesPrefabPath = CombatEvent.CreateBasicEncounter();
                Debug.Log("Basic Combat Encounter Chosen");
                break;
            case 1:
                enemiesPrefabPath = CombatEvent.CreateBasicHeavyEncounter();
                Debug.Log("Brute Encounter Chosen");
                break;
            case 2:
                enemiesPrefabPath = CombatEvent.CreateBasicWeakSwarmEncounter();
                Debug.Log("Slave Encounter Chosen");
                break;
            default:
                Debug.Log("No Basic Combat Encounter Chosen");
                break;
        }

    }

    private void CreateMiniBossEncounter()
    {
        float probAssassin = 0.5f;
        if (Random.value <= probAssassin)
        {
            enemiesPrefabPath = CombatEvent.CreateAssassinMiniBossEncounter();
            Debug.Log("Mini Boss Encounter Chosen: Assasin");
        }
        else
        {
            enemiesPrefabPath = CombatEvent.CreateHornBlowerMiniBossEncounter();
            Debug.Log("Mini Boss Encounter Chosen: Horn Blower");
        }
    }

    private void CreateBossEncounter()
    {
        Debug.Log("BOSS ENCOUNTER CHOSEN");
        enemiesPrefabPath = CombatEvent.CreateAssassinMiniBossEncounter();
    }


    //spawns passed enemies onto the corners of the room
    public void CallReinforcements(List<string> reinforcements)
    {
        List<GameObject> enemies = new List<GameObject>();
      
        for(int i = 0; i < reinforcements.Count; i++)
        {
            GameObject enemy = Instantiate(Resources.Load<GameObject>(reinforcements[i]), Vector3.zero, Quaternion.identity) as GameObject;
            enemiesInEncounter.Add(enemy);
            enemies.Add(enemy);

            enemy.transform.position = FindReinforcementSpawnLocation(0.5f);

            enemy.GetComponent<Enemy>().SetTileAsObstacle(enemy.transform.position);
        }

        AddCharactersToInitiative(enemies);
    }

    //given a list of enemy prefab paths, instantiate these enemies in the scene
    private void InstantiateEnemiesInEncounter()
    {
        for(int i = 0; i < enemiesPrefabPath.Count; i++)
        {
            StartCoroutine(SpawnEnemy(i));
            encounterRating += enemiesInEncounter[i].GetComponent<Enemy>().combatRating;    //calculate the combats encounter rating
        }
    }

    private IEnumerator SpawnEnemy(int index)
    {
        GameObject enemy = Instantiate(Resources.Load<GameObject>(enemiesPrefabPath[index]), Vector3.zero, Quaternion.identity) as GameObject;
        enemiesInEncounter.Add(enemy);
        yield return null;
    }

    //helper method for combat encounters to call "FindSpawnLocation"
    private void SetEnemySpawnPositions()
    {
        for (int i = 0; i < enemiesInEncounter.Count; i++)
        {
            enemiesInEncounter[i].transform.position = FindSpawnLocationInRoom(0.5f);

            //set the enemies rotation to be randomly chosen
            ProbabilityGenerator prob = new ProbabilityGenerator(new float[]{ 0.25f, 0.25f, 0.25f, 0.25f});
            int lookDirection = prob.GenerateNumber();
            Enemy enemy = enemiesInEncounter[i].GetComponent<Enemy>();

            switch (lookDirection)
            {
                case 0: enemiesInEncounter[i].transform.rotation = Quaternion.Euler(Vector3.zero); break;
                case 1: enemiesInEncounter[i].transform.rotation = Quaternion.Euler(0f, 180f, 0f); break;
                case 2: enemiesInEncounter[i].transform.rotation = Quaternion.Euler(0f, 90f, 0f); break;
                case 3: enemiesInEncounter[i].transform.rotation = Quaternion.Euler(0f, -90f, 0f); break;
            }

            enemy.SetTileAsObstacle(enemiesInEncounter[i].transform.position);
        }
    }

    //spawns charcters on a favorable location in room
    private Vector3 FindSpawnLocationInRoom(float hieghtFromGround)
    {
        int searches = 0;
        int randomLocation;
        Vector3 characterPosition;

        do
        {
            //manually search for an open position for the enemy
            if (searches > 25)
            {
                for (int j = 0; j < roomGenerator.openPositions.Count; j++)
                {
                    characterPosition = new Vector3(roomGenerator.openPositions[j].x, hieghtFromGround, roomGenerator.openPositions[j].z);
                    
                    //only spawns enemies on open tiles without anything on them
                    if (tileGenerator.traversability[(int)characterPosition.x][(int)characterPosition.z] == TileGenerator.Traversability.Walkable
                        && tileGenerator.tiles[(int)characterPosition.x][(int)characterPosition.z] == TileGenerator.TileType.Floor)
                    {
                        roomGenerator.openPositions.RemoveAt(j);
                        return characterPosition;
                    }
                }
                randomLocation = Random.Range(1, roomGenerator.openPositions.Count - 1);
                roomGenerator.openPositions.RemoveAt(randomLocation);
                characterPosition = new Vector3(roomGenerator.openPositions[randomLocation].x, hieghtFromGround, roomGenerator.openPositions[randomLocation].z);
                break;
            }

            //keep randomly trying to find a suitable location for an enemy, until search limit reached

            randomLocation = Random.Range(1, roomGenerator.openPositions.Count - 1);
            characterPosition = new Vector3(roomGenerator.openPositions[randomLocation].x, hieghtFromGround, roomGenerator.openPositions[randomLocation].z);
            searches++;

        } while (tileGenerator.tiles[(int)characterPosition.x][(int)characterPosition.z] != TileGenerator.TileType.Floor);

        return characterPosition;
    }

    //finds a spawn location for reinforcing enemies spawned during combat
    private Vector3 FindReinforcementSpawnLocation(float heightFromGround)
    {
        Vector3 characterPosition;

        //IF I WANT ALL REINFORCING ENEMIES TO SPAWN RIGHT NEXT TO EACH OTHER
        //REPLACE CHARACTERPOSITION VECTOR3 WITH OPENPOSITIONS[I]
        List<Vector3> openPositions = roomGenerator.openPositions;

        //randomly find a spawn near a wall
        for(int i = 0; i < openPositions.Count; i++)
        {
            int randomIndex = Random.Range(0, openPositions.Count - 1);
            Vector3 randomPosition = openPositions[randomIndex];

            characterPosition = new Vector3(randomPosition.x, heightFromGround, randomPosition.z);
            if (NearRoomWall(characterPosition) && tileGenerator.traversability[(int)characterPosition.x][(int)characterPosition.z] == TileGenerator.Traversability.Walkable)
            {
                return characterPosition;
            }
            openPositions.RemoveAt(randomIndex);
        }
        //if one doesn't exist spawn anywhere in an open position in the room
        int randomLocation = Random.Range(1, openPositions.Count - 1);
        characterPosition = new Vector3(openPositions[randomLocation].x, heightFromGround, openPositions[randomLocation].z);
        return characterPosition;
    }

    //returns true if the passed Vector3 is adjacent to a TileGenerator.Wall
    private bool NearRoomWall(Vector3 position)
    {
        if (tileGenerator.tiles[(int)position.x + 1][(int)position.z] == TileGenerator.TileType.Wall) return true;
        if (tileGenerator.tiles[(int)position.x][(int)position.z + 1] == TileGenerator.TileType.Wall) return true;
        if (tileGenerator.tiles[(int)position.x - 1][(int)position.z] == TileGenerator.TileType.Wall) return true;
        if (tileGenerator.tiles[(int)position.x][(int)position.z - 1] == TileGenerator.TileType.Wall) return true;
        return false;
    }


    //creates the initiative order for the combat
    //creating an orderd list of gameobjects and characters to access their speed -- can definetely create a faster implementation O(n^2)
    private void Initiative()
    {
        for(int i = 0; i < enemiesInEncounter.Count; i++)
        {
            initiativeOrder.Add(enemiesInEncounter[i]);
            initiative.Add(enemiesInEncounter[i].GetComponent<Character>().speed);
        }
        initiativeOrder.Add(player);
        initiative.Add(player.GetComponent<Character>().speed);

        for(int i = 0; i < initiative.Count; i++)
        {
            int nextInInitiative = initiative[i];
            for (int j = 0; j < initiative.Count; j++)
            {
                if(initiative[j] < nextInInitiative)
                {
                    nextInInitiative = initiative[j];
                    initiative[j] = initiative[i];
                    initiative[i] = nextInInitiative;

                    GameObject temp = initiativeOrder[j];
                    initiativeOrder[j] = initiativeOrder[i];
                    initiativeOrder[i] = temp;
                }
            }
        }
    }

    //Adds newly spawned characters into the initative order
    private void AddCharactersToInitiative(List<GameObject> charactersToAdd)
    {
        for(int i = 0; i < charactersToAdd.Count; i++)
        {
            int charactersInitiative = charactersToAdd[i].GetComponent<Enemy>().speed;

            for(int j = 0; j < initiative.Count; j++)
            {
                //else place the character at the end of the initiative
                if (j == initiative.Count - 1)
                {
                    UIManager.Instance.PlaceCharacterInInitiative(charactersToAdd[i], true, initiativeOrder.Count, currentTurn);
                    initiative.Add(charactersInitiative);
                    initiativeOrder.Add(charactersToAdd[i]);
                    break;
                }

                if (charactersInitiative == initiative[j]) continue;

                if(charactersInitiative > initiative[j]){
                    UIManager.Instance.PlaceCharacterInInitiative(charactersToAdd[i], true, j, currentTurn);
                    initiative.Insert(j, charactersInitiative);
                    initiativeOrder.Insert(j, charactersToAdd[i]);
                    break;
                }
            }
        }
    }

    //once a character dies, remove them from the initiaitve order and destroy the gameobject from scene
    //if there are no enemies left in the encounter end the combat encounter
    public void RemoveCharacterFromInitiative(GameObject characterToRemove)
    {
        for (int i = 0; i < initiativeOrder.Count; i++)
        {
            if (characterToRemove.Equals(initiativeOrder[i]))
            {
                Debug.Log("Index to remove character: " + i);
                UIManager.Instance.PlaceCharacterInInitiative(characterToRemove, false, i, currentTurn);

                Debug.Log("Remvoing character from initiative and combat");
                if (i <= currentTurn) currentTurn--;        //update currentTurn if character killed hasn't already gone this round

                initiativeOrder.RemoveAt(i);
                initiative.RemoveAt(i);
                Vector3 enemyPreviousLocation = characterToRemove.GetComponent<Enemy>().previousLocation;
                tileGenerator.traversability[(int)enemyPreviousLocation.x][(int)enemyPreviousLocation.z] = TileGenerator.Traversability.Walkable;
                Destroy(characterToRemove);
            }
        }

        if (CombatOver())
        {
            Debug.Log("Combat is over exiting room");

            while (playerStats.statusEffects.Count > 0)                  //continuously apply all of the remaining effects on the player
            {
                playerStats.statusEffects[0].ApplyEffect();
            }

            Debug.Log("Creating CombatLootEvent");
            //create the loot event that the player can pick up after the combat, wihtin here combat ends after the last loot has been seen      
            CombatLootEvent combatLootEvent = new CombatLootEvent(player, eventManager.GetPlayer(), sceneManager, this);
        }
    }

    //determines which character starts off the initiative
    private void StartCombat()
    {
        if (initiativeOrder[0] != player)
        {
            NextTurn();
        }
        else
        {
            playerStats.characterTurn = true;
            playerStats.currentSkill.SetSkillDistance();
        }
    }

    //increments the turn count and updates turn order ui
    public void IncrementTurnCount()
    {
        UIManager.Instance.UpdateTurnOrderUI(initiativeOrder[currentTurn]);
        currentTurn++;
    }

    public void EndPlayerTurn()
    {
        CombatMovement.ClearMovement(ref playerStats.currentSkill, playerMovement.attacking, true);
        playerMovement.attacking = false;
        playerStats.SetCharacterLocation();
        IncrementTurnCount();
        NextTurn();
    }

    //Moves the combat to the next turn in the initiative order
    public void NextTurn()
    {
        playerStats.characterTurn = false;
        Debug.Log("Setting the players turn to false");

        Debug.Log("Current Turn = " + currentTurn);

        //end of initiative order
        if (currentTurn == initiativeOrder.Count)
        {
            currentTurn = 0;
            NextTurn();
        }
        // an enemy turn
        else if (initiativeOrder[currentTurn] != player)
        {
            UIManager.Instance.SetCombatInteractivePlayerUI(false);
            
            currentEnemy = initiativeOrder[currentTurn].GetComponent<Enemy>();

            currentEnemy.ReapplyEffects();
            currentEnemy.ApplyStatusEffects();
            StartCoroutine(currentEnemy.Turn());
        }
        //if it's the players turn
        else if (initiativeOrder[currentTurn] == player)
        {
            UIManager.Instance.SetCombatInteractivePlayerUI(true);

            playerStats.characterTurn = true;
            playerStats.currentSkill = playerStats.skillSet[0];             //default back to move skill    
            playerStats.currentSkill.movesMade = 0;
            playerStats.currentSkill.SetSkillDistance();
            Debug.Log("There are " + playerStats.statusEffects.Count + " status effects active on the player");
            playerStats.ReapplyEffects();
            playerStats.ApplyStatusEffects();
        }
    }

    //checks if there are no more enemies left in room - combat is over - return to dungeon
    private bool CombatOver()
    {
        int enemiesLeftInEncounter = 0;

        for (int i = 0; i < initiativeOrder.Count; i++)
        {
            Enemy enemyLeftInEncounter = initiativeOrder[i].GetComponent<Enemy>();
            if (enemyLeftInEncounter != null)
            {
                enemiesLeftInEncounter++;
            }
        }
        if (enemiesLeftInEncounter == 0) return true;
        return false;
    }

    //reassigns the combatPlayer stats to the DungeonPlayer stats
    private void ReassignValuesAndEquipment()
    {
        sceneManager.GetDungeonPlayer().SetActive(true);

        dungeonPlayerStats.skillSet = playerStats.skillSet;
        dungeonPlayerStats.currentSkill = playerStats.currentSkill;

        dungeonPlayerStats.SetDamage(playerStats.damage - playerStats.damageModifier);
        //dungeonPlayerStats.damageModifier = playerStats.damageModifier;
        dungeonPlayerStats.SetDamageModifier(playerStats.damageModifier);
        dungeonPlayerStats.armor = playerStats.armor;
        dungeonPlayerStats.SetHealth(playerStats.health);
        dungeonPlayerStats.deathRoll = playerStats.deathRoll;
        //dungeonPlayerStats.speed = playerStats.speed;             //should only be for permanent change in speed

        dungeonPlayerStats.fear = playerStats.fear;
        dungeonPlayerStats.horror = playerStats.horror;

        dungeonPlayerStats.inventory = playerStats.inventory;
        dungeonPlayerStats.carryCapacity = playerStats.carryCapacity;
        dungeonPlayerStats.inventoryCarryLoad = playerStats.inventoryCarryLoad;
        dungeonPlayerStats.inventoryCarryWeight = playerStats.inventoryCarryWeight;
        dungeonPlayerStats.inventory.ReassignInventoryCharacters(dungeonPlayerStats);

        dungeonPlayerStats.helmArmor = playerStats.helmArmor;
        dungeonPlayerStats.bodyArmor = playerStats.bodyArmor;
        dungeonPlayerStats.greavesArmor = playerStats.greavesArmor;
        dungeonPlayerStats.bootArmor = playerStats.bootArmor;

        dungeonPlayerStats.weapon = playerStats.weapon;
        dungeonPlayerStats.weaponRange = playerStats.weaponRange;
        dungeonPlayerStats.enemiesKilled = playerStats.enemiesKilled;

        dungeonPlayerStats.gold = playerStats.gold;

        uiPlyaerStats.SetNewPlayer(dungeonPlayerStats, false);
    }

    //kills all enemies in the room FOR devloper testing
    public void InstaWipeRoom()
    {
        currentTurn = 0;
        for (int i = 0; i < enemiesInEncounter.Count; i++)
        {
            Destroy(enemiesInEncounter[i]);
            playerStats.enemiesKilled++;
        }
        enemiesInEncounter.Clear();
        initiativeOrder.Clear();
        RemoveCharacterFromInitiative(null);
    }

    //kills all enemies in the room without calling for combat Loot or updating enemies killed
    public void WipeRoom()
    {
        currentTurn = 0;
        for (int i = 0; i < enemiesInEncounter.Count; i++)
        {
            Destroy(enemiesInEncounter[i]);
        }
        enemiesInEncounter.Clear();
        enemiesPrefabPath.Clear();
        initiativeOrder.Clear();
    }

    /*//brings the character back to 1 hp
    public void StabalizeCharacter(Character character)
    {
        character.health = 1;
    }*/

    //Ends the Combat Encounter after the Player is finished looting, Called from CombatLootEvent
    public void EndCombat()
    {
        WipeRoom();

        playerStats.characterTurn = false;

        ReassignValuesAndEquipment();

        sceneManager.RemoveDungeonTileEvent();                      //removes the this combat event from the dungeon

        CombatMovement.ClearMovement(ref playerStats.currentSkill, playerMovement.attacking, true);
        Destroy(player);

        UIManager.Instance.ClearUITurnOrder();

        playerDungeonMovement.ResetMovesSinceCombat();

        SceneManager.UnloadSceneAsync("Combat");

        UIManager.Instance.DeactivateCombatRetreat();
        UIManager.Instance.ActivateCombatUI(false);
        UIManager.Instance.ActivateCombatPlayerUI(false);
        UIManager.Instance.ActivateSwitchToSkillsButton(false);

        //retick all the gameobjects from the first scene so they are not viewable in transition scene
        sceneManager.GetBoardHolder().SetActive(true); ;
        sceneManager.GetDungeonMainCamera().SetActive(true);
        sceneManager.GetLightSource().SetActive(true);

        UIManager.Instance.ActivateDungeonRetreat();

        //if the player killed the boss, exit the dungeon after ending combat
        if(bossEncounter)
        {
            GameObject.Find("DungeonManager").GetComponent<DungeonManager>().ExitDungeon(Dungeon.Run.BossKilled, false);
        }
    }
}

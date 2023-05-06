using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour
{
    bool playerDeath;

    //unloads combat, then dungeon scenes, then saves the game
    public void RetreatFromCombat(bool playerDeath)
    {
        GameObject pauseMenu = GameObject.Find("Combat Pause Menu");

        if(playerDeath)
        {
            Debug.Log("Player not retreating from combat, dismissing turning off combat pause menu, player died");
            playerDeath = true;
        }

        Debug.Log("Retreating from Combat");
        StartCoroutine(RetreatToDungeon());
    }

    //after exiting combat scene, retreat from the dungeon scene
    private IEnumerator RetreatToDungeon()
    {
        CombatManager combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();
        combatManager.EndCombat();
        Debug.Log("Unloaded Combat Scene");

        UIManager.Instance.ActivateCombatRetreat();
        StartCoroutine(RetreatToTown());
        yield return null;
    }

    //unloads dungeon scene then saves the game
    public void RetreatFromDungeon()
    {
        Debug.Log("Retreating from dungeon");
        StartCoroutine(RetreatToTown());
    }

    //after unloading the dungeon into town, save the game
    public IEnumerator RetreatToTown()
    {
        DungeonManager dungeonManager = GameObject.Find("DungeonManager").GetComponent<DungeonManager>();
        dungeonManager.ExitDungeon(Dungeon.Run.Fail, playerDeath);

        if(!playerDeath)
        {
            GameObject.Find("Player").GetComponent<PlayerDungeonMovement>().gamePaused = false;     //upause the game after retreating
            CheckForLoss();
        }
        yield return null;
    }

    //if the player is to retreat from the Dungeon Or Combat
    //create a debuff on the player to signify leaving unscathed
    private void CheckForLoss()
    {

        Player player = GameObject.Find("Player").GetComponent<Player>();

        if(Random.value <= player.deathRoll)
        {
            Debug.Log("Hero has sustained an injury");
            Injury.SustainInjury(player);
            player.LoseLoot();
        }
        else
        {
            Debug.Log("Hero has escaped unscathed");
        }
    }

    public static void SaveGame()
    {
        Player player = GameObject.Find("Player").GetComponent<Player>();
        GameObject dungeonObject = GameObject.Find("Dungeon");
        Dungeon dungeon = dungeonObject.GetComponent<Dungeon>();
        DungeonMeter dungeonMeter = dungeonObject.GetComponent<DungeonMeter>();
        SaveData save = new SaveData(dungeon.numLayers);

        save.player.layerNumber = player.layerNumber;

        save.player.gold = player.gold;
        save.player.health = player.health;
        save.player.healthCap = player.healthCap;
        save.player.speed = player.speed;

        save.player.helmArmor = player.helmArmor.ToString();
        save.player.bodyArmor = player.bodyArmor.ToString();
        save.player.greavesArmor = player.greavesArmor.ToString();
        save.player.bootArmor = player.bootArmor.ToString();

        save.player.weapon = player.weapon.ToString();
        save.player.weaponRange = player.weaponRange;
        save.player.damage = player.damage;
        save.player.damageModifier = player.damageModifier;

        save.player.fear = player.fear;
        save.player.horror = player.horror;

        save.player.carryCapacity = player.carryCapacity;
        save.player.inventoryCarryLoad = player.inventoryCarryLoad;

        for (int i = 0; i < player.inventory.inventory.Count; i++)
        {
            if (!player.inventory.inventory[i].GetIsEmpty())
            {
                save.player.inventory.Add(player.inventory.inventory[i].ToString());
                save.player.itemCount.Add(player.inventory.inventory[i].quantity);
            }
        }

        //saving the dungeon data
        for(int i = 0; i < dungeon.layers.Count; i++)
        {
            save.dungeon.layers[i].layerHealthPool = dungeon.layers[i].layerHealthPool;
            save.dungeon.layers[i].currentLayerHealth = dungeon.layers[i].currentLayerHealthPool;
            save.dungeon.layers[i].layerThreshold = dungeon.layers[i].layerThreshold;
            save.dungeon.layers[i].layerNumber = dungeon.layers[i].layerNumber;
            save.dungeon.layers[i].minRows = dungeon.layers[i].minNumberRows;
            save.dungeon.layers[i].maxRows = dungeon.layers[i].maxNumberRows;
            save.dungeon.layers[i].minColumns = dungeon.layers[i].minNumberColumns;
            save.dungeon.layers[i].maxColumns = dungeon.layers[i].maxNumberColumns;
            save.dungeon.layers[i].minRooms = dungeon.layers[i].minNumberRooms;
            save.dungeon.layers[i].maxRooms = dungeon.layers[i].maxNumberRooms;
        }

        save.dungeon.dungeonMeter.maxDungeonMeterValue = dungeonMeter.maxDungeonMeterValue;
        save.dungeon.dungeonMeter.currentDungeonMeterValue = dungeonMeter.currentDungeonMeterValue;

        string json = JsonUtility.ToJson(save);

        Debug.Log(json);

        File.WriteAllText(Application.persistentDataPath + "/saves/DungeonSave.save", json);
    }

    public static void LoadFromTown()
    {
        if (File.Exists(Application.persistentDataPath + "/saves/DungeonSave.save"))
        {
            LoadGame();
        }
        else
        {
            Debug.Log("Save file does not exist. Save your game!");
        }
    }

    public void LoadFromTitleScreen()
    {
        if(File.Exists(Application.persistentDataPath + "/saves/DungeonSave.save"))
        {
            //prevent other tilte menu buttons to be selected when loading
            GameObject.Find("Load Game").GetComponent<Button>().interactable = false;
            GameObject.Find("New Game").GetComponent<Button>().interactable = false;

            StartCoroutine(LoadLevel());
        }
        else
        {
            Debug.Log("Save file does not exist. Start a new game!");
        }
    }

    private IEnumerator LoadLevel()
    {
        AsyncOperation loaded = SceneManager.LoadSceneAsync("Town", LoadSceneMode.Additive);
        loaded.allowSceneActivation = false;

        while (!loaded.isDone)
        {
            if (loaded.progress >= 0.9f)
            {
                loaded.allowSceneActivation = true;
            }
            yield return null;
        }

        MoveGameObjectsToTownScene();
        SceneManager.UnloadSceneAsync("Title Screen");
        LoadGame();
        UIManager.Instance.SetInventoryButtons(true);
    }

    //moves all initially instantiated gameobjects to the Town scene
    //before unloading the Start Menu
    private void MoveGameObjectsToTownScene()
    {
        SceneManager.MoveGameObjectToScene(GameObject.Find("Player"), SceneManager.GetSceneByName("Town"));
    }

    //transfers the players stats
    private static void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/saves/DungeonSave.save"))
        {
            Player player = GameObject.Find("Player").GetComponent<Player>();
            GameObject dungeonObject = GameObject.Find("Dungeon");
            Dungeon dungeon = dungeonObject.GetComponent<Dungeon>();
            DungeonMeter dungeonMeter = dungeonObject.GetComponent<DungeonMeter>();
            string saveString = File.ReadAllText(Application.persistentDataPath + "/saves/DungeonSave.save");

            SaveData save = JsonUtility.FromJson<SaveData>(saveString);

            player.layerNumber = save.player.layerNumber;

            player.SetHealth(save.player.health);
            player.healthCap = save.player.healthCap;
            player.speed = save.player.speed;
            player.inventoryCarryWeight = 0;

            player.EquipNewArmor((Armor)SaveDataItemParser.ItemParser(save.player.helmArmor));
            player.EquipNewArmor((Armor)SaveDataItemParser.ItemParser(save.player.bodyArmor));
            player.EquipNewArmor((Armor)SaveDataItemParser.ItemParser(save.player.greavesArmor));
            player.EquipNewArmor((Armor)SaveDataItemParser.ItemParser(save.player.bootArmor));
            player.EquipNewWeapon((Weapon)SaveDataItemParser.ItemParser(save.player.weapon));

            player.weaponRange = save.player.weaponRange;
            player.SetDamage(save.player.damage);
            //player.damageModifier = save.player.damageModifier;
            player.SetDamageModifier(save.player.damageModifier);

            player.fear = save.player.fear;
            player.horror = save.player.horror;

            //clear the players current inventory
            for (int i = 0; i < player.inventory.inventorySlots.Count; i++)
            {
                Destroy(player.inventory.inventorySlots[i]);
            }
            player.inventory.inventorySlots.Clear();
            player.inventory.inventory.Clear();

            //reassign saved players inventory
            player.inventoryCarryLoad = save.player.inventoryCarryLoad;
            player.carryCapacity = save.player.carryCapacity;

            //create new inventory, recalling CreateinventorySlots, and referencing the newly created InventorySlots buttons
            player.inventory = new Inventory(save.player.inventoryCarryLoad, save.player.carryCapacity);
            player.inventory.CreateInventorySlots(player.inventoryCarryLoad);
            UIManager.Instance.ReferenceInventoryUI(player, false);

            //give saved items to new player inventory
            for (int i = 0; i < save.player.inventory.Count; i++)
            {
                while (save.player.itemCount[i] > 0)
                {
                    player.inventory.AddItem(SaveDataItemParser.ItemParser(save.player.inventory[i]), player);
                    save.player.itemCount[i]--;
                }
            }

            //loading Dungeon data
            dungeon.layers = new List<Layer>();
            for (int i = 0; i < dungeon.numLayers; i++) dungeon.layers.Add(new Layer());

            for(int i = 0; i < save.dungeon.layers.Count; i++)
            {
                dungeon.layers[i].layerHealthPool = save.dungeon.layers[i].layerHealthPool;
                dungeon.layers[i].currentLayerHealthPool = save.dungeon.layers[i].currentLayerHealth;
                dungeon.layers[i].layerThreshold = save.dungeon.layers[i].layerThreshold;
                dungeon.layers[i].layerNumber = save.dungeon.layers[i].layerNumber;
                dungeon.layers[i].minNumberRows = save.dungeon.layers[i].minRows;
                dungeon.layers[i].maxNumberRows = save.dungeon.layers[i].maxRows;
                dungeon.layers[i].minNumberColumns = save.dungeon.layers[i].minColumns;
                dungeon.layers[i].maxNumberColumns = save.dungeon.layers[i].maxColumns;
                dungeon.layers[i].minNumberRooms = save.dungeon.layers[i].minRooms;
                dungeon.layers[i].maxNumberRooms = save.dungeon.layers[i].maxRooms;
            }

            dungeonMeter.maxDungeonMeterValue = save.dungeon.dungeonMeter.maxDungeonMeterValue;
            dungeonMeter.dungeonMeter.maxValue = save.dungeon.dungeonMeter.maxDungeonMeterValue;
            dungeonMeter.currentDungeonMeterValue = save.dungeon.dungeonMeter.currentDungeonMeterValue;
            dungeonMeter.dungeonMeter.value = save.dungeon.dungeonMeter.currentDungeonMeterValue;
        }
        else
        {
            Debug.Log("File doesn't exist");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***
    *This class is an extension of an independent rouge-like rpg in development
    *The purpose of this class is to pass a unique parameterized dungeon through different 'Layers'
    *Layers are entered with the UI, and updated after a the dungeon has been exited
***/
public class Layer
{
    public int layerNumber;
    public int layerHealthPool;
    public int currentLayerHealthPool;
    public int layerThreshold;
    public bool layerAccessible = false;
    private bool bossLayer = false;
    public LayerEnemyPool layerEnemyPool;

    //for Dungeon generation for this layer
    public int minNumberRows;
    public int maxNumberRows;
    public int minNumberColumns;
    public int maxNumberColumns;
    public int minNumberRooms;
    public int maxNumberRooms;

    //for holding temporary Layer Data when Loading
    public Layer() { }

    //for creating new Layer objects within the Data to be Saved
    public Layer(int layerNumber, int layerHealthPool, int layerThreshold, int minRows, int maxRows, int minColumns, int maxColumns, int minRooms, int maxRooms)
    {
        this.layerNumber = layerNumber;
        this.layerHealthPool = layerHealthPool;
        currentLayerHealthPool = layerHealthPool;

        if (layerNumber == 1) currentLayerHealthPool = (int)(0.7 * layerHealthPool);

        this.layerThreshold = layerThreshold;
        minNumberRows = minRows;
        maxNumberRows = maxRows;
        minNumberColumns = minColumns;
        maxNumberColumns = maxColumns;
        minNumberRooms = minRooms;
        maxNumberRooms = maxRooms;
    }

    //set this dungeon layer to the UI layer of the "Dungeon Layers" scene
    //is called everytime the "Dungeon Layers" scene is loaded for every layer
    public void AssignLayerEntrance()
    {
        layerAccessible = true;
        layerEnemyPool = GameObject.Find("Layer Entrance " + layerNumber.ToString()).GetComponent<LayerEnemyPool>();
        layerEnemyPool.SetLayerHealthPool(layerHealthPool);
        layerEnemyPool.SetCurrentLayerPool(currentLayerHealthPool);
        layerEnemyPool.SetLayerThreshold(layerThreshold);
    }

    public void SetPlayerCurrentLayer()
    {
        Player player = GameObject.Find("Player").GetComponent<Player>();
        player.layerNumber = layerNumber;
    }

    public void CreateLayer()
    {
        int rows = Random.Range(minNumberRows, maxNumberRows);
        int columns = Random.Range(minNumberColumns, maxNumberColumns);
        int rooms = Random.Range(minNumberRooms, maxNumberRooms);

        GameManager gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        gameManager.EnterDungeon(rows, columns, minNumberRooms, maxNumberRooms, bossLayer);        
    }

    public void DepleteLayer(int enemiesKilled)
    {
        if (currentLayerHealthPool - enemiesKilled <= 0)
        {
            currentLayerHealthPool = 0;
            layerEnemyPool.SetCurrentLayerPool(0);
        }
        else
        {
            currentLayerHealthPool -= enemiesKilled;
            layerEnemyPool.SetCurrentLayerPool(currentLayerHealthPool);
        }
    }

    //returns if the boss can spawn in this layer
    public void CheckForBoss()
    {
        if (layerNumber == 5 && currentLayerHealthPool <= layerThreshold)
        {
            bossLayer = true;
        }
        else
        {
            bossLayer = false;
        }
    }
}

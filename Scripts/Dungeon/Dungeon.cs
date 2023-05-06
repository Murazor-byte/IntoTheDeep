using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Class that represents the entire dungeon near this town
public class Dungeon : MonoBehaviour
{
    public readonly int dungeonHealthPool = 1000;

    //Player results of the dungeon run whenever they get back to town
    public enum Run
    {
        Successful, BossKilled, Fail, NoRun
    }

    public int numLayers = 5;
    public List<Layer> layers;

    public void AssignLayerPools()
    {
        if(System.IO.File.Exists(Application.persistentDataPath + "/saves/DungeonSave.save"))
        {
            CreateNewLayers();
        }
    }

    //sets each layer's attributes in the dungeon
    //only called whenever a new game is made by setting its initial layer health pool and layerNumber
    public void CreateNewLayers()
    {
        layers = new List<Layer>();
        layers.Add(new Layer(1, 75, 25, 100, 100, 100, 100, 4, 10));        //50 for max layer health, 25 for threshold
        layers.Add(new Layer(2, 150, 47, 100, 200, 100, 200, 8, 15));
        layers.Add(new Layer(3, 200, 40, 100, 250, 100, 250, 12, 24));
        layers.Add(new Layer(4, 250, 25, 125, 275, 125, 275, 22, 35));
        layers.Add(new Layer(5, 325, 20,  175, 300, 175, 300, 34, 50));
    }

    //checks whether the player can enter layers of the dungeon
    //Player must have the previoys layer's current enemy health below or equal to that layers threshold
    public bool LayerAccessible(int layerNumber)
    {
        //first layer is always accessible
        if (layerNumber == 0) return true;

        if (layers[layerNumber - 1].currentLayerHealthPool <= layers[layerNumber - 1].layerThreshold) return true;

        return false;
    }

    public Layer GetLayer(int layerIndex)
    {
        if(layerIndex >= layers.Count)
        {
            Debug.Log("CAN'T ACCESS LAYERS INDEX, TOO BIG");
            return null;
        }
        return layers[layerIndex];
    }

    public void DepleteLayerPool(int layerNumber, int enemiesKilled)
    {
        layerNumber--;                              //since this is called from player and is indexed from 1
        layers[layerNumber].DepleteLayer(enemiesKilled);
    }

}

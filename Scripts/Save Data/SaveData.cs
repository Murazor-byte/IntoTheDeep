using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    //private SaveData saveData;
    /*public SaveData current
    {
        get
        {
            if(saveData == null)
            {
                saveData = new SaveData();
            }
            return saveData;
        }
        set
        {
            if(value != null)
            {
                saveData = value;
            }
        }
    }*/

    public PlayerProfile player;
    public DungeonProfile dungeon;

    public SaveData(int numDungeonLayers)
    {
        player = new PlayerProfile();
        dungeon = new DungeonProfile(numDungeonLayers);
    }
}

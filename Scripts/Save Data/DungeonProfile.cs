using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DungeonProfile
{
    public DungeonMeterProfile dungeonMeter;
    public List<LayerProfile> layers;

    public DungeonProfile(int numDungeonLayers)
    {
        dungeonMeter = new DungeonMeterProfile();
        layers = new List<LayerProfile>();
        for (int i = 0; i < numDungeonLayers; i++) layers.Add(new LayerProfile());
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LayerProfile
{
    public int layerNumber;
    public int layerHealthPool;
    public int currentLayerHealth;
    public int layerThreshold;

    public int minRows;
    public int maxRows;
    public int minColumns;
    public int maxColumns;
    public int minRooms;
    public int maxRooms;
}

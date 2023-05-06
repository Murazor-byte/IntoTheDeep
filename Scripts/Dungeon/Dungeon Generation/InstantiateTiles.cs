using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*This class only instantiates tiles
already set from DungeonGenerator or GridGenerator class
this allows for me to easily create a DungeonManager class-
which keeps track of everything*/
public class InstantiateTiles : MonoBehaviour
{

    //goes through every unit on board and place a wall or floor there
    public void InstantiateAllTiles(DungeonGenerator.TileType[][] tiles, GameObject boardHolder, GameObject floorTiles, GameObject wallTiles)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {

                if (tiles[i][j] == DungeonGenerator.TileType.Wall)
                {
                    InstantiateFromArray(floorTiles, i, j, boardHolder);
                }
                else
                {
                    InstantiateFromArray(wallTiles, i, j, boardHolder);
                }
            }
        }
    }

    //instantiate tiles for Dungeon on '2-D' map
    private void InstantiateFromArray(GameObject prefab, float xCoord, float yCoord, GameObject boardHolder)
    {
        Vector3 position = new Vector3(xCoord, yCoord);

        GameObject tileInstance = Instantiate(prefab, position, Quaternion.identity) as GameObject;

        tileInstance.transform.parent = boardHolder.transform;
    }

}

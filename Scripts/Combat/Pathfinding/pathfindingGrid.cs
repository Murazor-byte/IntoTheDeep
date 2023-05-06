using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * Code Inspired from CodeMonkey
*/

public class PathfindingGrid
{
    private int width;
    private int hieght;

    public PathNode[,] pathfindingGrid;

    public PathfindingGrid(int width, int hieght, TileGenerator.TileType[][] tileType, TileGenerator.Traversability[][] traversibility, Func<PathfindingGrid, int, int, PathNode> gridObject)
    {
        this.width = width;
        this.hieght = hieght;

        pathfindingGrid = new PathNode[width, hieght];
        for (int i = 0; i < pathfindingGrid.GetLength(0); i++)
        {
            for (int j = 0; j < pathfindingGrid.GetLength(1); j++)
            {
                pathfindingGrid[i,j] = gridObject(this,i,j);

                //sets path nodes to be unwalkable
                if (traversibility != null && traversibility[i][j] != TileGenerator.Traversability.Walkable)
                {
                    pathfindingGrid[i, j].isWalkable = false;
                }

                //setting the nodes penatly costs
                switch (tileType[i][j])
                {
                    case TileGenerator.TileType.Lava:
                        pathfindingGrid[i, j].penatlyCost = 50;
                        break;
                    case TileGenerator.TileType.Water:
                        pathfindingGrid[i, j].penatlyCost = 20;
                        break;
                    case TileGenerator.TileType.Pit:
                        pathfindingGrid[i,j].penatlyCost = 50;
                        break;
                    default:
                        pathfindingGrid[i, j].penatlyCost = 0;
                        break;
                }
            }
        }
    }

    //when passing as a Vector3 => tuple
    public void GetGridObject(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt(worldPosition.x);
        y = Mathf.FloorToInt(worldPosition.y);
    }

    //when passing as a tuple
    public PathNode GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 00 && x < width && y < hieght)
        {
            return pathfindingGrid[x,y];
        }
        else
        {
            return default(PathNode);
        }
    }

    //When passing as a tuple
    public void SetGridValue(int x, int y, PathNode value)
    {
        pathfindingGrid[x,y] = value;
    }
    
    //when passing as a Vector3 => tuple
    public void SetGridValue(Vector3 worldPosition, PathNode value)
    {
        int x, y;
        GetGridObject(worldPosition, out x, out y);
        SetGridValue(x, y, value);
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return hieght;
    }
    
}

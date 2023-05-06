using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Code inspired from CodeMonkey
*/

public class PathNode
{
    private PathfindingGrid pathfindingGrid;

    public int x;
    public int z;

    public int gCost;                   //cost from start node
    public int hCost;                   //cost of direct path from startNode to endNode
    public int fCost;                   //gCost + hCost
    public int penatlyCost = 0;         //penatly for this node (effect tile)
    public int proximityPenalty = 0;    //cumaltive penalty for proximity to target (only for "Retreat" pathfinding)

    public bool isWalkable;

    public PathNode previousNode;

    public PathNode(PathfindingGrid pathfindingGrid, int x, int z)
    {
        this.pathfindingGrid = pathfindingGrid;
        this.x = x;
        this.z = z;
        isWalkable = true;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    //sets the current nodes walkable trait
    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }
}

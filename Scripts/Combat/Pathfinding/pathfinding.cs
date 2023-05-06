using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Code inspired from CodeMonkey
*/

public class Pathfinding
{
    private const int MOVE_COST = 10;                      //cost to move between nodes by default
    private const int PROXIMITY_MODIFIER = 270;            //cost to modify the targetPenalty
    private const int REATREAT_PENALTYCOST_MODIFIER = 2;   //modifier for penaltyCosts on nodes, ensuring less chance of retreating on negative tiles
     
    private PathfindingGrid pathfindingGrid;               //holds all nodes within a 'room'

    public List<PathNode> openList;                        //nodes not already searched
    public List<PathNode> closedList;                      //nodes already searched

    public Pathfinding(int width, int height, TileGenerator.TileType[][] tileType, TileGenerator.Traversability[][] traversability)
    {
        pathfindingGrid = new PathfindingGrid(width, height, tileType, traversability, (PathfindingGrid grid, int x, int z) => new PathNode(grid,x,z));
    }

    public PathfindingGrid GetGrid()
    {
        return pathfindingGrid;
    }

    //finds the optimal path between start coords and end coords on a 3-D plane
    public List<Vector3> FindPath(Vector3 startPosition, Vector3 endPosition)
    {
        pathfindingGrid.GetGridObject(startPosition, out int startX, out int startY);
        pathfindingGrid.GetGridObject(endPosition, out int endX, out int endY);

        List<PathNode> path = FindPath(startX, startY, endX, endY);

        if(path == null)
        {
            return null;
        }
        else
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach(PathNode pathNode in path)
            {
                vectorPath.Add(new Vector3(pathNode.x, pathNode.z));
            }
            return vectorPath;
        }

    }

    //finds the optimal path between start coords and end coords on a 2-D plane
    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = pathfindingGrid.GetGridObject(startX, startY);
        PathNode endNode = pathfindingGrid.GetGridObject(endX, endY);

        if(startNode == null || endNode == null)
        {
            Debug.Log("Start or endNode was null");
            return null;
        }

        openList = new List<PathNode>() {startNode};
        closedList = new List<PathNode>();

        for(int i = 0; i < pathfindingGrid.GetWidth(); i++)
        {
            for(int j = 0; j < pathfindingGrid.GetHeight(); j++)
            {
                PathNode pathNode = pathfindingGrid.GetGridObject(i, j);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.previousNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistance(startNode, endNode);
        startNode.CalculateFCost();

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if(currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach(PathNode neighborNode in GetNeighborNodes(currentNode))
            {
                if ((closedList.Contains(neighborNode) || !neighborNode.isWalkable) && neighborNode !=endNode) continue;

                int totalCost = currentNode.gCost + CalculateDistance(currentNode, neighborNode) + currentNode.penatlyCost;

                if (totalCost < neighborNode.gCost)
                {
                    neighborNode.previousNode = currentNode;
                    neighborNode.gCost = totalCost;
                    neighborNode.hCost = CalculateDistance(neighborNode, endNode);
                    neighborNode.CalculateFCost();

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                        
                    }
                }
            }
        }
        Debug.Log("Out of bounds on opend List");
        return null;
    }

    //find the most optimal path AWAY from the end PathNode, taking into account proximity to the target
    public List<PathNode> Retreat(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = pathfindingGrid.GetGridObject(startX, startY);
        PathNode endNode = pathfindingGrid.GetGridObject(endX, endY);

        if (startNode == null || endNode == null)
        {
            Debug.Log("Start or endNode was null");
            return null;
        }

        openList = new List<PathNode>() { startNode };
        closedList = new List<PathNode>();

        for (int i = 0; i < pathfindingGrid.GetWidth(); i++)
        {
            for (int j = 0; j < pathfindingGrid.GetHeight(); j++)
            {
                PathNode pathNode = pathfindingGrid.GetGridObject(i, j);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.previousNode = null;
            }
        }

        startNode.gCost = 0;
        int distanceToTarget = CalculateFarDistance(startNode, endNode);
        double proximityPenalty = distanceToTarget / MOVE_COST;
        startNode.proximityPenalty = (int)(1 / proximityPenalty * PROXIMITY_MODIFIER);
        startNode.hCost = distanceToTarget - startNode.proximityPenalty;
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestHCostNode(openList);
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighborNode in GetNeighborNodes(currentNode))
            {
                if (closedList.Contains(neighborNode) || !neighborNode.isWalkable || neighborNode == endNode) continue;

                int totalCost = currentNode.gCost + CalculateDistance(currentNode, neighborNode) ;

                if (totalCost < neighborNode.gCost)
                {
                    neighborNode.previousNode = currentNode;
                    neighborNode.gCost = totalCost;

                    distanceToTarget = (CalculateFarDistance(neighborNode, endNode));
                    proximityPenalty = distanceToTarget / MOVE_COST;
                    neighborNode.proximityPenalty = (int)(1 / proximityPenalty * PROXIMITY_MODIFIER);

                    //if the Enemy is moving toward the target, cumulate the proximity penalty
                    if (CalculateFarDistance(neighborNode.previousNode, endNode) >= distanceToTarget)
                    {
                        neighborNode.proximityPenalty += neighborNode.previousNode.proximityPenalty;
                    }
                    //else they are moving away from the target, don't create an additional penalty
                    else
                    {
                        neighborNode.proximityPenalty = 0 + neighborNode.previousNode.proximityPenalty;
                    }

                    //subtract penalty cost as the enemy still doesn't want to walk on negative tiles
                    neighborNode.hCost = distanceToTarget - neighborNode.proximityPenalty - (currentNode.penatlyCost * 2);
                    neighborNode.CalculateFCost();

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        PathNode farthestNode = GetHighestHCostNode(closedList);
        //Debug.Log("Farthest Node = " + farthestNode.x + ", " + farthestNode.z);

        List<PathNode> calculatedPath = CalculatePath(farthestNode);
        
        return calculatedPath;
    }

    public List<PathNode> FindIdlePath(int startX, int startZ, int movement)
    {
        PathNode startNode = pathfindingGrid.GetGridObject(startX, startZ);

        if(startNode == null)
        {
            Debug.Log("Start Node was null");
            return null;
        }

        openList = new List<PathNode>() { startNode };
        closedList = new List<PathNode>();

        startNode.gCost = 0;
        startNode.hCost = 0;
        startNode.CalculateFCost();

        PathNode currentNode = startNode;

        while(openList.Count > 0)
        {
            currentNode = openList[0];

            foreach(PathNode neighborNode in GetNeighborNodes(currentNode))
            {
                if (closedList.Contains(neighborNode) || openList.Contains(neighborNode) || !neighborNode.isWalkable) continue;

                if ((CalculateFarDistance(neighborNode, startNode) / MOVE_COST) <= movement)
                {
                    //subtracting penalty cost as to not move on unfavorable tiles
                    int totalCost = currentNode.gCost + CalculateDistance(currentNode, neighborNode) - currentNode.penatlyCost;
                    neighborNode.gCost = totalCost;
                    neighborNode.hCost = CalculateFarDistance(neighborNode, startNode);
                    neighborNode.CalculateFCost();

                    openList.Add(neighborNode);
                }
            }

            closedList.Add(currentNode);
            openList.Remove(currentNode);
        }

        PathNode idleNode = GetHighestFCostIdleNode(closedList);
        Debug.Log("Enemy moving to idle path at " + idleNode.x + ", " + idleNode.z);

        List<PathNode> idlePath = FindPath(startX, startZ, idleNode.x, idleNode.z);
        return idlePath;
    }

    //gets the neighbor nodes to the current node
    private List<PathNode> GetNeighborNodes(PathNode currentNode)
    {
        List<PathNode> neighborNodes = new List<PathNode>();

        //left Node
        if (currentNode.x - 1 >= 0) neighborNodes.Add(GetNode(currentNode.x - 1, currentNode.z));
        //Right Node
        if (currentNode.x + 1 < pathfindingGrid.GetWidth()) neighborNodes.Add(GetNode(currentNode.x + 1, currentNode.z));
        //Up Node
        if (currentNode.z + 1 < pathfindingGrid.GetHeight()) neighborNodes.Add(GetNode(currentNode.x, currentNode.z + 1));
        //Down Node
        if (currentNode.z - 1 >= 0) neighborNodes.Add(GetNode(currentNode.x, currentNode.z - 1));

        return neighborNodes;
    }

    //given the endNode, trace back its previous nodes to add to a newly returned list
    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>() { endNode };
        PathNode currentNode = endNode;

        while (currentNode.previousNode != null)
        {            
            path.Add(currentNode.previousNode);
            currentNode = currentNode.previousNode;
        }

        path.Reverse();
        return path;
    }

    //finds the cost value between two nodes going horizontally then vertically (ignoring unwalkable spaces)
    private int CalculateDistance(PathNode first, PathNode second)
    {
        int xDistance = Mathf.Abs(first.x - second.x);
        int yDistance = Mathf.Abs(first.z - second.z);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_COST * remaining;
    }

    //similar to "CalculateDistance" Except takes the sum of the two vlaues
    //(This is for finding the distance farthest away from the two nodes,
    //while "CalculateDistance" is for the distance closest to the two nodes)
    private int CalculateFarDistance(PathNode first, PathNode second)
    {
        int xDistance = Mathf.Abs(first.x - second.x);
        int yDistance = Mathf.Abs(first.z - second.z);
        int remaining = Mathf.Abs(xDistance + yDistance);
        return MOVE_COST * remaining;
    }

    //gets the lowest fCost value of a node in a list
    private PathNode GetLowestFCostNode(List<PathNode> pathList)
    {
        PathNode lowestFCostNode = pathList[0];
        for(int i = 0; i < pathList.Count; i++)
        {
            if(pathList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathList[i];
            }
        }
        return lowestFCostNode;
    }

    //gets the highest fCost value of an idle node in a list
    //and selecting a random node if they are equal
    private PathNode GetHighestFCostIdleNode(List<PathNode> pathList)
    {
        PathNode highestFCostNode = pathList[0];
        for (int i = 0; i < pathList.Count; i++)
        {
            if (pathList[i].fCost > highestFCostNode.fCost)
            {
                highestFCostNode = pathList[i];
            }
            //if the idle nodes are of same distance, randomly select one to be the highest idleNode
            else if(pathList[i].fCost == highestFCostNode.fCost)
            {
                if (Random.value < 0.5f) highestFCostNode = pathList[i];
            }
        }
        return highestFCostNode;
    }

    private PathNode GetHighestHCostNode(List<PathNode> pathList)
    {
        PathNode highestHCostNode = pathList[0];
        for(int i = 0; i < pathList.Count; i++)
        {
            if(pathList[i].hCost > highestHCostNode.hCost)
            {
                highestHCostNode = pathList[i];
            }
        }
        return highestHCostNode;
    }

    private PathNode GetLowestHCostNode(List<PathNode> pathList)
    {
        PathNode lowestHCostNode = pathList[0];
        for (int i = 0; i < pathList.Count; i++)
        {
            if (pathList[i].hCost > lowestHCostNode.hCost)
            {
                lowestHCostNode = pathList[i];
            }
        }
        return lowestHCostNode;
    }

    //call the PathNode.method to retrieve a the node in the grid at these indicies
    private PathNode GetNode(int x, int y)
    {
        return pathfindingGrid.GetGridObject(x, y);
    }

}

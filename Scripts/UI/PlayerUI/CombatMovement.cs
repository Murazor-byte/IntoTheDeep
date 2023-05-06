using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this class is for instantiating skill distance tiles and all movements tiles for
    //both combat and player movement
public class CombatMovement : MonoBehaviour
{
    public enum Moves
    {
        Forward, Right, Left, Back
    }

    public static float moveMarkerHieght = 0.55f;
    public static int localDistance = 1;

    public static int DistanceFromPlayer(Vector3 location, Character character)
    {
        int xDistance = (int)Mathf.Abs(location.x - character.transform.position.x);
        int zDistance = (int)Mathf.Abs(location.z - character.transform.position.z);
        int xPath = (int)Mathf.Pow(xDistance, 2);
        int zPath = (int)Mathf.Pow(zDistance, 2);
        return (int)Mathf.Sqrt(xPath + zPath);
    }

    //removes the last moves, and deletes the last move marker
    public static void BackTrack(Skill skill, bool attacking)
    {
        Destroy(skill.moveMarkers[skill.moveMarkers.Count - 1]);
        skill.moveMarkers.RemoveAt(skill.moveMarkers.Count - 1);
        skill.moves.RemoveAt(skill.moves.Count - 1);
        skill.moveLocations.RemoveAt(skill.moveLocations.Count - 1);

        if (attacking) skill.singelEnemySelected.RemoveAt(skill.singelEnemySelected.Count - 1);
    }

    //clears all attack/move markers, and skill distance markers based on params
    public static void ClearMovement(ref Skill skill, bool attacking, bool destroyDistanceMarkers)
    {
        if (destroyDistanceMarkers) skill.DestroySkillDistanceMarkers();

        for (int i = 0; i < skill.moveMarkers.Count; i++)
        {
            Destroy(skill.moveMarkers[i]);
            if (!attacking) skill.movesMade--;
        }

        if (skill.skillType != Skill.SkillType.Line || (skill.skillType == Skill.SkillType.Line && destroyDistanceMarkers))
        {
            if (skill.skillType != Skill.SkillType.Cone && skill.skillType != Skill.SkillType.Sweep) skill.moveLocations.Clear();
            skill.selectedAttackTiles.Clear();
            skill.selectedCharacters.Clear();
        }

        skill.ResetSkillDistanceMarkers();
        skill.moveMarkers.Clear();
        skill.moves.Clear();
        skill.singelEnemySelected.Clear();
        skill.singleEnemySelected = null;
        skill.currentLineMove = new Vector3(skill.character.transform.position.x, moveMarkerHieght, skill.character.transform.position.z);
        skill.currentConeMove = skill.currentLineMove;
        skill.currentDirections = new List<int>() { 0, 0, 0, 0 };
    }

    ///<summary>
    ///returns the local tile traversability based on the last direction the player moved
    ///lastMove" returning position of character or the last move location
    ///</summary>
    public static TileGenerator.Traversability LocalTile(ref PlayerCombatMovement movement, Skill skill, Moves toMove, Character character)
    {
        Vector3 localPosition = LocalPosition(toMove, character, skill, CombatMovement.localDistance);
        return movement.tiles.traversability[(int)localPosition.x][(int)localPosition.z];
    }

    //returns the local tile type based on the last direction player moved
    //"lastMove" returning position of character or the last move location
    public static TileGenerator.TileType LocalTileType(Moves toMove, ref PlayerCombatMovement playerMovement, Character character, Skill skill)
    {
        Vector3 localPosition = LocalPosition(toMove, character, skill, CombatMovement.localDistance);
        return playerMovement.tiles.tiles[(int)localPosition.x][(int)localPosition.z];
    }

    //returns the local position 'distance' far away, based on the last direction the player moved
    //"lastMove" returning the local position of character or lastMove vector3
    public static Vector3 LocalPosition(Moves toMove, Character character, Skill skill, int distance)
    {
        Vector3 localPosition = character.transform.position;
        if (skill.skillType == Skill.SkillType.Line) localPosition = skill.currentLineMove;
        else if (skill.skillType == Skill.SkillType.Cone || skill.skillType == Skill.SkillType.Sweep)
        {
            localPosition = skill.localPos;
        }
        else if (skill.moveLocations.Count > 0) localPosition = skill.moveLocations[skill.moveLocations.Count - 1];


        switch (toMove)
        {
            case Moves.Forward:
                switch (skill.lastDirection)
                {
                    case Moves.Forward: return new Vector3(localPosition.x, moveMarkerHieght, localPosition.z + distance);
                    case Moves.Right: return new Vector3(localPosition.x + distance, moveMarkerHieght, localPosition.z);
                    case Moves.Back: return new Vector3(localPosition.x, moveMarkerHieght, localPosition.z - distance);
                    case Moves.Left: return new Vector3(localPosition.x - distance, moveMarkerHieght, localPosition.z);
                }
                break;
            case Moves.Right:
                switch (skill.lastDirection)
                {
                    case Moves.Forward: return new Vector3(localPosition.x + distance, moveMarkerHieght, localPosition.z);
                    case Moves.Right: return new Vector3(localPosition.x, moveMarkerHieght, localPosition.z - distance);
                    case Moves.Back: return new Vector3(localPosition.x - distance, moveMarkerHieght, localPosition.z);
                    case Moves.Left: return new Vector3(localPosition.x, moveMarkerHieght, localPosition.z + distance);
                }
                break;
            case Moves.Back:
                switch (skill.lastDirection)
                {
                    case Moves.Forward: return new Vector3(localPosition.x, moveMarkerHieght, localPosition.z - distance);
                    case Moves.Right: return new Vector3(localPosition.x - distance, moveMarkerHieght, localPosition.z);
                    case Moves.Back: return new Vector3(localPosition.x, moveMarkerHieght, localPosition.z + distance);
                    case Moves.Left: return new Vector3(localPosition.x + distance, moveMarkerHieght, localPosition.z);
                }
                break;
            case Moves.Left:
                switch (skill.lastDirection)
                {
                    case Moves.Forward: return new Vector3(localPosition.x - distance, moveMarkerHieght, localPosition.z);
                    case Moves.Right: return new Vector3(localPosition.x, moveMarkerHieght, localPosition.z + distance);
                    case Moves.Back: return new Vector3(localPosition.x + distance, moveMarkerHieght, localPosition.z);
                    case Moves.Left: return new Vector3(localPosition.x, moveMarkerHieght, localPosition.z - distance);
                }
                break;
        }
        Debug.Log("ERROR: Returning local Position!!");
        return new Vector3(localPosition.x, moveMarkerHieght, localPosition.z);
    }

    // checks if player is trying to back track on moves
    public static bool CheckIfBackTrack(Moves move, List<Moves> moves)
    {
        switch (move)
        {
            case Moves.Forward:
                if (moves[moves.Count - 1] == Moves.Back)
                {
                    return true;
                }
                break;
            case Moves.Right:
                if (moves[moves.Count - 1] == Moves.Left)
                {
                    return true;
                }
                break;
            case Moves.Back:
                if (moves[moves.Count - 1] == Moves.Forward)
                {
                    return true;
                }
                break;
            case Moves.Left:
                if (moves[moves.Count - 1] == Moves.Right)
                {
                    return true;
                }
                break;
        }
        return false;
    }

    //updates the players direction to move (locally) based on current and last move direction
    public static void UpdateDirection(Moves lastMove, ref Moves lastDirection)
    {
        switch (lastMove)
        {
            case Moves.Forward:
                switch (lastDirection)
                {
                    case Moves.Forward: lastDirection = Moves.Forward; return;
                    case Moves.Right: lastDirection = Moves.Right; return;
                    case Moves.Back: lastDirection = Moves.Back; return;
                    case Moves.Left: lastDirection = Moves.Left; return;
                }
                break;
            case Moves.Right:
                switch (lastDirection)
                {
                    case Moves.Forward: lastDirection = Moves.Right; return;
                    case Moves.Right: lastDirection = Moves.Back; return;
                    case Moves.Back: lastDirection = Moves.Left; return;
                    case Moves.Left: lastDirection = Moves.Forward; return;
                }
                break;
            case Moves.Back:
                switch (lastDirection)
                {
                    case Moves.Forward: lastDirection = Moves.Back; return;
                    case Moves.Right: lastDirection = Moves.Left; return;
                    case Moves.Back: lastDirection = Moves.Forward; return;
                    case Moves.Left: lastDirection = Moves.Right; return;
                }
                break;
            case Moves.Left:
                switch (lastDirection)
                {
                    case Moves.Forward: lastDirection = Moves.Left; return;
                    case Moves.Right: lastDirection = Moves.Forward; return;
                    case Moves.Back: lastDirection = Moves.Right; return;
                    case Moves.Left: lastDirection = Moves.Back; return;
                }
                break;
        }
    }

    //sets the current tile the player is on as an obstacle
    public static void SetTileAsObstacle(ref TileGenerator tiles, Vector3 position, ref Vector3 previousLocationAt)
    {
        previousLocationAt = position;
        tiles.traversability[(int)position.x][(int)position.z] = TileGenerator.Traversability.PlayerOccupied;
    }

    //removes the previous tile the player was on as an obstacle
    public static void RemoveTileAsObstacle(ref TileGenerator tiles, Vector3 previousLocationAt)
    {
        tiles.traversability[(int)previousLocationAt.x][(int)previousLocationAt.z] = TileGenerator.Traversability.Walkable;
    }

    //updating the last direction the player moved based on the most moves made in a single direction
    public static Moves LargestDirection(Skill skill)
    {
        int largestDirection = 0;

        for (int i = 0; i < skill.currentDirections.Count; i++)
        {
            if (skill.currentDirections[i] > largestDirection) largestDirection = i;

            Debug.Log("Direction: " + i + " = " + skill.currentDirections[i]);
        }
        switch (largestDirection)
        {
            case 0: return Moves.Forward;
            case 1: return Moves.Right; 
            case 2: return Moves.Back;
            case 3: return Moves.Left; 
            default: return  Moves.Forward; 
        }
    }
}

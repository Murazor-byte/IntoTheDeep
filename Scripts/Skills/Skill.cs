using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Skill : MonoBehaviour
{
    //what type the skill is
    public enum SkillType
    {
        Movement, Direct, Sweep, Cone, Area, Line, Self
    }

    public bool limitedUses;
    protected int numberUses;
    protected int skillDamage;

    public Character character { get; protected set; }
    protected PlayerCombatMovement playerMovement;
    protected List<GameObject> skillDistanceMarkers = new List<GameObject>();
    public SkillType skillType { get; protected set; }
    protected int skillDistance;
    public List<Vector3> selectedAttackTiles { get; protected set; }                             //tiles selected by an AOE or line ability
    public Dictionary<Vector3, Character> selectedCharacters;                                    //enemies selected by the selected attacks vector3 list

    public CombatMovement.Moves lastDirection = CombatMovement.Moves.Forward;                    //last direction the player moved
    public List<int> currentDirections = new List<int>() { 0, 0, 0, 0 };
    public List<GameObject> moveMarkers = new List<GameObject>();
    public List<Vector3> moveLocations = new List<Vector3>();                                    //locations of player current moves
    public List<CombatMovement.Moves> moves = new List<CombatMovement.Moves>();                  //total moves made by player, excluding backtracks
    public Vector3 currentLineMove;                                                              //tracks players line attack
    public Vector3 currentConeMove;                                                              //tracks players cone attacks
    public Vector3 localPos;
    public int movesMade;

    public List<bool> singelEnemySelected = new List<bool>();       //tracks when an enemy has been selected by an attackMarker
    public Enemy singleEnemySelected;                               //references a single character that the last CombatMarker selected

    //default when holding skill types in list
    public Skill() { }

    //when setting up skill given character and utilize everything from combat room
    public virtual void SetSkillReferences(Character character, PlayerCombatMovement playerMovement)
    {
        this.character = character;
        this.playerMovement = playerMovement;
        lastDirection = CombatMovement.Moves.Forward;       //reset after each encounter
        movesMade = 0;
        moveLocations.Clear();
        moveMarkers.Clear();
        moves.Clear();
        currentDirections = new List<int>() { 0, 0, 0, 0 };
        selectedAttackTiles = new List<Vector3>();
        selectedCharacters = new Dictionary<Vector3, Character>();
    }

    public abstract void UseSkill();
    public virtual void ContinueSkill() { Debug.Log("Continuing the Skill within the main skill class."); }
    public abstract void SetSkillDistance();
    public virtual Texture GetSkillAsset() { return SkillAssets.Instance.emptySlot; }
    public virtual void Attack(Character enemyToAttack) { Debug.Log("Attacking the Character"); }

    //deal direct damage based on the skills damage output and enemyToAttack armor
    protected void DealDirectDamage(Character enemyToAttack, int incomingDamage, bool armorReduction)
    {
        if(armorReduction)
            incomingDamage -= ArmorReductionDamage(enemyToAttack.armor, incomingDamage);

        enemyToAttack.TakeDamage(incomingDamage, character, playerMovement.combatManager);
    }

    //determines how much damage is taken with armor reduction
    protected int ArmorReductionDamage(int armor, int damage)
    {
        double damageReduction;
        switch (armor)
        {
            case int enemyArmor when (enemyArmor <= 0):
                Debug.Log("Enemy has a 0% damage reduction");
                damageReduction = 0;
                break;
            case int enemyArmor when (enemyArmor >= 50):
                Debug.Log("Enemy has a 50% damage reduction");
                damageReduction = damage * 0.5;
                break;
            default:
                Debug.Log("Enemy has a " + armor + "% damage reduction");
                damageReduction = damage * ((double)armor / 100);
                break;
        }
        return (int)Math.Round(damageReduction);
    }

    public void DestroySkillDistanceMarkers()
    {
        foreach (GameObject marker in skillDistanceMarkers) Destroy(marker);
        skillDistanceMarkers.Clear();
    }

    /***********SETTING COMBAT MOVEMENT BASED ON CURRENTLY SELECTED SKILL***************/

    //based on what the current Skill type is, use that movement type for selecting its position
    public void UpdateSkillMovement()
    {
        switch (skillType)
        {
            case SkillType.Direct: DirectAttack(); break;
            case SkillType.Movement: PlayerMovement(); break;
            case SkillType.Line: LineAttack(); break;
            case SkillType.Sweep: SweepAttack(); break;
            case SkillType.Cone: ConeAttack(); break;
            case SkillType.Self: break;
            default: DirectAttack(); break;
        }
    }

    //for moving the player only
    private void PlayerMovement()
    {
        GameObject moveMarker;

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && playerMovement.player.speed > 0)
        {
            if (moves.Count == 0 && movesMade < playerMovement.player.speed && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Forward, playerMovement.player) != TileGenerator.Traversability.Obstacle
                && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Forward, playerMovement.player) != TileGenerator.Traversability.EnemyOccupied)
            {
                movesMade++;
                moves.Add(CombatMovement.Moves.Forward);
                moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Forward, playerMovement.player, this, CombatMovement.localDistance));
                moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Move_Marker"), moveLocations[0], Quaternion.identity) as GameObject;
                moveMarkers.Add(moveMarker);
            }
            else if (moves.Count > 0 && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Forward, playerMovement.player) != TileGenerator.Traversability.Obstacle
                && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Forward, playerMovement.player) != TileGenerator.Traversability.EnemyOccupied)
            {
                if (moves.Count > 0 && CombatMovement.CheckIfBackTrack(CombatMovement.Moves.Forward, moves))
                {
                    CombatMovement.BackTrack(this, false);
                    movesMade--;
                }
                else if (movesMade < playerMovement.player.speed && !moveLocations.Contains(CombatMovement.LocalPosition(CombatMovement.Moves.Forward, playerMovement.player, this, CombatMovement.localDistance)))
                {
                    movesMade++;
                    moves.Add(CombatMovement.Moves.Forward);
                    moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Forward, playerMovement.player, this, CombatMovement.localDistance));
                    moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Move_Marker"), moveLocations[moveLocations.Count - 1], Quaternion.identity) as GameObject;
                    moveMarkers.Add(moveMarker);
                }
            }
        }
        else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && playerMovement.player.speed > 0)
        {
            if (moves.Count == 0 && movesMade < playerMovement.player.speed && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Right, playerMovement.player) != TileGenerator.Traversability.Obstacle
                && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Right, playerMovement.player) != TileGenerator.Traversability.EnemyOccupied)
            {
                movesMade++;
                moves.Add(CombatMovement.Moves.Right);
                moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Right, playerMovement.player, this, CombatMovement.localDistance));
                moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Move_Marker"), moveLocations[0], Quaternion.identity) as GameObject;
                moveMarkers.Add(moveMarker);
            }
            else if (moves.Count > 0 && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Right, playerMovement.player) != TileGenerator.Traversability.Obstacle
                && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Right, playerMovement.player) != TileGenerator.Traversability.EnemyOccupied)
            {
                if (moves.Count > 0 && CombatMovement.CheckIfBackTrack(CombatMovement.Moves.Right, moves))
                {
                    CombatMovement.BackTrack(this, false);
                    movesMade--;
                }
                else if (movesMade < playerMovement.player.speed && !moveLocations.Contains(CombatMovement.LocalPosition(CombatMovement.Moves.Right, playerMovement.player, this, CombatMovement.localDistance)))
                {
                    movesMade++;
                    moves.Add(CombatMovement.Moves.Right);
                    moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Right, playerMovement.player, this, CombatMovement.localDistance));
                    moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Move_Marker"), moveLocations[moveLocations.Count - 1], Quaternion.identity) as GameObject;
                    moveMarkers.Add(moveMarker);
                }
            }
        }
        else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && playerMovement.player.speed > 0)
        {
            if (moves.Count == 0 && movesMade < playerMovement.player.speed && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Back, playerMovement.player) != TileGenerator.Traversability.Obstacle
                && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Back, playerMovement.player) != TileGenerator.Traversability.EnemyOccupied)
            {
                movesMade++;
                moves.Add(CombatMovement.Moves.Back);
                moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Back, playerMovement.player, this, CombatMovement.localDistance));
                moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Move_Marker"), moveLocations[0], Quaternion.identity) as GameObject;
                moveMarkers.Add(moveMarker);
            }
            else if (moves.Count > 0 && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Back, playerMovement.player) != TileGenerator.Traversability.Obstacle
                && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Back, playerMovement.player) != TileGenerator.Traversability.EnemyOccupied)
            {
                if (moves.Count > 0 && CombatMovement.CheckIfBackTrack(CombatMovement.Moves.Back, moves))
                {
                    CombatMovement.BackTrack(this, false);
                    movesMade--;
                }
                else if (movesMade < playerMovement.player.speed && !moveLocations.Contains(CombatMovement.LocalPosition(CombatMovement.Moves.Back, playerMovement.player, this, CombatMovement.localDistance)))
                {
                    movesMade++;
                    moves.Add(CombatMovement.Moves.Back);
                    moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Back, playerMovement.player, this, CombatMovement.localDistance));
                    moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Move_Marker"), moveLocations[moveLocations.Count - 1], Quaternion.identity) as GameObject;
                    moveMarkers.Add(moveMarker);
                }
            }
        }
        else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && playerMovement.player.speed > 0)
        {
            if (moves.Count == 0 && movesMade < playerMovement.player.speed && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Left, playerMovement.player) != TileGenerator.Traversability.Obstacle
                && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Left, playerMovement.player) != TileGenerator.Traversability.EnemyOccupied)
            {
                movesMade++;
                moves.Add(CombatMovement.Moves.Left);
                moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Left, playerMovement.player, this, CombatMovement.localDistance));
                moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Move_Marker"), moveLocations[0], Quaternion.identity) as GameObject;
                moveMarkers.Add(moveMarker);
            }
            else if (moves.Count > 0 && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Left, playerMovement.player) != TileGenerator.Traversability.Obstacle
                && CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Left, playerMovement.player) != TileGenerator.Traversability.EnemyOccupied)
            {
                if (moves.Count > 0 && CombatMovement.CheckIfBackTrack(CombatMovement.Moves.Left, moves))
                {
                    CombatMovement.BackTrack(this, false);
                    movesMade--;
                }
                else if (movesMade < playerMovement.player.speed && !moveLocations.Contains(CombatMovement.LocalPosition(CombatMovement.Moves.Left, playerMovement.player, this, CombatMovement.localDistance)))
                {
                    movesMade++;
                    moves.Add(CombatMovement.Moves.Left);
                    moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Left, playerMovement.player, this, CombatMovement.localDistance));
                    moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Move_Marker"), moveLocations[moveLocations.Count - 1], Quaternion.identity) as GameObject;
                    moveMarkers.Add(moveMarker);
                }
            }
        }
    }

    //for a single target attack from the character
    private void DirectAttack()
    {
        GameObject moveMarker;

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && CombatMovement.DistanceFromPlayer(CombatMovement.LocalPosition(CombatMovement.Moves.Forward, character, this, CombatMovement.localDistance), character) <= skillDistance)
        {
            if (moves.Count == 0 && CombatMovement.LocalTileType(CombatMovement.Moves.Forward, ref playerMovement, character, this) != TileGenerator.TileType.Wall)
            {
                moves.Add(CombatMovement.Moves.Forward);
                moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Forward, character, this, CombatMovement.localDistance));
                singelEnemySelected.Add(false);
                moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Attack_Marker"), moveLocations[0], Quaternion.identity) as GameObject;
                moveMarkers.Add(moveMarker);
            }
            else if (moves.Count > 0 && CombatMovement.LocalTileType(CombatMovement.Moves.Forward, ref playerMovement, character, this) != TileGenerator.TileType.Wall)
            {
                if (moves.Count > 0 && CombatMovement.CheckIfBackTrack(CombatMovement.Moves.Forward, moves))
                {
                    CombatMovement.BackTrack(this, true);
                }
                else if (!moveLocations.Contains(CombatMovement.LocalPosition(CombatMovement.Moves.Forward, character, this, CombatMovement.localDistance)))
                {
                    moves.Add(CombatMovement.Moves.Forward);
                    moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Forward, character, this, CombatMovement.localDistance));
                    singelEnemySelected.Add(false);
                    moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Attack_Marker"), moveLocations[moveLocations.Count - 1], Quaternion.identity) as GameObject;
                    moveMarkers.Add(moveMarker);
                }
            }
        }
        else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && CombatMovement.DistanceFromPlayer(CombatMovement.LocalPosition(CombatMovement.Moves.Right, character, this, CombatMovement.localDistance), character) <= skillDistance)
        {
            if (moves.Count == 0 && CombatMovement.LocalTileType(CombatMovement.Moves.Right, ref playerMovement, character, this) != TileGenerator.TileType.Wall)
            {
                moves.Add(CombatMovement.Moves.Right);
                moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Right, character, this, CombatMovement.localDistance));
                singelEnemySelected.Add(false);
                moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Attack_Marker"), moveLocations[0], Quaternion.identity) as GameObject;
                moveMarkers.Add(moveMarker);
            }
            else if (moves.Count > 0 && CombatMovement.LocalTileType(CombatMovement.Moves.Right, ref playerMovement, character, this) != TileGenerator.TileType.Wall)
            {
                if (moves.Count > 0 && CombatMovement.CheckIfBackTrack(CombatMovement.Moves.Right, moves))
                {
                    CombatMovement.BackTrack(this, true);
                }
                else if (!moveLocations.Contains(CombatMovement.LocalPosition(CombatMovement.Moves.Right, character, this, CombatMovement.localDistance)))
                {
                    moves.Add(CombatMovement.Moves.Right);
                    moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Right, character, this, CombatMovement.localDistance));
                    singelEnemySelected.Add(false);
                    moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Attack_Marker"), moveLocations[moveLocations.Count - 1], Quaternion.identity) as GameObject;
                    moveMarkers.Add(moveMarker);
                }
            }
        }
        else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && CombatMovement.DistanceFromPlayer(CombatMovement.LocalPosition(CombatMovement.Moves.Back, character, this, CombatMovement.localDistance), character) <= skillDistance)
        {
            if (moves.Count == 0 && CombatMovement.LocalTileType(CombatMovement.Moves.Back, ref playerMovement, character, this) != TileGenerator.TileType.Wall)
            {
                moves.Add(CombatMovement.Moves.Back);
                moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Back, character, this, CombatMovement.localDistance));
                singelEnemySelected.Add(false);
                moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Attack_Marker"), moveLocations[0], Quaternion.identity) as GameObject;
                moveMarkers.Add(moveMarker);
            }
            else if (moves.Count > 0 && CombatMovement.LocalTileType(CombatMovement.Moves.Back, ref playerMovement, character, this) != TileGenerator.TileType.Wall)
            {
                if (moves.Count > 0 && CombatMovement.CheckIfBackTrack(CombatMovement.Moves.Back, moves))
                {
                    CombatMovement.BackTrack(this, true);
                }
                else if (!moveLocations.Contains(CombatMovement.LocalPosition(CombatMovement.Moves.Back, character, this, CombatMovement.localDistance)))
                {
                    moves.Add(CombatMovement.Moves.Back);
                    moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Back, character, this, CombatMovement.localDistance));
                    singelEnemySelected.Add(false);
                    moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Attack_Marker"), moveLocations[moveLocations.Count - 1], Quaternion.identity) as GameObject;
                    moveMarkers.Add(moveMarker);
                }
            }
        }
        else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && CombatMovement.DistanceFromPlayer(CombatMovement.LocalPosition(CombatMovement.Moves.Left, character, this, CombatMovement.localDistance), character) <= skillDistance)
        {
            if (moves.Count == 0 && CombatMovement.LocalTileType(CombatMovement.Moves.Left, ref playerMovement, character, this) != TileGenerator.TileType.Wall)
            {
                moves.Add(CombatMovement.Moves.Left);
                moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Left, character, this, CombatMovement.localDistance));
                singelEnemySelected.Add(false);
                moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Attack_Marker"), moveLocations[0], Quaternion.identity) as GameObject;
                moveMarkers.Add(moveMarker);
            }
            else if (moves.Count > 0 && CombatMovement.LocalTileType(CombatMovement.Moves.Left, ref playerMovement, character, this) != TileGenerator.TileType.Wall)
            {
                if (moves.Count > 0 && CombatMovement.CheckIfBackTrack(CombatMovement.Moves.Left, moves))
                {
                    CombatMovement.BackTrack(this, true);
                }
                else if (!moveLocations.Contains(CombatMovement.LocalPosition(CombatMovement.Moves.Left, character, this, CombatMovement.localDistance)))
                {
                    moves.Add(CombatMovement.Moves.Left);
                    moveLocations.Add(CombatMovement.LocalPosition(CombatMovement.Moves.Left, character, this, CombatMovement.localDistance));
                    singelEnemySelected.Add(false);
                    moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Attack_Marker"), moveLocations[moveLocations.Count - 1], Quaternion.identity) as GameObject;
                    moveMarkers.Add(moveMarker);
                }
            }
        }
    }

    private void LineAttack()
    {
        Vector3 localPosition = Vector3.zero;

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && CombatMovement.DistanceFromPlayer(CombatMovement.LocalPosition(CombatMovement.Moves.Forward, character, this, CombatMovement.localDistance), character) <= skillDistance)
        {
            if (CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Forward, playerMovement.player) != TileGenerator.Traversability.Obstacle)
            {
                localPosition = CombatMovement.LocalPosition(CombatMovement.Moves.Forward, character, this, CombatMovement.localDistance);
                currentDirections[0]++; currentDirections[2]--;
            }
        }
        else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && CombatMovement.DistanceFromPlayer(CombatMovement.LocalPosition(CombatMovement.Moves.Right, character, this, CombatMovement.localDistance), character) <= skillDistance)
        {
            if (CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Right, playerMovement.player) != TileGenerator.Traversability.Obstacle)
            {
                localPosition = CombatMovement.LocalPosition(CombatMovement.Moves.Right, character, this, CombatMovement.localDistance);
                currentDirections[1]++; currentDirections[3]--;
            }
        }
        else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && CombatMovement.DistanceFromPlayer(CombatMovement.LocalPosition(CombatMovement.Moves.Back, character, this, CombatMovement.localDistance), character) <= skillDistance)
        {
            if (CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Back, playerMovement.player) != TileGenerator.Traversability.Obstacle)
            {
                localPosition = CombatMovement.LocalPosition(CombatMovement.Moves.Back, character, this, CombatMovement.localDistance);
                currentDirections[2]++; currentDirections[0]--;
            }
        }
        else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && CombatMovement.DistanceFromPlayer(CombatMovement.LocalPosition(CombatMovement.Moves.Left, character, this, CombatMovement.localDistance), character) <= skillDistance)
        {
            if (CombatMovement.LocalTile(ref playerMovement, this, CombatMovement.Moves.Left, playerMovement.player) != TileGenerator.Traversability.Obstacle)
            {
                localPosition = CombatMovement.LocalPosition(CombatMovement.Moves.Left, character, this, CombatMovement.localDistance);
                currentDirections[3]++; currentDirections[1]--;
            }
        }

        if (moveLocations.Contains(localPosition))
        {
            //changes the old locations color to skill distance color
            ResetSkillDistanceMarkers();
            selectedAttackTiles.Clear();
            selectedCharacters.Clear();
            //changes the new locations color to attack color
            skillDistanceMarkers[moveLocations.IndexOf(localPosition)].GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Red_Transparent");
            //updates the current line position
            currentLineMove = localPosition;

            Vector3 position = new Vector3(character.transform.position.x, CombatMovement.moveMarkerHieght, character.transform.position.z);
            Vector3 direction = currentLineMove - position;
            Debug.DrawRay(new Vector3(character.transform.position.x, CombatMovement.moveMarkerHieght, character.transform.position.z), direction, Color.green, 5f, false);

            //make all skill distance markers orignal color except currentLineMarker before casting ray
            RaycastHit[] hits;
            Ray ray = new Ray(position, direction.normalized);
            hits = Physics.RaycastAll(ray, CombatMovement.DistanceFromPlayer(currentLineMove, character));
            foreach (RaycastHit hit in hits)
            {

                if (hit.transform.CompareTag("Combat Marker"))
                {
                    hit.transform.gameObject.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Red_Transparent");
                    selectedAttackTiles.Add(hit.transform.position);
                    selectedCharacters.Add(hit.transform.position, null);
                }
            }
            //Orders the raycast hits in order based on distance from player (since the ordering is undefined)
            selectedAttackTiles.Sort((x, z) => { return Vector3.Distance(x, character.transform.position).CompareTo(Vector3.Distance(z, character.transform.position)); });
            if (!selectedAttackTiles.Contains(currentLineMove))
            {
                selectedAttackTiles.Add(currentLineMove);
                selectedCharacters.Add(currentLineMove, null);
            }
            FindEnemiesInPath();
        }
    }

    /* For Sweep attack
             * player can only select adjacent tiles
             * keep track of last tile selected (so if the range is max 2, but they selected a range of 1)
             * create attack marker tiles to the side of the player as far as the last tile selected was
             * then:
             *      create a line straight out from the player + 1
             *      starting from the farthest straight tile: create an attack marker tile on either side based on the 'counter' 
             *              with the counter starting at 0 for the farthest tile and incrementing by one for each tile closer to the player
             *          
             *      EX: if the player targetted a tile 2 tiles away, create a straight line 3 tiles out (1 farther than what they selected)
             *              the farthest tile has no adjacent attack tiles next to it
             *              the second straight tile has 1 attack tile on either side of it
             *              the first straight tile has 2 attack tiles on either side of it
             *              and the number of the players immediate adjacent tiles will be the same as the first straight tile (2 in this case)
            */
    private void SweepAttack()
    {
        //find direction of sweep
        bool moved = false;

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && moveLocations.Contains(CombatMovement.LocalPosition(CombatMovement.Moves.Forward, character, this, CombatMovement.localDistance)))
        {
            currentConeMove = CombatMovement.LocalPosition(CombatMovement.Moves.Forward, character, this, CombatMovement.localDistance);
            currentDirections[0]++; currentDirections[2]--;
            moved = true;
        }
        else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && moveLocations.Contains(CombatMovement.LocalPosition(CombatMovement.Moves.Right, character, this, CombatMovement.localDistance)))
        {
            currentConeMove = CombatMovement.LocalPosition(CombatMovement.Moves.Right, character, this, CombatMovement.localDistance);
            currentDirections[1]++; currentDirections[3]--;
            moved = true;
        }
        else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && moveLocations.Contains(CombatMovement.LocalPosition(CombatMovement.Moves.Back, character, this, CombatMovement.localDistance)))
        {
            currentConeMove = CombatMovement.LocalPosition(CombatMovement.Moves.Back, character, this, CombatMovement.localDistance);
            currentDirections[2]++; currentDirections[0]--;
            moved = true;
        }
        else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && moveLocations.Contains(CombatMovement.LocalPosition(CombatMovement.Moves.Left, character, this, CombatMovement.localDistance)))
        {
            currentConeMove = CombatMovement.LocalPosition(CombatMovement.Moves.Left, character, this, CombatMovement.localDistance);
            currentDirections[3]++; currentDirections[1]--;
            moved = true;
        }


        //if back on the character tile don't do anything
        if(moved && currentConeMove == new Vector3(character.transform.position.x, localPos.y, character.transform.position.z))
            CombatMovement.ClearMovement(ref character.currentSkill, true, false);


        if(moved && moveLocations.Contains(currentConeMove))
        {
            localPos = currentConeMove;                     //save the players currentConeMove and directions made before clearing it
            List<int> directionMoved = currentDirections;
            CombatMovement.ClearMovement(ref character.currentSkill, true, false);
            currentDirections = directionMoved;
            currentConeMove = localPos;

            CombatMovement.Moves direction = CombatMovement.LargestDirection(this);

            //if going one extra tile farther is valid
            Vector3 farther = CombatMovement.LocalPosition(direction, character, this, CombatMovement.localDistance);
            if (currentConeMove != new Vector3(character.transform.position.x, localPos.y, character.transform.position.z) && playerMovement.tiles.tiles[(int)farther.x][(int)farther.z] != TileGenerator.TileType.Wall)
                localPos = CombatMovement.LocalPosition(direction, character, this, CombatMovement.localDistance);

            int width = 0;
            int sweepLength =  CombatMovement.DistanceFromPlayer(localPos, character);

            GameObject moveMarker;
            for(int i = sweepLength; i >= 0; i--)
            {
                //don't spawn an attack tile on the characters position
                if (localPos != new Vector3(character.transform.position.x, localPos.y, character.transform.position.z))
                {
                    moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Attack_Marker"), localPos, Quaternion.identity) as GameObject;
                    moveMarkers.Add(moveMarker);
                }

                Vector3 leftSideSweep = localPos; 
                Vector3 rightSideSweep = localPos;
                bool continueLeft = true;                   //if we should keep spawning attack markers to either side of sweep, stopping if there's a wall
                bool continueRight = true;

                for(int j = 1; j < width + 1; j++)
                {
                    switch (direction)
                    {
                        case CombatMovement.Moves.Back:
                        case CombatMovement.Moves.Forward:
                            if (continueLeft)
                            {
                                leftSideSweep = CombatMovement.LocalPosition(CombatMovement.Moves.Left, character, this, j);
                                continueLeft = AOEReachesWall(leftSideSweep);
                            }
                            if (continueRight){
                                rightSideSweep = CombatMovement.LocalPosition(CombatMovement.Moves.Right, character, this, j);
                                continueRight = AOEReachesWall(rightSideSweep);
                            }
                            break;
                        case CombatMovement.Moves.Left:
                        case CombatMovement.Moves.Right:
                            if (continueLeft)
                            {
                                leftSideSweep = CombatMovement.LocalPosition(CombatMovement.Moves.Forward, character, this, j);
                                continueLeft = AOEReachesWall(leftSideSweep);
                            }
                            if (continueRight)
                            {
                                rightSideSweep = CombatMovement.LocalPosition(CombatMovement.Moves.Back, character, this, j);
                                continueRight = AOEReachesWall(rightSideSweep);
                            }
                            break;
                    }
                }

                //update the middle tile to move closer to player every iteration
                switch (direction)
                {
                    case CombatMovement.Moves.Forward: localPos = CombatMovement.LocalPosition(CombatMovement.Moves.Back, character, this, CombatMovement.localDistance); break;
                    case CombatMovement.Moves.Right: localPos = CombatMovement.LocalPosition(CombatMovement.Moves.Left, character, this, CombatMovement.localDistance); break;
                    case CombatMovement.Moves.Back: localPos = CombatMovement.LocalPosition(CombatMovement.Moves.Forward, character, this, CombatMovement.localDistance); break;
                    case CombatMovement.Moves.Left: localPos = CombatMovement.LocalPosition(CombatMovement.Moves.Right, character, this, CombatMovement.localDistance); break;
                }

                //don't sweep out so far to characters immediate sides
                if (i != 1) width++;
            }
            localPos = currentConeMove;
        }
    }

    //SHOULD BE COMPLETE OPPOSITE OF SWEEP ATTACK
    private void ConeAttack()
    {

    }


    /*SETTING VISUAL TILE RANGE BASED ON SKILL DISTANCE*/

    //What tiles the skill range can be used at, and instantiates them at their positions
    protected void SkillDistance()
    {
        //skill distance is created using an Archimedean spiral
        if (skillType == SkillType.Direct || skillType == SkillType.Movement || skillType == SkillType.Line)
        {
            List<CombatMovement.Moves> moves = new List<CombatMovement.Moves> { CombatMovement.Moves.Back };
            Vector3 currentPosition = new Vector3(character.transform.position.x, CombatMovement.moveMarkerHieght, character.transform.position.z);
            int currentSet = 0;             //how far each line goes in a direction
            int setIterations = 0;          //every 2 iterations increment currentSet by 1
            int numberFails = 0;

            GameObject marker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Skill_Marker"), currentPosition, Quaternion.identity) as GameObject;
            skillDistanceMarkers.Add(marker);
            if (skillType == SkillType.Line)
            {
                moveLocations.Add(marker.transform.position);
                skillDistanceMarkers[0].GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Red_Transparent");
            }
            while (true)
            {
                numberFails = 0;
                //increase distance of set every 2 iterations
                if (setIterations % 2 == 0)
                {
                    currentSet++;
                    setIterations = 0;
                }

                //update direction
                switch (moves[moves.Count - 1])
                {
                    case CombatMovement.Moves.Forward: moves.Add(CombatMovement.Moves.Left); break;
                    case CombatMovement.Moves.Right: moves.Add(CombatMovement.Moves.Forward); break;
                    case CombatMovement.Moves.Back: moves.Add(CombatMovement.Moves.Right); break;
                    case CombatMovement.Moves.Left: moves.Add(CombatMovement.Moves.Back); break;
                }

                setIterations++;

                //go through the whole current set in the correct direction then repeat
                for (int i = 0; i < currentSet; i++)
                {
                    //increment the same direction
                    switch (moves[moves.Count - 1])
                    {
                        case CombatMovement.Moves.Forward: currentPosition = new Vector3(currentPosition.x, CombatMovement.moveMarkerHieght, currentPosition.z + 1); break;
                        case CombatMovement.Moves.Right: currentPosition = new Vector3(currentPosition.x + 1, CombatMovement.moveMarkerHieght, currentPosition.z); break;
                        case CombatMovement.Moves.Back: currentPosition = new Vector3(currentPosition.x, CombatMovement.moveMarkerHieght, currentPosition.z - 1); break;
                        case CombatMovement.Moves.Left: currentPosition = new Vector3(currentPosition.x - 1, CombatMovement.moveMarkerHieght, currentPosition.z); break;
                    }
                    //stop if out of range
                    if (CombatMovement.DistanceFromPlayer(currentPosition, character) > skillDistance || currentPosition.x < 0 || currentPosition.z < 0)
                    {
                        numberFails++;
                        if (numberFails >= currentSet) return;
                    }
                    else
                    {
                        //place the marker if it's within the bounds of the rooms
                        if (playerMovement.tiles.tileFinder.openPositions.Contains(new Vector3(currentPosition.x, 0, currentPosition.z)))
                        {
                            marker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Skill_Marker"), currentPosition, Quaternion.identity) as GameObject;
                            skillDistanceMarkers.Add(marker);
                            if(skillType == SkillType.Line) moveLocations.Add(marker.transform.position);
                        }
                        else if(!playerMovement.tiles.tileFinder.openPositions.Contains(new Vector3(currentPosition.x, 0, currentPosition.z)))
                        {
                            Debug.Log("TileFinder open positions doesn't contain: " + new Vector3(currentPosition.x, 0, currentPosition.z));
                        }
                    }
                }
            }
        }

        //skill distance markers are only placed horizationlly in all directions, player can't do cone/sweep attacks diagonally
            //since this will make diagonal sqaured unbalanced, and they're already targeted with horizaontal selection
        else if(skillType == SkillType.Cone || skillType == SkillType.Sweep)
        {
            GameObject marker;
            Vector3 characterPos = character.transform.position;
            Vector3 currentPosition;
            TileGenerator.Traversability[][] traverse = playerMovement.tiles.traversability;
            bool[] continueDirection = new bool[4]{ true, true, true, true};    //if we have not encounter an obstacle in this direction

            //spawn a skill marker on the character (helps with overall movement, but they can't really attack at this location)
            marker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Skill_Marker"), new Vector3(characterPos.x, CombatMovement.moveMarkerHieght, characterPos.z), Quaternion.identity) as GameObject;
            moveLocations.Add(marker.transform.position);
            skillDistanceMarkers.Add(marker);

            for (int i = 1; i <= skillDistance; i++)
            {
                if (continueDirection[0] == true && traverse[(int)characterPos.x + i][(int)characterPos.z] != TileGenerator.Traversability.Obstacle)
                {
                    currentPosition = new Vector3(characterPos.x + i, CombatMovement.moveMarkerHieght, characterPos.z);
                    marker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Skill_Marker"), currentPosition, Quaternion.identity) as GameObject;
                    moveLocations.Add(marker.transform.position);
                    skillDistanceMarkers.Add(marker);
                }
                else continueDirection[0] = false;

                if (continueDirection[1] == true && traverse[(int)characterPos.x][(int)characterPos.z + i] != TileGenerator.Traversability.Obstacle)
                {
                    currentPosition = new Vector3(characterPos.x, CombatMovement.moveMarkerHieght, characterPos.z + i);
                    marker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Skill_Marker"), currentPosition, Quaternion.identity) as GameObject;
                    moveLocations.Add(marker.transform.position);
                    skillDistanceMarkers.Add(marker);
                }
                else continueDirection[1] = false;

                if (continueDirection[2] == true && traverse[(int)characterPos.x - i][(int)characterPos.z] != TileGenerator.Traversability.Obstacle)
                {
                    currentPosition = new Vector3(characterPos.x - i, CombatMovement.moveMarkerHieght, characterPos.z);
                    marker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Skill_Marker"), currentPosition, Quaternion.identity) as GameObject;
                    moveLocations.Add(marker.transform.position);
                    skillDistanceMarkers.Add(marker);
                }
                else continueDirection[2] = false;

                if (continueDirection[3] == true && traverse[(int)characterPos.x][(int)characterPos.z - i] != TileGenerator.Traversability.Obstacle)
                {
                    currentPosition = new Vector3(characterPos.x, CombatMovement.moveMarkerHieght, characterPos.z - i);
                    marker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Skill_Marker"), currentPosition, Quaternion.identity) as GameObject;
                    moveLocations.Add(marker.transform.position);
                    skillDistanceMarkers.Add(marker);
                }
                else continueDirection[3] = false;
            }
        }
        else if (skillType == SkillType.Area)
        {

        }
        else if(skillType == SkillType.Self)
        {
            Vector3 currentPosition = new Vector3(character.transform.position.x, CombatMovement.moveMarkerHieght, character.transform.position.z);
            GameObject moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Attack_Marker"), currentPosition, Quaternion.identity) as GameObject;
            skillDistanceMarkers.Add(moveMarker);
        }
    }




    public void ResetSkills()
    {
        moveLocations.Clear();
        moveMarkers.Clear();
        moves.Clear();
        currentDirections = new List<int>() { 0, 0, 0, 0 };
    }

    //resets all skill distance markers to be default color
    public void ResetSkillDistanceMarkers()
    {
        foreach (GameObject marker in skillDistanceMarkers)
        {
            marker.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Blue_Light_Transparent");
        }
    }

    //assigns characters to the selected (Line or Area) attack tiles
    private void FindEnemiesInPath()
    {
        foreach(Vector3 pos in selectedAttackTiles)
        {
            foreach(GameObject characterPos in playerMovement.combatManager.initiativeOrder)
            {
                Vector3 enmeyPosition = new Vector3(characterPos.transform.position.x, pos.y, characterPos.transform.position.z);
                if(enmeyPosition.x == pos.x && enmeyPosition.z == pos.z)
                {
                    selectedCharacters[pos] = characterPos.GetComponent<Character>();
                }
            }
        }
    }

    //determines if an AOE marker reaches a wall, if so stop spawning markers in that direction in the calling method
    private bool AOEReachesWall(Vector3 location)
    {
        if (playerMovement.tiles.tiles[(int)location.x][(int)location.z] != TileGenerator.TileType.Wall)
        {
            GameObject moveMarker = Instantiate(Resources.Load<GameObject>("Prefabs/Markers/Attack_Marker"), location, Quaternion.identity) as GameObject;
            moveMarkers.Add(moveMarker);
            return true;
        }
        return false;
    }
}

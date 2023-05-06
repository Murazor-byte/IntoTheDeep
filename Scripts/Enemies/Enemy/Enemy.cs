using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Enemy : Character
{
    protected enum ActionTaken
    {
        NoAction, MoveToward, Retreat, Attack, Idle
    }

    protected ActionTaken lastActionTaken = ActionTaken.NoAction;   //what action the enemy took last round

    protected int minDamage;
    protected int maxDamage;

    protected static List<Armor> helmArmorSelection;           //types of armor for this enemy to choose from
    protected static List<Armor> bodyArmorSelection;
    protected static List<Armor> greavesArmorSelection;
    protected static List<Armor> bootArmorSelection;
    protected static List<List<Armor>> allArmor;

    protected static float[] helmProb;
    protected static float[] bodyProb;
    protected static float[] greavesProb;
    protected static float[] bootsProb;
    protected static float[][] allArmorProb;

    protected List<Weapon> weaponSelection;                    //type of weapons to select from
    protected static float[] weaponProb;


    private GameObject mainCombatManager;
    protected GameObject playerObject;                         //accessing the player object

    protected CombatManager combatManager;
    protected TileGenerator tileGenerator;
    protected TileGenerator.Traversability[][] traversability; //size of the PathfindingGrid to = the rooms occupiedTiles size - MIGHT BE TOO BIG
    protected TileGenerator.TileType[][] tileType;

    protected Vector3 previousLocationAt;

    protected Player player;                                   //accessing the players stats

    protected Enemy(int minHealth, int maxHealth, int minSpeed, int maxSpeed, int combatRating) : base (minHealth, maxHealth, minSpeed, maxSpeed, combatRating) { }

    private void Awake()
    {
        SetHealth(UnityEngine.Random.Range(minHealth, maxHealth));
        healthCap = maxHealth;
        speed = UnityEngine.Random.Range(minSpeed, maxSpeed);

        mainCombatManager = GameObject.Find("CombatManager");                         //caching of componenets
        combatManager = mainCombatManager.GetComponent<CombatManager>();
        tileGenerator = combatManager.GetComponent<RoomGenerator>().tileGenerator;
        traversability = tileGenerator.traversability;

        playerObject = GameObject.Find("CombatPlayer");
        player = playerObject.GetComponent<Player>();

        //randomly select an armor piece for each armor type
        for (int i = 0; i < allArmor.Count; i++)
        {
            EquipArmor(i);
        }

        EquipWeapon();
    }

    public int GetCombatRating() { return combatRating; }

    //ends the enemies turn after their movement and attack
    protected IEnumerator EndTurn()
    {
        SetCharacterLocation();

        Debug.Log("Enemy turn has ended");
        combatManager.IncrementTurnCount();
        combatManager.NextTurn();
        yield return new WaitForFixedUpdate();
    }

    //Logic: Attack if in range, else move then attack if in range
    public virtual IEnumerator Turn()
    {
        traversability = tileGenerator.traversability;                              //set each turn since tiles can be altered during combat
        tileType = tileGenerator.tiles;

        Pathfinding pathfinding = new Pathfinding(traversability.Length, traversability.Length, tileType, traversability);

        //path from current enemy position to the player position
        List<PathNode> pathToTarget = pathfinding.FindPath((int)transform.position.x, (int)transform.position.z, (int)playerObject.transform.position.x, (int)playerObject.transform.position.z);

        if (DistanceToTarget(player) <= weaponRange)
        {
            yield return StartCoroutine(Attack(player, null));
        }
        else
        {
            yield return StartCoroutine(Move(pathToTarget, false, 0));

            if (DistanceToTarget(player) <= weaponRange)
            {
                yield return StartCoroutine(Attack(player, null));
            }
        }
        StartCoroutine(EndTurn());
    }

    //various movements made by enemy type, with reachTarget if they are trying to reach that tile
    //or just reach within its weaponRange
    public virtual IEnumerator Move(List<PathNode> pathToTarget, bool reachTarget, int spentMovement)
    {
        int moves = spentMovement;

        traversability = tileGenerator.traversability;
        tileType = tileGenerator.tiles;

        if (traversability == null) Debug.Log("Traversability was null");

        for (int i = 0; i < pathToTarget.Count; i++)
        {
            if (!reachTarget && (moves > speed || pathToTarget.Count - i == weaponRange)) break;

            if (reachTarget && moves > speed) break;

            Vector3 locationToMove = new Vector3(pathToTarget[i].x, transform.position.y, pathToTarget[i].z);

            //set rotation of the enemy
            SetRotation(locationToMove);

            transform.position = locationToMove;
            RemoveTileAsObstacle();
            SetTileAsObstacle(transform.position);
            moves++;
            yield return new WaitForSeconds(0.5f);
        }
    }

    //attacks the player when the enemy is within range
    /*public override IEnumerator Attack(Character player, CombatManager combatManager)
    {
        double damageReduction;
        int incomingDamage = damage;

        switch (player.armor)
        {
            case int armor when (armor <= 0):                     //takes flat damage w/ no armor
                Debug.Log("Player has a 0% damage reduction");
                damageReduction = 0;
                break;
            case int armor when (armor >= 50):
                Debug.Log("Player has a 50% damage reduction");
                damageReduction = damage * 0.5;
                break;
            default:
                Debug.Log("Player has a " + player.armor + "% damage reduction");
                damageReduction = damage * ((double)player.armor /100);
                break;
        }
        Debug.Log("Damage Reduction Value = " + damageReduction);
        Debug.Log("Damage Reduction after calculatoin = " + (int)Math.Round(damageReduction));
        incomingDamage -= (int)Math.Round(damageReduction);
        Debug.Log("Damage being dealt after calculation = " + incomingDamage);

        player.TakeDamage(incomingDamage, true);

        yield return new WaitForFixedUpdate();
    }*/


    //given a Equipment type parameter alter this enemies stats
    protected virtual void EquipArmor(int armorIndex)
    {
        ProbabilityGenerator selectArmorPiece = new ProbabilityGenerator(allArmorProb[armorIndex]);
        int selectedArmor = selectArmorPiece.GenerateNumber();

        switch (armorIndex)
        {
            case 0:
                helmArmor = helmArmorSelection[selectedArmor];
                armor += helmArmorSelection[selectedArmor].GetArmorValue();
                break;
            case 1:
                bodyArmor = bodyArmorSelection[selectedArmor];
                armor += bodyArmorSelection[selectedArmor].GetArmorValue();
                break;
            case 2:
                greavesArmor = greavesArmorSelection[selectedArmor];
                armor += greavesArmorSelection[selectedArmor].GetArmorValue();
                break;
            case 3:
                bootArmor = bootArmorSelection[selectedArmor];
                armor += bootArmorSelection[selectedArmor].GetArmorValue();
                break;
        }
    }

    //given a Equipment type parameter alter this enemies stats
    protected virtual void EquipWeapon()
    {
        ProbabilityGenerator selectedWeaponPiece = new ProbabilityGenerator(weaponProb);
        int selectedWeapon = selectedWeaponPiece.GenerateNumber();

        weapon = weaponSelection[selectedWeapon];

        SetDamage(weaponSelection[selectedWeapon].GetWeaponDamage());
        weaponRange = weaponSelection[selectedWeapon].GetWeaponRange();
    }

    //sets the current tile the character is on as an obstacle
    public void SetTileAsObstacle(Vector3 position)
    {
        previousLocationAt = position;
        traversability[(int)position.x][(int)position.z] = TileGenerator.Traversability.EnemyOccupied;
    }

    //removes the previous tile the character was on as an obstacle
    private void RemoveTileAsObstacle()
    {
        traversability[(int)previousLocationAt.x][(int)previousLocationAt.z] = TileGenerator.Traversability.Walkable;
    }

    //given the number of ally enemies and the distance from the current enmies pos. to them
    //determines if their are enough enemy allies within range
    protected bool AlliesInRange(int numberAllies, int distance)
    {
        int alliesInRange = 0;

        for(int i = 0; i < combatManager.enemiesInEncounter.Count; i++)
        {
            if (CalculateDistance(combatManager.enemiesInEncounter[i]) <= distance)
            {
                alliesInRange++;
            }
        }
        if (alliesInRange >= numberAllies) return true;

        return false;
    }

    //calculates number of units between this enemy and another character
    protected int CalculateDistance(GameObject character)
    {
        int xDistance = (int)Mathf.Abs(character.transform.position.x - transform.position.x);
        int zDistance = (int)Mathf.Abs(character.transform.position.z - transform.position.z);

        int xPath = (int)Mathf.Pow(xDistance, 2);
        int zPath = (int)Mathf.Pow(zDistance, 2);

        int pathDistance = (int)Mathf.Sqrt(xPath + zPath);

        return pathDistance;
    }
}

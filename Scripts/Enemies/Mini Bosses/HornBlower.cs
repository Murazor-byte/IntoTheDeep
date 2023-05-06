using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HornBlower : MiniBoss
{
    private int actionsTaken;                               //total number of actions taken this turn
    private int actionsCap = 2;                             //total number of actions to take in a turn

    private const int SAFE_DISTANCE_FROM_HERO = 9;          //How far this enemy is to feel safe from Hero threat
    private const float PROB_SEPERATE_DISTANCE = 0.45f;     //Likliness of enemy to run away from hero if within safeDistance
    private const float PROBIDLE = 0.6f;                    //likliness for enemy to idle at their current position
    private const float PROB_IDLE_PATH = 0.70f;             //likliness for enemy to idle move over litreally idling
    private const float PROB_REPEATED_ACTION = 0.6f;        //likliness to repeat the same action used last round

    private const float blowHornProb = 0.3f;                //probability to call Reinforcements given opportunity
    private const float blowHornIncrProb = 0.2f;            //incremental value added to "blowHornProb" after each turn pass "hornBlownFreq"
    private int hornBlownFreq = 7;                          //Min number of turns between each time the mini-boss calls for reinforcements
    private int turnsSinceHornBlown = 4;                    //How many turns its been since last time the horn was blown

    private int numNearAlliesToBuff = 2;                    //How many nearby allies there needs to be in order to buff
    private int maxNumAlliesToBuff = 3;                     //Max number of allies (including self) to buff within range
    private float buffProb = 0.4f;                          //prob. to buff allies when all conditions met
    private int buffFreq = 4;                               //Min number of turns bewteen each time mini-boss can buff
    private int turnsSinceBuff = 2;                         //How many turns its been since last buff used
    private int buffRange = 12;                             //Range in which the buff will be applied

    private HornBlower() : base(40, 90, 4, 7, 10)
    {
        helmArmorSelection = new List<Armor> { new LeatherHelm(), new MailHelm() };
        bodyArmorSelection = new List<Armor> { new LeatherBreastPlate(), new ChainMail() };
        greavesArmorSelection = new List<Armor> { new LeatherGreaves(), new MailGreaves() };
        bootArmorSelection = new List<Armor> { new LeatherBoots(), new MailBoots() };

        allArmor = new List<List<Armor>> { helmArmorSelection, bodyArmorSelection, greavesArmorSelection, bootArmorSelection };
        helmProb = new float[] { 0.75f, 0.25f };
        bodyProb = new float[] { 0.75f, 0.25f };
        greavesProb = new float[] { 0.75f, 0.25f };
        bootsProb = new float[] { 0.75f, 0.25f };
        allArmorProb = new float[][] { helmProb, bodyProb, greavesProb, bootsProb };

        weaponSelection = new List<Weapon> { new Club(), new MorningStar(), new Sword() };
        weaponProb = new float[] { 0.4f, 0.3f, 0.3f };
    }

    void Start()
    {
        ChangeDamageModifier(3);
    }

    public override IEnumerator Turn()
    {
        actionsTaken = 0;
        turnsSinceHornBlown++;
        turnsSinceBuff++;

        traversability = tileGenerator.traversability;      //set each turn since tiles can be altered during combat
        tileType = tileGenerator.tiles;

        Pathfinding pathfinding = new Pathfinding(traversability.Length, traversability.Length, tileType, traversability);

        float takenProb = 0f;                               //probability taken for each enemy choice

        //Deciding to call reinforcements
        if(turnsSinceHornBlown >= hornBlownFreq)
        {
            takenProb = ((turnsSinceHornBlown - hornBlownFreq) * blowHornIncrProb) + blowHornProb;
            Debug.Log("Turns since Horn Blown = " + turnsSinceHornBlown + "with probability to blow of " + takenProb);

            if(Random.value <= takenProb)
            {
                CallReinforcements();
                actionsTaken++;
                turnsSinceHornBlown = 0;
            }
        }

        //Deciding to buff nearby allies
        if(turnsSinceBuff >= buffFreq && Random.value <= buffProb && AlliesInRange(numNearAlliesToBuff, buffRange))
        {
            Debug.Log("HORN BLOWER HAS INVIGORATED ALLIES");
            BuffAllies();
            turnsSinceBuff = 0;
        }

        //STARTS TURN INSIDE SAFEDISTANCE
        if (DistanceToTarget(player) < SAFE_DISTANCE_FROM_HERO)
        {
            if (lastActionTaken == ActionTaken.Retreat)
                takenProb = (PROB_SEPERATE_DISTANCE * PROB_REPEATED_ACTION) + PROB_SEPERATE_DISTANCE;
            else if (lastActionTaken == ActionTaken.MoveToward || lastActionTaken == ActionTaken.Attack)
                takenProb = PROB_SEPERATE_DISTANCE - (PROB_SEPERATE_DISTANCE * PROB_REPEATED_ACTION);
            else takenProb = PROB_SEPERATE_DISTANCE;

            //RETREAT FROM HERO - IN SAFEDISTANCE
            if(Random.value <= takenProb)
            {
                //if Enemy is within attacking range first, attack the hero then retreat
                if (actionsTaken < actionsCap &&  DistanceToTarget(player) <= weaponRange)
                {
                    yield return StartCoroutine(Attack(player, null));
                    actionsTaken++;
                }

                lastActionTaken = ActionTaken.Retreat;
                Debug.Log("Enemy is retreating from Hero");
                List<PathNode> retreat = pathfinding.Retreat((int)transform.position.x, (int)transform.position.z, (int)playerObject.transform.position.x, (int)playerObject.transform.position.z);
                yield return StartCoroutine(Move(retreat, true, 0));
            }
            //MOVE TOWARD HERO - IN SAFEDISTNCE
            else
            {
                Debug.Log("Enemy is within safeDistance and moving toward Hero");

                if (actionsTaken < actionsCap &&  DistanceToTarget(player) <= weaponRange)
                {
                    lastActionTaken = ActionTaken.Attack;
                    yield return StartCoroutine(Attack(player, null));
                    actionsTaken++;
                }
                else
                {
                    lastActionTaken = ActionTaken.MoveToward;
                    List<PathNode> pathToHero = pathfinding.FindPath((int)transform.position.x, (int)transform.position.z, (int)playerObject.transform.position.x, (int)playerObject.transform.position.z);
                    yield return StartCoroutine(Move(pathToHero, false, 0));

                    if (actionsTaken < actionsCap &&  DistanceToTarget(player) <= weaponRange)
                    {
                        lastActionTaken = ActionTaken.Attack;
                        yield return StartCoroutine(Attack(player, null));
                        actionsTaken++;
                    }
                }
            }
        }
        //STARTS TURN OUTSIDE SAFEDISTANCE
        else
        {
            if (lastActionTaken == ActionTaken.Idle)
                takenProb = (PROBIDLE * PROB_REPEATED_ACTION) + PROBIDLE;
            else if (lastActionTaken == ActionTaken.MoveToward)
                takenProb = PROBIDLE - (PROBIDLE * PROB_REPEATED_ACTION);
            else
                takenProb = PROBIDLE;

            if(Random.value <= takenProb)
            {
                //Make the the enemy move a few sqaures in a random(safe) direction
                if (Random.value <= PROB_IDLE_PATH)
                {
                    //attack hero if within range first
                    if (actionsTaken < actionsCap &&  DistanceToTarget(player) <= weaponRange)
                    {
                        yield return StartCoroutine(Attack(player, null));
                        actionsTaken++;
                    }

                    lastActionTaken = ActionTaken.Idle;
                    Debug.Log("Enemy is idle moving");
                    int idleDistance = Random.Range(1, maxSpeed / 2);
                    List<PathNode> idlePath = pathfinding.FindIdlePath((int)transform.position.x, (int)transform.position.z, idleDistance);
                    yield return StartCoroutine(Move(idlePath, true, 0));
                }
                //Make enemy stay at their position
                else
                {
                    lastActionTaken = ActionTaken.MoveToward;
                    Debug.Log("Enemy is idling at their position");
                    yield return null;
                }
                
            }
            //MOVE TOWARD HERO - OUTSIDE SAFEDISTANCE
            else
            {
                Debug.Log("Enemy is outside of safeDistance and moving toward Hero");

                if (actionsTaken < actionsCap &&  DistanceToTarget(player) <= weaponRange)
                {
                    lastActionTaken = ActionTaken.Attack;
                    yield return StartCoroutine(Attack(player, null));
                    actionsTaken++;
                }
                else
                {
                    lastActionTaken = ActionTaken.MoveToward;
                    List<PathNode> pathToHero = pathfinding.FindPath((int)transform.position.x, (int)transform.position.z, (int)playerObject.transform.position.x, (int)playerObject.transform.position.z);
                    yield return StartCoroutine(Move(pathToHero, false, 0));

                    if (actionsTaken < actionsCap &&  DistanceToTarget(player) <= weaponRange)
                    {
                        lastActionTaken = ActionTaken.Attack;
                        yield return StartCoroutine(Attack(player, null));
                        actionsTaken++;
                    }
                }
            }
        }

        StartCoroutine(EndTurn());
    } 

    //Calls a group of weaker enemies to fight for the mini-boss
    //spawning a group of 1 - 3 enemies within the room
    private void CallReinforcements()
    {
        Debug.Log("Calling Reinforcements");

        float[] reinforcemntsProb = new float[] { 0.3f, 0.4f, 0.3f };   //prob. for number of enmeis
        ProbabilityGenerator probabilityNumEnemies = new ProbabilityGenerator(reinforcemntsProb);
        int numberEnemies = probabilityNumEnemies.GenerateNumber();
        numberEnemies++;                                                //sine the Generator returns the index, all I need is to increment to get how may enmeies

        Debug.Log("Number of enemies being called = " + numberEnemies);

        float[] enemyTypes = new float[] { 0.55f, 0.45f };
        ProbabilityGenerator probailityEnemyType = new ProbabilityGenerator(enemyTypes);

        List<string> reinforcements = new List<string>();

        for (int i = 0; i < numberEnemies; i++)
        {
            switch (probailityEnemyType.GenerateNumber())
            {
                case 0:
                    Debug.Log("A Slave is Reinforcing");
                    reinforcements.Add("Prefabs/Enemies/Slave");
                    break;
                case 1:
                    Debug.Log("A Warrior is Reinforcing");
                    reinforcements.Add("Prefabs/Enemies/Warrior");
                    break;
                default: reinforcements.Add("Prefabs/Enemies/Slave");
                    break;
            }
        }
        combatManager.CallReinforcements(reinforcements);
    }

    //creates a buff for nearby allies
    private void BuffAllies()
    {
        int enemiesBuffed = 0;

        //mini-boss creates a buff for themself
        statusEffects.Add(new Invigorated(this, 3));
        enemiesBuffed++;

        for(int i = 0; i < combatManager.enemiesInEncounter.Count; i++)
        {
            if(combatManager.enemiesInEncounter[i] != gameObject && CalculateDistance(combatManager.enemiesInEncounter[i]) <= buffRange)
            {
                //apply a buff on that enemy
                Enemy enemy = combatManager.enemiesInEncounter[i].GetComponent<Enemy>();
                enemy.statusEffects.Add(new Invigorated(enemy, 3));
                enemiesBuffed++;
            }
            if (enemiesBuffed >= maxNumAlliesToBuff) return;
        }
    }
}

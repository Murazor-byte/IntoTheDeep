using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Mini-boss that goes stealth every few turns, with ability to deploy smoke bombs
 * including a high speed to appear in different locations
 * If the Assasin is to attack from stealth, then he is immediately unstealthed
*/
public class Assassin : MiniBoss
{
    private int actionsTaken;
    private int actionsCap = 2;

    private const float smokeBombProb = 0.4f;           //prob to use a smoke bomb
    private const int smokeBombFreq = 4;                //min turns before smoke bomb can be used again
    private int turnsSinceSmokeBomb = 2;                //turns since last smoke bomb

    private SkinnedMeshRenderer skinMesh;               //used for hiding the enemy in game view
    private const float stealthProb = 0.8f;             //prob to hide from player
    private const int stealthFreq = 3;                  //min turns before stealth can be used again
    private int turnsSinceStealth = 1;                  //turns since last stealth 
    private bool stealthed = false;                     //if assasin is currently stealthed
    private const int maxStealthDuration = 3;           //max turns to stay stealthed
    private int turnsStealthed = 0;                     //#turns remained stealthed

    private const float probAttack = 0.80f;             //prob attack in range
    private const float probRetreat = 0.6f;             //prob retreat in range
    private const float probIdle = 0.4f;                //prob idle in range

    private const float probAttackOut = 0.5f;           //prob to attack out of range
    private const float probRetreatOut = 0.3f;          //prob to keep retreating
    private const float probIdleOut = 0.7f;             //prob to idle out of range

    private Assassin() : base(30, 50, 5, 10, 9)
    {
        helmArmorSelection = new List<Armor> { new LeatherHelm()};
        bodyArmorSelection = new List<Armor> { new LeatherBreastPlate() };
        greavesArmorSelection = new List<Armor> { new LeatherGreaves() };
        bootArmorSelection = new List<Armor> { new LeatherBoots()};

        allArmor = new List<List<Armor>> { helmArmorSelection, bodyArmorSelection, greavesArmorSelection, bootArmorSelection };
        helmProb = new float[] { 1f };
        bodyProb = new float[] { 1f };
        greavesProb = new float[] { 1f };
        bootsProb = new float[] { 1f };
        allArmorProb = new float[][] { helmProb, bodyProb, greavesProb, bootsProb };

        weaponSelection = new List<Weapon> { new Sword(), new Maul(), new Crossbow(), new ShortBow() };
        weaponProb = new float[] { 0.35f, 0.15f, 0.25f, 0.25f };
    }

    void Start()
    {
        ChangeDamageModifier(5);

        skinMesh = transform.Find("Mesh_Body").GetComponent<SkinnedMeshRenderer>();
    }

    public override IEnumerator Turn()
    {
        actionsTaken = 0;
        turnsSinceSmokeBomb++;
        turnsSinceStealth++;

        traversability = tileGenerator.traversability;      //set each turn since tiles can be altered during combat
        tileType = tileGenerator.tiles;

        Pathfinding pathfinding = new Pathfinding(traversability.Length, traversability.Length, tileType, traversability);

        int distanceFromHero = DistanceToTarget(player);

        //if target is within range to attack
        if(distanceFromHero <= weaponRange)
        {
            //if currently stealthed in range
            if (stealthed)
            {
                Debug.Log("Enemy is stealthed and in range");
                //decide to attack target
                if(actionsTaken < actionsCap && Random.value <= probAttack)
                {
                    Debug.Log("Enemy is attacking target");
                    RemoveStealth();
                    yield return StartCoroutine(Attack(player, null));
                    actionsTaken++;

                    yield return StartCoroutine(RetreatOrIdle(pathfinding));
                }
                //don't attack target while stealthed
                else
                {
                    Debug.Log("Enemy is not attacking target");
                    //decide to Idle Move
                    if(Random.value <= probIdle)
                    {
                        yield return StartCoroutine(IdleMove(pathfinding));
                    }
                    //else retreat from target
                    else
                    {
                        yield return StartCoroutine(RetreatFromTarget(pathfinding, 0));
                    }
                }

            }
            //else not stealth in range
            else
            {
                Debug.Log("Enemy is not stealthed and in range");
                //attack target
                yield return StartCoroutine(Attack(player, null));
                actionsTaken++;

                //decide to Stealth
                if(actionsTaken < actionsCap && turnsSinceStealth >= stealthFreq && Random.value <= stealthProb)
                {
                    Stealth();
                    yield return StartCoroutine(RetreatFromTarget(pathfinding, 0));
                }
                else
                {
                    yield return StartCoroutine(RetreatOrIdle(pathfinding));
                }
            }
        }
        //target is NOT in range to attack
        else
        {
            //if currently stealthed
            if (stealthed)
            {
                Debug.Log("Enemy is stealthed and NOT in range");
                //decide to attack target
                if(actionsTaken <= actionsCap && Random.value <= probAttack)
                {
                    yield return StartCoroutine(DistantAttack(pathfinding));
                }
                //don't attack while stealthed
                else
                {
                    Debug.Log("Enemy is NOT attacking target");
                    //decide to idleMove
                    if(Random.value <= probIdleOut)
                    {
                        yield return StartCoroutine(IdleMove(pathfinding));
                    }
                    //else decide to keep retreating
                    else if(Random.value <= probRetreatOut)
                    {
                        yield return StartCoroutine(RetreatFromTarget(pathfinding, 0));
                    }
                    //else stay in position
                    else
                    {
                        Debug.Log("Enemy staying in same position");
                        yield return null;
                    }
                }
            }
            //if not stealthed
            else
            {
                Debug.Log("Enemy is NOT stealthed and NOT in range");
                //decide to stealth while out of range
                if(actionsTaken < actionsCap && turnsSinceStealth >= stealthFreq && Random.value <= stealthProb)
                {
                    Stealth();
                    
                    //else deide to attack target
                    if (Random.value <= probAttackOut && actionsTaken < actionsCap)
                    {
                        yield return StartCoroutine(DistantAttack(pathfinding));
                    }
                    //else decide to idle
                    else if(Random.value <= probIdleOut)
                    {
                        yield return StartCoroutine(IdleMove(pathfinding));
                    }
                    //else decide to keep retreating
                    else if (Random.value <= probRetreatOut)
                    {
                        yield return StartCoroutine(RetreatFromTarget(pathfinding, 0));
                    }
                    //else idle at same location
                    else
                    {
                        Debug.Log("Enemy staying in same position");
                        yield return null;
                    }
                }
                //else not to stealth while out of range
                else
                {
                    //decide to retreat from target
                    if(Random.value <= probRetreat)
                    {
                        yield return StartCoroutine(RetreatFromTarget(pathfinding, 0));
                    }
                    //else decide to attack target
                    else if(Random.value <= probAttackOut)
                    {
                        yield return StartCoroutine(DistantAttack(pathfinding));
                    }
                    //else decide to idle
                    else if(Random.value <= probIdleOut)
                    {
                        yield return StartCoroutine(IdleMove(pathfinding));
                    }
                    //else stay at same location
                    else
                    {
                        yield return null;
                    }
                }
            }
        }

        if (stealthed) turnsStealthed++;
        if (turnsStealthed >= maxStealthDuration)
        {
            RemoveStealth();
        }

        StartCoroutine(EndTurn());
    }

    //when the enemy is trying to attack the target while out of range
    //move toward target, if they reach attack, and decide to retreat
    private IEnumerator DistantAttack(Pathfinding pathfinding)
    {
        Debug.Log("Enemy is moving toward Hero");
        List<PathNode> pathToHero = pathfinding.FindPath((int)transform.position.x, (int)transform.position.z, (int)playerObject.transform.position.x, (int)playerObject.transform.position.z);
        yield return StartCoroutine(Move(pathToHero, false, 0));

        //if enemy gets within range to attack
        if (DistanceToTarget(player) <= weaponRange)
        {
            if(stealthed) RemoveStealth();

            yield return StartCoroutine(Attack(player, null));
            actionsTaken++;

            //if there's extra movement, decide to spend the rest to retreat from target
            if (pathToHero.Count < speed && Random.value <= probRetreat)
            {
                yield return StartCoroutine(RetreatFromTarget(pathfinding, pathToHero.Count));
            }
            else
            {
                Debug.Log("Enemy staying in same position");
                yield return null;
            }
        }
    }

    //make the enemy go stealthed
    private void Stealth()
    {
        Debug.Log("Enemy has gone stealth");
        stealthed = true;
        skinMesh.enabled = false;
        actionsTaken++;
    }

    //decide to retreat or Idle with given movement
    private IEnumerator RetreatOrIdle(Pathfinding pathfinding)
    {
        //retreat from target
        if (Random.value <= probRetreat)
        {
            yield return StartCoroutine(RetreatFromTarget(pathfinding, 0));
        }
        //else stay in same position
        else
        {
            Debug.Log("Enemy staying in same position");
            yield return null;
        }
    }

    private IEnumerator RetreatFromTarget(Pathfinding pathfinding, int spentMovement)
    {
        Debug.Log("Enemy is retreating from Hero");
        List<PathNode> retreat = pathfinding.Retreat((int)transform.position.x, (int)transform.position.z, (int)playerObject.transform.position.x, (int)playerObject.transform.position.z);
        yield return StartCoroutine(Move(retreat, true, spentMovement));
    }

    private IEnumerator IdleMove(Pathfinding pathfinding)
    {
        Debug.Log("Enemy is idle moving");
        int idleDistance = Random.Range(minSpeed, maxSpeed);
        List<PathNode> idlePath = pathfinding.FindIdlePath((int)transform.position.x, (int)transform.position.z, idleDistance);
        yield return StartCoroutine(Move(idlePath, true, 0));
    }

    //Removes stealth from Mini-Boss
    private void RemoveStealth()
    {
        Debug.Log("Enemy has removed stealth");
        stealthed = false;
        skinMesh.enabled = true;
        turnsStealthed = 0;
        turnsSinceStealth = 0;
    }
}

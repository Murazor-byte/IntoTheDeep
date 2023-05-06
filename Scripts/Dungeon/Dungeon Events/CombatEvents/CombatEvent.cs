using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*this is an event which will be only created in EventManager
if a combat event has been decided to be placed, within eventManager it will decide
if the combat encounter will be a basic combat encounter, mini-boss, or boss
this script will hold these types of combatEvents the player may come across
and randomly generate an encounter of that type and instantiates those enemies*/
public class CombatEvent : Event
{
    public static int numEnemies;

    private static List<string> enemiesPrefabPath = new List<string>();
    private static float[] enemiesProb;
    private static float[] numEnemyProb;
    private static ProbabilityGenerator populateRoom;

    public override void SetUpEvent(){ }

    //creates a basic grunt encounter containing an array of each basic monster type
    /*
        * enemies Indicies:
        *          0: Archer
        *          1: Sniper
        *          2: Slave
        *          3: Warrior
        *          4: Brute
    */
    public static List<string> CreateBasicEncounter()
    {
        //relates to encounter index for holding percent values to choose from these enemies
        enemiesProb = new float[] { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f};   

        //set up how many enemies will be in this room (right now there are max of 6) MIGHT BE ABLE TO MAKE A FORMULA FOR THESE VALUES, INSTEAD OF HARD CODED
        numEnemyProb = new float[] { 0.25f, 0.30f, 0.2f, 0.135f, 0.075f, 0.04f };

        //calculates how many enemies will be in this room
        populateRoom = new ProbabilityGenerator(numEnemyProb);

        //incrementing by 1 becuase index base starts at 0
        numEnemies = populateRoom.GenerateNumber() + 1;
        Debug.Log("Number of Enemies in Encounter = " + numEnemies);
        for(int i = 0; i < numEnemies; i++)
        {

            //randomly selects a enemy type, CORRELATED WITH "enemiesProb" array
            ProbabilityGenerator selectEnemy = new ProbabilityGenerator(enemiesProb);
            int enemyIndex = selectEnemy.GenerateNumber();

            switch (enemyIndex)
            {
                case 0:
                    enemiesPrefabPath.Add("Prefabs/Enemies/Archer");
                    break;
                case 1:
                    enemiesPrefabPath.Add("Prefabs/Enemies/Sniper");
                    break;
                case 2:
                    enemiesPrefabPath.Add("Prefabs/Enemies/Slave");
                    break;
                case 3:
                    enemiesPrefabPath.Add("Prefabs/Enemies/Warrior");
                    break;
                case 4:
                    enemiesPrefabPath.Add("Prefabs/Enemies/Brute");
                    break;
                default:
                    Debug.Log("Enemy index out of bounds");
                    break;
            }
        }
        Debug.Log("Number of enemies in Prefab = " + enemiesPrefabPath.Count);
        return enemiesPrefabPath;
    }



    //can implement mini boss, specific boss events, or specialized combat events later here

    /* A basic Encounter that consisits of only medium - harder enemies
         * Enemies:
         *      0: Brute
        */
    public static List<string> CreateBasicHeavyEncounter()
    {

        enemiesProb = new float[] { 1f };
        numEnemyProb = new float[] { 0.25f, 0.30f, 0.25f, 0.20f};

        populateRoom = new ProbabilityGenerator(numEnemyProb);

        numEnemies = populateRoom.GenerateNumber() + 1;

        for (int i = 0; i < numEnemies; i++)
        {

            ProbabilityGenerator selectEnemy = new ProbabilityGenerator(enemiesProb);
            int enemyIndex = selectEnemy.GenerateNumber();

            switch (enemyIndex)
            {
                case 0:
                    enemiesPrefabPath.Add("Prefabs/Enemies/Brute");
                    break;
                default:
                    Debug.Log("Enemy index out of bounds");
                    break;
            }
        }
        return enemiesPrefabPath;
    }

    /* Basic Encounter consisiting of many weak enemies 
     *  Enemies:
     *      0: Slave
    */
    public static List<string> CreateBasicWeakSwarmEncounter()
    {
        enemiesProb = new float[] { 1f };
        numEnemyProb = new float[] { 0f, 0f, 0.05f, 0.15f, 0.20f, 0.25f, 0.15f, 0.10f, 0.10f };  //max 9

        populateRoom = new ProbabilityGenerator(numEnemyProb);

        numEnemies = populateRoom.GenerateNumber() + 1;

        for (int i = 0; i < numEnemies; i++)
        {
            ProbabilityGenerator selectEnemy = new ProbabilityGenerator(enemiesProb);
            int enemyIndex = selectEnemy.GenerateNumber();

            switch (enemyIndex)
            {
                case 0:
                    enemiesPrefabPath.Add("Prefabs/Enemies/Slave");
                    break;
                default:
                    Debug.Log("Enemy index out of bounds");
                    break;
            }
        }
        return enemiesPrefabPath;
    }


    //Mini-Boos encounters
    public static List<string> CreateHornBlowerMiniBossEncounter()
    {
        enemiesPrefabPath.Add("Prefabs/Enemies/Mini Boss/Horn Blower");

        return enemiesPrefabPath;
    }

    public static List<string> CreateAssassinMiniBossEncounter()
    {
        enemiesPrefabPath.Add("Prefabs/Enemies/Mini Boss/Assassin");
        return enemiesPrefabPath;
    }
}

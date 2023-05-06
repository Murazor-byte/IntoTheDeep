using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*Keeps track of all the events contained within the dungeon
  give each corridor tile and room a probability of spawning an event
  given that an event has spawned, give each event a probaility to be created
  at that event tile 
  IMPORTANT NOTE: whenever a new Event needs to be added,
                  add it to the "CreateEvent" method to be chosen from
                  as well as the positive and negative ordeal indecies variables*/
public class EventManager : MonoBehaviour
{
    private Room[] rooms;
    private List<Corridor> allCorridors;
    private bool[][] occupiedTiles;
    private DungeonGenerator.FloorType[][] floorType;
    public float roomEventProb = 0.45f;                 //creates probability of there being an event within a room
    public float corridorTileEventProb = 0.08f;         //creates probability of there being an event within each tile of a corridor

    private bool bossInLayer = false;                   //if the boss can spawn within this layer
    private bool bossSpawned = false;                   //if the boss has already been placed in this dungeon
    private const float bossProb = 0.75f;               //prob. of the boss spawning within this layer
    private const float miniBossProb = 0.05f;           //prob. of a minBoss occurring within a room combatEvent

    //references to the Event UI elements
    public GameObject eventUIHolder;                    //COULD MAKE THESE STATIC TO HELP WITH ORDEAL PARAMETER COUNT
    public GameObject eventButton1;
    public GameObject eventButton2;
    public GameObject eventButton3;
    public GameObject eventButton4;

    public EventText textEvent;                      
    private GameObject player;

    private ScenesManager sceneManager;

    //occurrence probablities for each type of Event
    [SerializeField] private readonly float choiceEventProb = 0f;
    [SerializeField] private readonly float combatEventProb = 1f;
    [SerializeField] private readonly float positiveEventProb = 0f;
    [SerializeField] private readonly float decisionEventProb = 0f;
    [SerializeField] private readonly float negativeEventProb = 0f;

    //indicies for 'Event.EventTypes' when choosing between events
    private const int POSITIVE_ORDEAL_MIN_INDEX = 5;    //(Inclusive) 5
    private const int POSITIVE_ORDEAL_MAX_INDEX = 15;   //(Exclusive) 

    private const int DECISION_ORDEAL_MIN_INDEX = 15;
    private const int DECISION_ORDEAL_MAX_INDEX = 25;

    private const int NEGATIVE_ORDEAL_MIN_INDEX = 25;
    private const int NEGATIVE_ORDEAL_MAX_INDEX = 37;

    private const int ORDEAL_MIN_INDEX = 5;             //for choiced events
    private const int ORDEAL_MAX_INDEX = 9;

    public void TheStart()
    {
        SetUpCorridorTileEvents();
        CreateEvents();

        SetUpUIElements();
    }

    //sets up the EventManager to gather the generated dungeon's variables
    public void SetUp(Room[] rooms, List<Corridor> allCorridors, bool[][] occupiedTiles, DungeonGenerator.FloorType[][] floorType, bool bossInLayer)
    {
        this.rooms = rooms;
        this.allCorridors = allCorridors;
        this.occupiedTiles = occupiedTiles;
        this.floorType = floorType;
        this.bossInLayer = bossInLayer;
    }

    //sets up the corridor tiles event system
    private void SetUpCorridorTileEvents()
    {
        for (int i = 0; i < allCorridors.Count; i++)
        {
            //Go through each tile for each corridor and sets up the list matrix to hold list in them, 
            //so I can access them by index if a event is made within this corridor
            for (int j = 0; j < allCorridors[i].corridorLength; j++)
            {
                allCorridors[i].discovered = new bool[allCorridors[i].corridorLength];
                List<Event.EventType> tileEvents = new List<Event.EventType>();
                allCorridors[i].possibleEvents.Add(tileEvents);
            }
        }
    }

    //caches the all references to pass to event for Event UI elements
    private void SetUpUIElements()
    {
        sceneManager = GameObject.Find("SceneManager").GetComponent<ScenesManager>();
        player = GameObject.Find("Player");

        eventUIHolder = GameObject.Find("Event Background Holder");
        textEvent = GameObject.Find("Event Text").GetComponent<EventText>();
        eventButton1 = GameObject.Find("Button1");
        eventButton2 = GameObject.Find("Button2");
        eventButton3 = GameObject.Find("Button3");
        eventButton4 = GameObject.Find("Button4");
       

        eventUIHolder.SetActive(false);       //deactivate UI when game starts
        eventButton1.SetActive(false);
        eventButton2.SetActive(false);
        eventButton3.SetActive(false);
        eventButton4.SetActive(false);
    }

    public GameObject GetPlayer()
    {
        return player;
    }

    //goes through every room and corridorTile to create events
    private void CreateEvents()
    {
        //go through every room and create an event if it falls under the roomEventProb
        for(int i = 1; i < rooms.Length; i++)
        {
            if (rooms[i] == null) continue;

            //assign a room event
            if(Random.value <= roomEventProb)
            {
                SelectRandomEvent(true, i, 0);
            }
        }

        //go through every corridorTile, and randomly create an event given that it falls under the CorridorTileEventProb
        for (int i = 0; i< allCorridors.Count; i++)
        {
            for(int j = 0; j < allCorridors[i].corridorLength; j++)
            {
                //assign a corridor event
                if(Random.value <= corridorTileEventProb)
                {
                    SelectRandomEvent(false, i, j);
                }
            }
        }
    }

    //randomly selectes an event type (combat, pos., neg., or choice) where 'isRoom' checks if the event being created is inside a room or corridor
    private void SelectRandomEvent(bool isRoom, int firstTileIndex, int secondTileIndex )
    {
        //randomly chooses an event Type given the four event Types probabilites
        ProbabilityGenerator chooseEventType = new ProbabilityGenerator(new float[] { combatEventProb, positiveEventProb, decisionEventProb, negativeEventProb, choiceEventProb });
        int eventTypeChosen = chooseEventType.GenerateNumber();

        int positiveEventChosen = Random.Range(POSITIVE_ORDEAL_MIN_INDEX, POSITIVE_ORDEAL_MAX_INDEX);
        int decisionEventChosen = Random.Range(DECISION_ORDEAL_MIN_INDEX, DECISION_ORDEAL_MAX_INDEX);
        int negativeEventChosen = Random.Range(NEGATIVE_ORDEAL_MIN_INDEX, NEGATIVE_ORDEAL_MAX_INDEX);

        switch (eventTypeChosen)
        {
            case 0: //combat event
                if(isRoom && bossInLayer && !bossSpawned && Random.value <= bossProb)
                {
                    Debug.Log("BOSS HAS SPAWNED ON THIS LAYER");
                    bossSpawned = true;
                    AddEventToTile(isRoom, firstTileIndex, secondTileIndex, Event.EventType.Boss);
                }
                else if(isRoom && Random.value <= miniBossProb)
                {
                    AddEventToTile(isRoom, firstTileIndex, secondTileIndex, Event.EventType.MiniBoss);
                }
                else
                {
                    AddEventToTile(isRoom, firstTileIndex, secondTileIndex, Event.EventType.GruntCombat);
                }
                break;
            case 1: //positive event
                AddEventToTile(isRoom, firstTileIndex, secondTileIndex, (Event.EventType)positiveEventChosen);
                break;
            case 2: //decision event
                AddEventToTile(isRoom, firstTileIndex, secondTileIndex, (Event.EventType)decisionEventChosen);
                break;
            case 3: //negative Event
                AddEventToTile(isRoom, firstTileIndex, secondTileIndex, (Event.EventType)negativeEventChosen);
                break;
            case 4: // choice event
                AddEventToTile(isRoom, firstTileIndex, secondTileIndex, Event.EventType.Choice);
                break;
        }
    }

    //adds the 'eventType' to the passed room or corridor, adding to its possible events
    private void AddEventToTile(bool isRoom, int firstTileIndex, int secondTileIndex, Event.EventType eventType)
    {
        if (isRoom) rooms[firstTileIndex].possibleEvents.Add(eventType);
        else allCorridors[firstTileIndex].possibleEvents[secondTileIndex].Add(eventType);
    }

    //chooses a random number of events, randomly selects that many number of events and returns that event list
    private List<Ordeal> SetUpChoicedEvent()
    {
        List<Ordeal> ordealsChosen = new List<Ordeal>();

        ProbabilityGenerator choices = new ProbabilityGenerator(new float[] { 0.50f, 0.30f, 0.20f });

        int numberOfChoices = choices.GenerateNumber();
        
        //+2 as it chooses off of index, and the least number of choices equals 2
        for(int i = 0; i < numberOfChoices + 2; i++)
        {
            Ordeal chosen;
            switch(i + 1)
            {
                case 1:
                    chosen = CreateEvent((Event.EventType)Random.Range(ORDEAL_MIN_INDEX, ORDEAL_MAX_INDEX), sceneManager);
                    break;
                case 2:
                    chosen = CreateEvent((Event.EventType)Random.Range(ORDEAL_MIN_INDEX, ORDEAL_MAX_INDEX), sceneManager);
                    break;
                case 3:
                    chosen = CreateEvent((Event.EventType)Random.Range(ORDEAL_MIN_INDEX, ORDEAL_MAX_INDEX), sceneManager);
                    break;
                case 4:
                    chosen = CreateEvent((Event.EventType)Random.Range(ORDEAL_MIN_INDEX, ORDEAL_MAX_INDEX), sceneManager);
                    break;
                default:
                    Debug.Log("number of chosen ordeals for chosen event went out of index");
                    chosen = null;
                    break;
            }
            ordealsChosen.Add(chosen);
        }

        Debug.Log("Number of Choices = " + ordealsChosen.Count);
        return ordealsChosen;
    }

    /*called from room or corridor to set up this specific event Type
     * method works with duel purpose:
     *      for creating a specific event to be set up in a room/corridor, nothing of value returned to caller
     *      for selecting a random ordeal when a choiced ordeal is called, returning that Ordeal chosen
    */
    public Ordeal CreateEvent(Event.EventType eventType, ScenesManager sceneManager)
    {
        Ordeal newOrdeal;
        switch (eventType)
        {
        //Choiced Event
            case Event.EventType.Choice:
                Debug.Log("Choiced Event Encountered");
                List<Ordeal> ordealsChosen = SetUpChoicedEvent();
                newOrdeal = new ChoicedOrdeal(ordealsChosen, player, sceneManager);
                break;
        //Positive Events
            case Event.EventType.GainWeapon:
                newOrdeal = new GainWeaponEvent(player, sceneManager); break;
            case Event.EventType.GainArmor:
                newOrdeal = new GainArmorEvent(player, sceneManager); break;
            case Event.EventType.GainItem:
                newOrdeal = new GainItemEvent(player, sceneManager); break;
            case Event.EventType.Rest:
                newOrdeal = new RestEvent(player, sceneManager); break;
            case Event.EventType.HighKilled:
                newOrdeal = new HighMonstersKilledEvent(player, sceneManager); break;
            case Event.EventType.LowFighting:
                newOrdeal = new LowFightingEvent(player, sceneManager); break;
            case Event.EventType.LowHorror:
                newOrdeal = new LowHorrorEvent(player, sceneManager); break;
            case Event.EventType.Thought1:
                newOrdeal = new Thought1Event(player, sceneManager); break;
            case Event.EventType.GoldDeposit:
                newOrdeal = new GoldDepositEvent(player, sceneManager); break;
            case Event.EventType.BrokenLight:
                newOrdeal = new BrokenLightEvent(player, sceneManager); break;
        //Decision Events
            case Event.EventType.EvadeDanger:
                newOrdeal = new EvadeDangerEvent(player, sceneManager); break;
            case Event.EventType.LootPile:
                newOrdeal = new LootPileEvent(player, sceneManager); break;
            case Event.EventType.NaturalPit:
                newOrdeal = new NaturalPitEvent(player, sceneManager); break;
            case Event.EventType.TownsfolkAdventurer:
                newOrdeal = new TownsfolkAdventurerEvent(player, sceneManager); break;
            case Event.EventType.TerrifiedTownsfolk:
                newOrdeal = new TerrifiedTownsfolkEvent(player, sceneManager); break;
            case Event.EventType.DeadTownsfolk:
                newOrdeal = new DeadTownsfolkEvent(player, sceneManager); break;
            case Event.EventType.SmallLake:
                newOrdeal = new SmallLakeEvent(player, sceneManager); break;
            case Event.EventType.DeadAnimal:
                newOrdeal = new DeadAnimalEvent(player, sceneManager); break;
            case Event.EventType.LootHorror:
                newOrdeal = new LootHorrorEvent(player, sceneManager); break;
            case Event.EventType.MysteriousFungus:
                newOrdeal = new MysteriousFungusEvent(player, sceneManager); break;
        //Negative Events
            case Event.EventType.CaveIn:
                newOrdeal = new CaveInEvent(player, sceneManager); break;
            case Event.EventType.Carnage:
                newOrdeal = new CarnageEvent(player, sceneManager); break;
            case Event.EventType.Stress:
                newOrdeal = new StressEvent(player, sceneManager); break;
            case Event.EventType.Sound:
                newOrdeal = new SoundEvent(player, sceneManager); break;
            case Event.EventType.Crevasse:
                newOrdeal = new CrevasseEvent(player, sceneManager); break;
            case Event.EventType.TimeElapsed:
                newOrdeal = new TimeElapsedEvent(player, sceneManager); break;
            case Event.EventType.HighHorror:
                newOrdeal = new HighHorrorEvent(player, sceneManager); break;
            case Event.EventType.LowKilled:
                newOrdeal = new LowMonstersKilledEvent(player, sceneManager); break;
            case Event.EventType.EscapeCusedPuddle:
                newOrdeal = new EscapeCursedPuddleEvent(player, sceneManager); break;
            case Event.EventType.BlockedPath:
                newOrdeal = new BlockedPathEvent(player, sceneManager); break;
            case Event.EventType.Fog:
                newOrdeal = new FogEvent(player, sceneManager); break;
            case Event.EventType.EatFood:
                newOrdeal = new EatFoodEvent(player, sceneManager); break;
            default:
                Debug.Log("ERROR: Event Could Not Be Found");
                newOrdeal = null;
                break;
        }

        UIManager.Instance.SetDungeonInteractivePlayerUI(false);
        newOrdeal.SetUpEvent();                                 //set up the chosen event

        return newOrdeal;
    }
}

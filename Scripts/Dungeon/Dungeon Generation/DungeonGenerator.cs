using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;


public class DungeonGenerator
{
    public enum TileType
    {
        Wall, Floor,
    }

    public enum FloorType
    {
        CorridorTile, RoomTile,
    }


    private int rows;
    private int columns;
    private float prob1CorridorConnect = 1.0f;
    private float prob2CorridorConnect = 0.45f;
    private float prob3CorridorConnect = 0.15f;
    private int minNumberRooms;
    private int maxNumberRooms;
    private RandomInt numRooms;
    private int minLengthCorridor = 4;
    private int maxLengthCorridor = 12;
    private RandomInt corridorLength;
    private GameObject floorTiles;
    private GameObject wallTiles;
    private List<Corridor> connectedCorridors = new List<Corridor>();   //a list of each corridor created through ConnectRooms()
    public List<Corridor> allCorridors = new List<Corridor>();         //puts both non-null corridors & connectedCorridors into one list


    private int roomWidth = 5;
    private int roomHeight = 5;
    private TileType[][] tiles;
    public FloorType[][] floorType;    //keeps track which occupied tile is a floor/corridor
    public bool[][] occupiedTiles;
    private bool corridorPlaced = true;
    private bool connectingCorridorPlaced;
    public Room[] rooms;
    private Corridor[] corridors;
    private GameObject boardHolder;
    public GameObject instantiateHolder { get; private set; }

    public DungeonGenerator(int rows, int columns, int minNumberRooms, int maxNumberRooms, GameObject floorTiles, GameObject wallTiles)
    {
        this.rows = rows;
        this.columns = columns;
        this.minNumberRooms = minNumberRooms;
        this.maxNumberRooms = maxNumberRooms;
        this.floorTiles = floorTiles;
        this.wallTiles = wallTiles;
    }


    public void Start()
    {
        numRooms = new RandomInt(minNumberRooms, maxNumberRooms);
        corridorLength = new RandomInt(minLengthCorridor, maxLengthCorridor);

        boardHolder = new GameObject("BoardHolder");

        SetUpTilesArray();
        SetUpFloorTypeArray();
        SetUpBooleanArray();

        CreateRoomsAndCorridors();
        ConnectRooms();

        SetTilesValuesForRooms();
        SetTilesValuesForCorridors();

        //This creates a gameobject which calls InstantiateTiles to instantiate all the tiles for the board
        //this allows for me to keep track of everything within the DungeonManager script
        instantiateHolder = new GameObject("Tile Instantiator");
        instantiateHolder.AddComponent<InstantiateTiles>();
        InstantiateTiles instantiateTiles = instantiateHolder.GetComponent<InstantiateTiles>();
        instantiateTiles.InstantiateAllTiles(tiles,boardHolder,floorTiles,wallTiles);

        ConnectAllCorridors();

    }

    private void ConnectAllCorridors()
    {
        for (int i = 0; i < corridors.Length; i++)
        {
            if (corridors[i] != null)
            {
                allCorridors.Add(corridors[i]);
            }
        }
        for(int i = 0; i < connectedCorridors.Count; i++)
        {
            allCorridors.Add(connectedCorridors[i]);
        }
    }

    //set up the main tiles matrix ready to hold corridors and rooms Pos.
    public void SetUpTilesArray()
    {
        tiles = new TileType[columns][];

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new TileType[rows];
        }
    }

    //set up the floorType tiles to keep track which occupied tile is a room or corridor
    private void SetUpFloorTypeArray()
    {
        floorType = new FloorType[columns][];

        for (int i = 0; i < floorType.Length; i++)
        {
            floorType[i] = new FloorType[rows];
        }
    }

    //set up the boolean matrix which starts with every corrd = false
    private void SetUpBooleanArray()
    {
        occupiedTiles = new bool[columns][];

        for (int i = 0; i < occupiedTiles.Length; i++)
        {
            occupiedTiles[i] = new bool[rows];
        }
    }

    private void CreateRoomsAndCorridors()
    {
        rooms = new Room[numRooms.Random];

        //keep track of created rooms for corridor to call when searching for a corridor placement
        List<Room> createdRooms = new List<Room>();

        //Keep track of last room placed as a reference for if a corridor returns null
        Room lastRoomPlaced;
        Corridor lastCorridorPlaced;

        Direction incomingCorridorDirection;

        corridors = new Corridor[rooms.Length - 1];

        rooms[0] = new Room();
        corridors[0] = new Corridor();

        //set up first room
        rooms[0].SetUpRoom(roomWidth, roomHeight, columns, rows);
        lastRoomPlaced = rooms[0];
        createdRooms.Add(rooms[0]);
        InstantiateOccupiedRoom(lastRoomPlaced);

        //set up first corridor using first room
        corridors[0].SetUpCorridor(createdRooms, rooms[0], corridorLength, roomWidth, roomHeight, columns, rows, true, occupiedTiles);
        lastCorridorPlaced = corridors[0];
        InstantiateOccupiedCorridor(corridors[0]);
        incomingCorridorDirection = corridors[0].oppositeDirection;

        //set up the remaining rooms and corridors
        for (int i = 1; i < rooms.Length; i++)
        {
            //test if previous corridor was placed set the room if it was
            if (corridorPlaced == true)
            {
                rooms[i] = new Room();
                rooms[i].SetUpRoom(roomWidth, roomHeight, lastCorridorPlaced);
                lastRoomPlaced = rooms[i];
                createdRooms.Add(rooms[i]);
                InstantiateOccupiedRoom(lastRoomPlaced);

                //depending on where the last corridor came from, set that room direction to true
                SetIncomingCorridorDirection(rooms[i], incomingCorridorDirection, i);
            }
            else
            {
                //if the previous corridor was not placed make this room null
                rooms[i] = null;
            }

            corridorPlaced = true;

            if (i < corridors.Length)
            {
                corridors[i] = new Corridor();
                corridors[i].SetUpCorridor(createdRooms, lastRoomPlaced, corridorLength, roomWidth, roomHeight, columns, rows, false, occupiedTiles);

                //test if the corridor was placed, if not then make this corridor and next room null
                if (corridors[i].corridorPlaced == false)
                {
                    corridorPlaced = false;
                    corridors[i] = null;
                }
                //else the corridor was placed and we can instantiate their tiles & set incomingCorridorDirection
                else
                {
                    lastCorridorPlaced = corridors[i];
                    incomingCorridorDirection = corridors[i].oppositeDirection;
                    InstantiateOccupiedCorridor(lastCorridorPlaced);
                }
            }
        }

    }

    //sets the values for rooms to be type wall
    private void SetTilesValuesForRooms()
    {
        // go through each room
        for (int i = 0; i < rooms.Length; i++)
        {
            //skip this room, since previous corridor couldn't be made
            if (rooms[i] == null)
            {
                continue;
            }

            Room currentRoom = rooms[i];
            //go through each rooms width
            for (int j = 0; j < currentRoom.roomWidth; j++)
            {
                int xCoord = currentRoom.xPos + j;

                //for every width tile go up its height
                for (int k = 0; k < currentRoom.roomHeight; k++)
                {
                    int yCoord = currentRoom.yPos + k;
                    tiles[xCoord][yCoord] = TileType.Floor;
                    floorType[xCoord][yCoord] = FloorType.RoomTile;
                }
            }
        }
    }

    //sets the values for corridors to be type floor
    private void SetTilesValuesForCorridors()
    {
        //go through every corridor
        for (int i = 0; i < corridors.Length; i++)
        {
            //skip this corridor, since it couldn't be made
            if (corridors[i] == null)
            {
                continue;
            }

            Corridor currentCorridor = corridors[i];

            //currentCorridor.connectedCorridorXUnits.Add(currentCorridor.startXPos);
            //currentCorridor.connectedCorridorYUnits.Add(currentCorridor.startYPos);
            //go through its length
            for (int j = 0; j < currentCorridor.corridorLength; j++)
            {
                //starting coordinates at the start of the corridor
                int xCoord = currentCorridor.startXPos;
                int yCoord = currentCorridor.startYPos;


                //If the corridor did not bend make tiles straight from start to end
                if (currentCorridor.corridorBend == false)
                {
                    switch (currentCorridor.direction)
                    {
                        case Direction.North:
                            yCoord += j;
                            break;
                        case Direction.East:
                            xCoord += j;
                            break;
                        case Direction.South:
                            yCoord -= j;
                            break;
                        case Direction.West:
                            xCoord -= j;
                            break;
                    }
                    currentCorridor.corridorXUnits.Add(xCoord);
                    currentCorridor.corridorYUnits.Add(yCoord);
                    tiles[xCoord][yCoord] = TileType.Floor;
                    floorType[xCoord][yCoord] = FloorType.CorridorTile;
                }
                //Then the corridor did bend, place the tiles up to the bend
                else
                {
                    if (j < currentCorridor.bendPlacement)
                    {
                        switch (currentCorridor.direction)
                        {
                            case Direction.North:
                                yCoord += j;
                                break;
                            case Direction.East:
                                xCoord += j;
                                break;
                            case Direction.South:
                                yCoord -= j;
                                break;
                            case Direction.West:
                                xCoord -= j;
                                break;
                        }
                        currentCorridor.corridorXUnits.Add(xCoord);
                        currentCorridor.corridorYUnits.Add(yCoord);
                        tiles[xCoord][yCoord] = TileType.Floor;
                        floorType[xCoord][yCoord] = FloorType.CorridorTile;
                    }
                }

            }
            //If corridor did bend, place the remaning bending tiles
            if (currentCorridor.corridorBend == true)
            {
                for (int k = 1; k < currentCorridor.corridorBendLength + 1; k++)
                {
                    int bendXCoord = currentCorridor.bendXPos;
                    int bendYCoord = currentCorridor.bendYPos;

                    switch (currentCorridor.bendDirection)
                    {
                        case Direction.North:
                            bendYCoord += k;
                            break;
                        case Direction.East:
                            bendXCoord += k;
                            break;
                        case Direction.South:
                            bendYCoord -= k;
                            break;
                        case Direction.West:
                            bendXCoord -= k;
                            break;
                    }
                    currentCorridor.corridorXUnits.Add(bendXCoord);
                    currentCorridor.corridorYUnits.Add(bendYCoord);
                    tiles[bendXCoord][bendYCoord] = TileType.Floor;
                    floorType[bendXCoord][bendYCoord] = FloorType.CorridorTile;
                }
            }
        }
    }

    //sets every position in this matrix to true if there is a corridor or room there
    //used to calculate the positions of every room and corridor
    //this boolean matrix will be used for finding overlaps when creating rooms/corridors
    private void InstantiateOccupiedCorridor(Corridor corridor)
    {
        //if corridor does not bend
        if (corridor.corridorBend == false)
        {
            for (int i = 0; i < corridor.corridorLength; i++)
            {

                switch (corridor.direction)
                {
                    case Direction.North:
                        occupiedTiles[corridor.startXPos][corridor.startYPos + i] = true;
                        break;
                    case Direction.East:
                        occupiedTiles[corridor.startXPos + i][corridor.startYPos] = true;
                        break;
                    case Direction.South:
                        occupiedTiles[corridor.startXPos][corridor.startYPos - i] = true;
                        break;
                    case Direction.West:
                        occupiedTiles[corridor.startXPos - i][corridor.startYPos] = true;
                        break;
                }

            }
        }
        //else if corridor does have a bend reflect that
        else if (corridor.corridorBend == true)
        {
            //get the first 'bendPlacement' units
            for (int i = 0; i < corridor.bendPlacement; i++)
            {
                switch (corridor.direction)
                {
                    case Direction.North:
                        occupiedTiles[corridor.startXPos][corridor.startYPos + i] = true;
                        break;
                    case Direction.East:
                        occupiedTiles[corridor.startXPos + i][corridor.startYPos] = true;
                        break;
                    case Direction.South:
                        occupiedTiles[corridor.startXPos][corridor.startYPos - i] = true;
                        break;
                    case Direction.West:
                        occupiedTiles[corridor.startXPos - i][corridor.startYPos] = true;
                        break;
                }
            }
            //set valus for the bend
            for (int i = 1; i < corridor.corridorBendLength + 1; i++)
            {
                switch (corridor.bendDirection)
                {
                    case Direction.North:
                        occupiedTiles[corridor.bendXPos][corridor.bendYPos + i] = true;
                        break;
                    case Direction.East:
                        occupiedTiles[corridor.bendXPos + i][corridor.bendYPos] = true;
                        break;
                    case Direction.South:
                        occupiedTiles[corridor.bendXPos][corridor.bendYPos - i] = true;
                        break;
                    case Direction.West:
                        occupiedTiles[corridor.bendXPos - i][corridor.bendYPos] = true;
                        break;
                }
            }
        }

    }

    private void InstantiateOccupiedRoom(Room room)
    {
        //find the x and y position of room and go over and up 4 to set the matrix to true
        for (int i = 0; i < roomWidth; i++)
        {
            for (int j = 0; j < roomHeight; j++)
            {
                occupiedTiles[room.xPos + i][room.yPos + j] = true;
            }
        }
    }

    //This sets the rooms direction that a corridor can be placed to true 
    //based on where a corridor came INTO this room
    private void SetIncomingCorridorDirection(Room room, Direction incomingCorridorDirection, int i)
    {
        switch (incomingCorridorDirection)
        {
            case Direction.North:
                rooms[i].corridorNorth = true;
                break;
            case Direction.East:
                rooms[i].corridorEast = true;
                break;
            case Direction.South:
                rooms[i].corridorSouth = true;
                break;
            case Direction.West:
                rooms[i].corridorWest = true;
                break;
        }
    }

    //connects corridors between nearby rooms where rooms with less
    //corridors attached to it will have a higher chance of connecting a corridor to it
    //updates both rooms where corridor was placed to true
    //updates occupiedTiles
    private void ConnectRooms()
    {
        for (int i = 1; i < rooms.Length; i++)
        {
            connectingCorridorPlaced = false;

            if (rooms[i] == null)
            {
                continue;
            }

            //if the room only has one corridor attached to it (always probabality)
            if (rooms[i].CountCorridorsInRoom() == 1)
            {
                // create the probability of creating connecting corridor
                if (UnityEngine.Random.value <= prob1CorridorConnect)
                    SearchForConnectingRooms(rooms[i],i);
            }
            //if the room has two corridors attached to it (medium-low probability)
            if (rooms[i].CountCorridorsInRoom() == 2)
            {
                //create the probability of creating connecting corridor
                if (UnityEngine.Random.value <= prob2CorridorConnect)
                    SearchForConnectingRooms(rooms[i],i);
            }
            //if the room has three corridors attached to it (low-very low probability)
            if (rooms[i].CountCorridorsInRoom() == 3)
            {
                //create probability of creating connecting corridor
                if (UnityEngine.Random.value < prob3CorridorConnect)
                    SearchForConnectingRooms(rooms[i],i);
            }
        }
    }

    //searches for either NE or SW rooms first
    private void SearchForConnectingRooms(Room room, int sameRoom)
    {
        bool checkNorth = false;

        if (UnityEngine.Random.value < 0.5f)
            checkNorth = true;

        if (checkNorth)
        {
            SearchRoomsNorthEast(room,sameRoom);
            if (!connectingCorridorPlaced)
            {
                SearchRoomsSouthWest(room, sameRoom);
            }
                
        }
        else if(!checkNorth)
        {
            SearchRoomsSouthWest(room,sameRoom);
            if (!connectingCorridorPlaced)
            {
                SearchRoomsNorthEast(room, sameRoom);
            }
        }
    }

    //searches for nearby rooms to attach to that are north and east
    private void SearchRoomsNorthEast(Room room, int sameRoom)
    {
        if(!connectingCorridorPlaced && !room.corridorNorth)
        {
            CheckRoomsNorth(room,sameRoom);
        }
        if(!connectingCorridorPlaced && !room.corridorEast)
        {
            CheckRoomsEast(room,sameRoom);
        }

    }

    //searches for nearvy rooms to attach to that are south and west
    private void SearchRoomsSouthWest(Room room, int sameRoom)
    {
        if (!connectingCorridorPlaced && !room.corridorSouth)
        {
            CheckRoomsSouth(room,sameRoom);
        }
        if (!connectingCorridorPlaced && !room.corridorWest)
        {
            CheckRoomsWest(room,sameRoom);
        }
    }

    //checks for rooms that are within range and are north of this room
    //testing for placement of found room to decide which side to place corridor on
    //setting occupiedTiles, TileType, and connectingCorridorPlaced to true if a corridor was created
    //sameRoom checks to make sure, the the room being checked doesn't check itself
    private void CheckRoomsNorth(Room room, int sameRoom)
    {
        /*look for rooms that have a startYPos at most 14 unit away from this rooms endYPos
        AND the room must be 4 units away from that rooms StartYPos and this rooms endYPos
        AND (look for rooms that have a startXPos at most 13 units to right of this rooms room.endXPos
        OR a endXPos at most 13 units to the left of this rooms room.startXPos)
        loop through each room created that fits this criteria
        once a room has been found find offset of where corridor will have to bend if it needs to
        CONNECTINGPOINT is where the last corridor unit will be placed, it is not inside the found room but flush with it
        these parameters are for connecting a corridor that runs north out of this room and connects south position of the room found*/


        //goes through every room to find one that fits parameters
        for (int i = 1; i < rooms.Length; i++)
        {
            if (rooms[i] == null || i == sameRoom)
            {
                continue;
            }

            int corridorStartX = room.xPos + 2;
            int corridorStartY = room.yPos + 5;
            int bendPos1X;
            int findBendPos1Y;
            int bendPos1Y;
            int bendPos2X;
            int bendPos2Y;
            int connectingPointX;
            int connectingPointY;
            bool foundXGreater = FoundXGreater(rooms[i], room);
            bool foundYGreater = FoundYGreater(rooms[i], room);
            bool aligned = RoomsAligned(rooms[i], room);
            bool overlapStartToBend1 = false;
            bool overlapBend1ToBend2 = false;
            bool overlapBend2ToEnd = false;
            bool overlapBend1ToEnd = false;
            bool overlapStartToEnd = false;
            Corridor connectedCorridor = new Corridor();

            //FOR CALCULATING OVERLAP: go through each of the for loops in each case to test for any overlaps updating any "overlap" bool to true if so
            //then test if all the appropriate bools are false and if so place occupiedTiles and Tiles with the same for loops for each condition

            if (!room.corridorNorth && ConnectingRoomsInRange(rooms[i], room, 0, foundXGreater, foundYGreater, aligned))
            {
                //testing if found room is to the right of this room
                if ((rooms[i].xPos > room.xPos))
                {
                    //find if we need to connect NORTH-WEST
                    if (!rooms[i].corridorWest && rooms[i].xPos >= room.endXPos)
                    {
                        connectingPointX = rooms[i].xPos - 1;
                        connectingPointY = rooms[i].yPos + 2;
                        bendPos1X = corridorStartX;
                        bendPos1Y = connectingPointY;

                        //TESTING OVERLAP
                        //from starting position to bend1
                        for (int j = 0; j < (bendPos1Y - corridorStartY +1); j++)
                        {
                            if (occupiedTiles[corridorStartX][corridorStartY + j] == true || occupiedTiles[corridorStartX-1][corridorStartY + j] == true || occupiedTiles[corridorStartX+1][corridorStartY + j] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bend to connectingPoint
                        for (int j = 0; j < (connectingPointX - bendPos1X) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X + j][bendPos1Y] == true || occupiedTiles[bendPos1X + j][bendPos1Y-1] == true || occupiedTiles[bendPos1X + j][bendPos1Y+1] == true)
                                overlapBend1ToEnd = true;
                        }
                        //PLACING CONNECTING CORRIDOR
                        if(!overlapStartToBend1 && !overlapBend1ToEnd)
                        {
                            connectedCorridor.corridorLength += (bendPos1Y - corridorStartY + 1);

                            //from starting position to bend1
                            for (int j = 0; j < (bendPos1Y - corridorStartY + 1); j++)
                            {
                                occupiedTiles[corridorStartX][corridorStartY + j] = true;
                                tiles[corridorStartX][corridorStartY + j] = TileType.Floor;
                                floorType[corridorStartX][corridorStartY + j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX);
                                connectedCorridor.corridorYUnits.Add(corridorStartY + j);
                            }

                            //from bend to connectingPoint
                            for (int j = 1; j < (connectingPointX - bendPos1X) + 1; j++)
                            {
                                occupiedTiles[bendPos1X + j][bendPos1Y] = true;
                                tiles[bendPos1X + j][bendPos1Y] = TileType.Floor;
                                floorType[bendPos1X + j][bendPos1Y] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X + j);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y);
                            }
                            connectedCorridor.corridorLength += (connectingPointX - bendPos1X);

                            connectingCorridorPlaced = true;
                            room.corridorNorth = true;
                            rooms[i].corridorWest = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                    //else we connect NORTH-SOUTH
                    else if (!rooms[i].corridorSouth)
                    {
                        bendPos1X = corridorStartX;
                        findBendPos1Y = (int)(rooms[i].yPos - room.endYPos) / 2;
                        bendPos1Y = room.endYPos + findBendPos1Y;
                        connectingPointX = rooms[i].xPos + 2;
                        connectingPointY = rooms[i].yPos - 1;
                        bendPos2X = connectingPointX;
                        bendPos2Y = bendPos1Y;

                        //TESTING OVERLAP
                        //from start of corridor to first bend
                        for (int j = 0; j < (bendPos1Y - corridorStartY) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX][corridorStartY + j] == true || occupiedTiles[corridorStartX-1][corridorStartY + j] == true || occupiedTiles[corridorStartX+1][corridorStartY + j] == true)
                                overlapStartToBend1 = true;
                        }
                        //from first bend to second bend
                        for (int j = 0; j < (bendPos2X - bendPos1X) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X + j][bendPos1Y] == true || occupiedTiles[bendPos1X + j][bendPos1Y-1] == true || occupiedTiles[bendPos1X + j][bendPos1Y+1] == true)
                                overlapBend1ToBend2 = true;
                        }
                        //from second bend to connectingPoint
                        for (int j = 0; j < (connectingPointY - bendPos2Y) + 1; j++)
                        {
                            if (occupiedTiles[bendPos2X][bendPos2Y + j] == true || occupiedTiles[bendPos2X-1][bendPos2Y + j] == true || occupiedTiles[bendPos2X+1][bendPos2Y + j] == true)
                                overlapBend2ToEnd = true;
                        }
                        //PLACING CONNECTING CORRIDOR
                        if(!overlapStartToBend1 && !overlapBend1ToBend2 && !overlapBend2ToEnd)
                        {
                            connectedCorridor.corridorLength += (bendPos1Y - corridorStartY + 1);

                            //from start of corridor to first bend
                            for (int j = 0; j < (bendPos1Y - corridorStartY) + 1; j++)
                            {
                                occupiedTiles[corridorStartX][corridorStartY + j] = true;
                                tiles[corridorStartX][corridorStartY + j] = TileType.Floor;
                                floorType[corridorStartX][corridorStartY + j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX);
                                connectedCorridor.corridorYUnits.Add(corridorStartY + j);
                            }
                            //from first bend to second bend
                            for (int j = 1; j < (bendPos2X - bendPos1X) + 1; j++)
                            {
                                occupiedTiles[bendPos1X + j][bendPos1Y] = true;
                                tiles[bendPos1X + j][bendPos1Y] = TileType.Floor;
                                floorType[bendPos1X + j][bendPos1Y] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X + j);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y);
                            }
                            connectedCorridor.corridorLength += (bendPos2X - bendPos1X);

                            //from second bend to connectingPoint
                            for (int j = 1; j < (connectingPointY - bendPos2Y) + 1; j++)
                            {
                                occupiedTiles[bendPos2X][bendPos2Y + j] = true;
                                tiles[bendPos2X][bendPos2Y + j] = TileType.Floor;
                                floorType[bendPos2X][bendPos2Y + j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos2X);
                                connectedCorridor.corridorYUnits.Add(bendPos2Y + j);
                            }
                            connectedCorridor.corridorLength += (connectingPointY - bendPos2Y);

                            connectingCorridorPlaced = true;
                            room.corridorNorth = true;
                            rooms[i].corridorSouth = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                }
                //testing if found room is to the left of this room
                else if ((rooms[i].xPos < room.xPos))
                {
                    //find if we need to connect NORTH-EAST
                    if (!rooms[i].corridorEast && rooms[i].endXPos <= room.xPos)
                    {
                        connectingPointX = rooms[i].xPos + 5;
                        connectingPointY = rooms[i].yPos + 2;
                        bendPos1X = corridorStartX;
                        bendPos1Y = connectingPointY;

                        //TESTING OVERLAP
                        //from startingPosition to bend1
                        for (int j = 0; j < (bendPos1Y - corridorStartY) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX][corridorStartY + j] == true || occupiedTiles[corridorStartX-1][corridorStartY + j] == true || occupiedTiles[corridorStartX+1][corridorStartY + j] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bendPos to connectingPoint
                        for (int j = 0; j < (bendPos1X - connectingPointX) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X - j][bendPos1Y] == true || occupiedTiles[bendPos1X - j][bendPos1Y-1] == true || occupiedTiles[bendPos1X - j][bendPos1Y+1] == true)
                                overlapBend1ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if(!overlapStartToBend1 && !overlapBend1ToEnd)
                        {
                            connectedCorridor.corridorLength += (bendPos1Y - corridorStartY + 1);

                            //from startingPosition to bend1
                            for (int j = 0; j < (bendPos1Y - corridorStartY) + 1; j++)
                            {
                                occupiedTiles[corridorStartX][corridorStartY + j] = true;
                                tiles[corridorStartX][corridorStartY + j] = TileType.Floor;
                                floorType[corridorStartX][corridorStartY + j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX);
                                connectedCorridor.corridorYUnits.Add(corridorStartY + j);
                            }

                            //from bendPos to connectingPoint
                            for (int j = 1; j < (bendPos1X - connectingPointX) + 1; j++)
                            {
                                occupiedTiles[bendPos1X - j][bendPos1Y] = true;
                                tiles[bendPos1X - j][bendPos1Y] = TileType.Floor;
                                floorType[bendPos1X - j][bendPos1Y] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X - j);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y);
                            }
                            connectedCorridor.corridorLength += (bendPos1X - connectingPointX);
                            connectingCorridorPlaced = true;
                            room.corridorNorth = true;
                            rooms[i].corridorEast = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                    //else we connect NORTH-SOUTH
                    else if (!rooms[i].corridorSouth)
                    {
                        bendPos1X = corridorStartX;
                        findBendPos1Y = (int)(rooms[i].yPos - room.endYPos) / 2;
                        bendPos1Y = room.endYPos + findBendPos1Y;
                        connectingPointX = rooms[i].xPos + 2;
                        connectingPointY = rooms[i].yPos - 1;
                        bendPos2X = connectingPointX;
                        bendPos2Y = bendPos1Y;

                        //TESTING OVERLAP
                        //from start of corridor to first bend
                        for (int j = 0; j < (bendPos1Y - corridorStartY) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX][corridorStartY + j] == true || occupiedTiles[corridorStartX-1][corridorStartY + j] == true || occupiedTiles[corridorStartX+1][corridorStartY + j] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bendPos1 to bendPos2
                        for (int j = 0; j < (bendPos1X - bendPos2X) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X - j][bendPos1Y] == true || occupiedTiles[bendPos1X - j][bendPos1Y-1] == true || occupiedTiles[bendPos1X - j][bendPos1Y+1] == true)
                                overlapBend1ToBend2 = true;
                        }
                        //from bendPos2 to connectingPoint
                        for (int j = 0; j < (connectingPointY - bendPos2Y) + 1; j++)
                        {
                            if (occupiedTiles[bendPos2X][bendPos2Y + j] == true || occupiedTiles[bendPos2X-1][bendPos2Y + j] == true || occupiedTiles[bendPos2X+1][bendPos2Y + j] == true)
                                overlapBend2ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if(!overlapStartToBend1 && !overlapBend1ToBend2 && !overlapBend2ToEnd)
                        {
                            connectedCorridor.corridorLength += (bendPos1Y - corridorStartY + 1);

                            //from start of corridor to first bend
                            for (int j = 0; j < (bendPos1Y - corridorStartY) + 1; j++)
                            {
                                occupiedTiles[corridorStartX][corridorStartY + j] = true;
                                tiles[corridorStartX][corridorStartY + j] = TileType.Floor;
                                floorType[corridorStartX][corridorStartY + j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX);
                                connectedCorridor.corridorYUnits.Add(corridorStartY + j);
                            }

                            //from bendPos1 to bendPos2
                            for (int j = 1; j < (bendPos1X - bendPos2X) + 1; j++)
                            {
                                occupiedTiles[bendPos1X - j][bendPos1Y] = true;
                                tiles[bendPos1X - j][bendPos1Y] = TileType.Floor;
                                floorType[bendPos1X - j][bendPos1Y] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X - j);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y);
                            }
                            connectedCorridor.corridorLength += (bendPos1X - bendPos2X);

                            //from bendPos2 to connectingPoint
                            for (int j = 1; j < (connectingPointY - bendPos2Y) + 1; j++)
                            {
                                occupiedTiles[bendPos2X][bendPos2Y + j] = true;
                                tiles[bendPos2X][bendPos2Y + j] = TileType.Floor;
                                floorType[bendPos2X][bendPos2Y + j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos2X);
                                connectedCorridor.corridorYUnits.Add(bendPos2Y + j);
                            }
                            connectedCorridor.corridorLength += (connectingPointY - bendPos2Y);
                            connectingCorridorPlaced = true;
                            room.corridorNorth = true;
                            rooms[i].corridorSouth = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                }
                //else test if the rooms are aligned and can make a straight corridor
                else if (!rooms[i].corridorSouth && aligned && (rooms[i].yPos - room.endYPos < 14 && rooms[i].yPos - room.endYPos > 3))
                {
                    connectingPointX = rooms[i].xPos + 2;
                    connectingPointY = rooms[i].yPos - 1;

                    //TESTING OVERLAP
                    for (int j = 0; j < (connectingPointY - corridorStartY) + 1; j++)
                    {
                        if (occupiedTiles[corridorStartX][corridorStartY + j] == true || occupiedTiles[corridorStartX-1][corridorStartY + j] == true || occupiedTiles[corridorStartX+1][corridorStartY + j] == true)
                            overlapStartToEnd = true;
                    }

                    //PLACING CONNECTING CORRIDOR
                    if (!overlapStartToEnd)
                    {
                        connectedCorridor.corridorLength += (connectingPointY - corridorStartY +1);

                        for (int j = 0; j < (connectingPointY - corridorStartY) + 1; j++)
                        {
                            occupiedTiles[corridorStartX][corridorStartY + j] = true;
                            tiles[corridorStartX][corridorStartY + j] = TileType.Floor;
                            floorType[corridorStartX][corridorStartY + j] = FloorType.CorridorTile;
                            connectedCorridor.corridorXUnits.Add(corridorStartX);
                            connectedCorridor.corridorYUnits.Add(corridorStartY + j);
                        }
                        connectingCorridorPlaced = true;
                        room.corridorNorth = true;
                        rooms[i].corridorSouth = true;
                        connectedCorridors.Add(connectedCorridor);
                    }
                }
            }
        }
    }

    //check CheckRoomsNorth comments
    private void CheckRoomsEast(Room room, int sameRoom)
    {
        for (int i = 1; i < rooms.Length; i++)
        {
            if (rooms[i] == null || i == sameRoom)
            {
                continue;
            }

            int corridorStartX = room.xPos + 5;
            int corridorStartY = room.yPos + 2;
            int findBendPos1X;
            int bendPos1X;
            int bendPos1Y;
            int bendPos2X;
            int bendPos2Y;
            int connectingPointX;
            int connectingPointY;
            bool foundXGreater = FoundXGreater(rooms[i], room);
            bool foundYGreater = FoundYGreater(rooms[i], room);
            bool aligned = RoomsAligned(rooms[i], room);
            bool overlapStartToBend1 = false;
            bool overlapBend1ToBend2 = false;
            bool overlapBend2ToEnd = false;
            bool overlapBend1ToEnd = false;
            bool overlapStartToEnd = false;
            Corridor connectedCorridor = new Corridor();

            if (!room.corridorEast && ConnectingRoomsInRange(rooms[i], room, 1, foundXGreater, foundYGreater, aligned))
            {
                //testing if found room is below this room
                if ((rooms[i].yPos < room.yPos))
                {
                    //find if we can connect EAST-NORTH
                    if (!rooms[i].corridorNorth && rooms[i].endYPos <= room.yPos)
                    {
                        connectingPointX = rooms[i].xPos + 2;
                        connectingPointY = rooms[i].yPos + 5;
                        bendPos1X = connectingPointX;
                        bendPos1Y = corridorStartY;

                        //TESTING OVERLAP
                        //from startingPos to bend1
                        for (int j = 0; j < (bendPos1X - corridorStartX) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX + j][corridorStartY] == true || occupiedTiles[corridorStartX + j][corridorStartY - 1] == true || occupiedTiles[corridorStartX + j][corridorStartY + 1] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bend1 to connectingPoint
                        for (int j = 0; j < (bendPos1Y - connectingPointY) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X][bendPos1Y - j] == true || occupiedTiles[bendPos1X - 1][bendPos1Y - j] == true || occupiedTiles[bendPos1X + 1][bendPos1Y - j] == true)
                                overlapBend1ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if (!overlapStartToBend1 && !overlapBend1ToEnd)
                        {
                            connectedCorridor.corridorLength += (bendPos1X - corridorStartX + 1);

                            //from startingPos to bend1
                            for (int j = 0; j < (bendPos1X - corridorStartX) + 1; j++)
                            {
                                occupiedTiles[corridorStartX + j][corridorStartY] = true;
                                tiles[corridorStartX + j][corridorStartY] = TileType.Floor;
                                floorType[corridorStartX + j][corridorStartY] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX + j);
                                connectedCorridor.corridorYUnits.Add(corridorStartY);
                            }

                            //from bend1 to connectingPoint
                            for (int j = 1; j < (bendPos1Y - connectingPointY) + 1; j++)
                            {
                                occupiedTiles[bendPos1X][bendPos1Y - j] = true;
                                tiles[bendPos1X][bendPos1Y - j] = TileType.Floor;
                                floorType[bendPos1X][bendPos1Y - j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y - j);
                            }
                            connectedCorridor.corridorLength += (bendPos1Y - connectingPointY);
                            connectingCorridorPlaced = true;
                            room.corridorEast = true;
                            rooms[i].corridorNorth = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                    //else we connect EAST-WEST
                    else if (!rooms[i].corridorWest)
                    {
                        findBendPos1X = (int)(rooms[i].xPos - room.endXPos) / 2;
                        bendPos1X = room.endXPos + findBendPos1X;
                        bendPos1Y = corridorStartY;
                        connectingPointX = rooms[i].xPos - 1;
                        connectingPointY = rooms[i].yPos + 2;
                        bendPos2X = bendPos1X;
                        bendPos2Y = connectingPointY;

                        //TESTING OVERLAP
                        //from startingPoint to bend 1
                        for (int j = 0; j < (bendPos1X - corridorStartX) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX + j][corridorStartY] == true || occupiedTiles[corridorStartX + j][corridorStartY - 1] == true || occupiedTiles[corridorStartX + j][corridorStartY + 1] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bend1 to bend2
                        for (int j = 0; j < (bendPos1Y - bendPos2Y) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X][bendPos1Y - j] == true || occupiedTiles[bendPos1X - 1][bendPos1Y - j] == true || occupiedTiles[bendPos1X + 1][bendPos1Y - j] == true)
                                overlapBend1ToBend2 = true;
                        }
                        //from bend2 to connectingPoint
                        for (int j = 0; j < (connectingPointX - bendPos2X) + 1; j++)
                        {
                            if (occupiedTiles[bendPos2X + j][bendPos2Y] == true || occupiedTiles[bendPos2X + j][bendPos2Y - 1] == true || occupiedTiles[bendPos2X + j][bendPos2Y + 1] == true)
                                overlapBend2ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if (!overlapStartToBend1 && !overlapBend1ToBend2 && !overlapBend2ToEnd)
                        {
                            connectedCorridor.corridorLength += (bendPos1X - corridorStartX + 1);

                            //from startingPoint to bend 1
                            for (int j = 0; j < (bendPos1X - corridorStartX) + 1; j++)
                            {
                                occupiedTiles[corridorStartX + j][corridorStartY] = true;
                                tiles[corridorStartX + j][corridorStartY] = TileType.Floor;
                                floorType[corridorStartX + j][corridorStartY] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX + j);
                                connectedCorridor.corridorYUnits.Add(corridorStartY);
                            }

                            //from bend1 to bend2
                            for (int j = 1; j < (bendPos1Y - bendPos2Y) + 1; j++)
                            {
                                occupiedTiles[bendPos1X][bendPos1Y - j] = true;
                                tiles[bendPos1X][bendPos1Y - j] = TileType.Floor;
                                floorType[bendPos1X][bendPos1Y - j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y - j);
                            }
                            connectedCorridor.corridorLength += (bendPos1Y - bendPos2Y);

                            //from bend2 to connectingPoint
                            for (int j = 1; j < (connectingPointX - bendPos2X) + 1; j++)
                            {
                                occupiedTiles[bendPos2X + j][bendPos2Y] = true;
                                tiles[bendPos2X + j][bendPos2Y] = TileType.Floor;
                                floorType[bendPos2X + j][bendPos2Y] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos2X + j);
                                connectedCorridor.corridorYUnits.Add(bendPos2Y);
                            }
                            connectedCorridor.corridorLength += (connectingPointX - bendPos2X);
                            connectingCorridorPlaced = true;
                            room.corridorEast = true;
                            rooms[i].corridorWest = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                }
                //testing if found room is above this room
                else if ((rooms[i].yPos > room.yPos))
                {
                    //find if we connect EAST-SOUTH
                    if (!rooms[i].corridorSouth && rooms[i].yPos >= room.endYPos)
                    {
                        connectingPointX = rooms[i].xPos + 2;
                        connectingPointY = rooms[i].yPos - 1;
                        bendPos1X = connectingPointX;
                        bendPos1Y = corridorStartY;

                        //TESTING OVERLAP
                        //from startingPos to bend1
                        for (int j = 0; j < (bendPos1X - corridorStartX) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX + j][corridorStartY] == true || occupiedTiles[corridorStartX + j][corridorStartY - 1] == true || occupiedTiles[corridorStartX + j][corridorStartY + 1] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bend1 to connectingPoint
                        for (int j = 0; j < (connectingPointY - bendPos1Y) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X][bendPos1Y + j] == true || occupiedTiles[bendPos1X - 1][bendPos1Y + j] == true || occupiedTiles[bendPos1X + 1][bendPos1Y + j] == true)
                                overlapBend1ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if (!overlapStartToBend1 && !overlapBend1ToEnd)
                        {
                            connectedCorridor.corridorLength += (bendPos1X - corridorStartX + 1);

                            //from startingPos to bend1
                            for (int j = 0; j < (bendPos1X - corridorStartX) + 1; j++)
                            {
                                occupiedTiles[corridorStartX + j][corridorStartY] = true;
                                tiles[corridorStartX + j][corridorStartY] = TileType.Floor;
                                floorType[corridorStartX + j][corridorStartY] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX + j);
                                connectedCorridor.corridorYUnits.Add(corridorStartY);
                            }

                            //from bend1 to connectingPoint
                            for (int j = 1; j < (connectingPointY - bendPos1Y) + 1; j++)
                            {
                                occupiedTiles[bendPos1X][bendPos1Y + j] = true;
                                tiles[bendPos1X][bendPos1Y + j] = TileType.Floor;
                                floorType[bendPos1X][bendPos1Y + j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y + j);
                            }
                            connectedCorridor.corridorLength += (connectingPointY - bendPos1Y);
                            connectingCorridorPlaced = true;
                            room.corridorEast = true;
                            rooms[i].corridorSouth = true;
                            connectedCorridors.Add(connectedCorridor);
                        }

                    }
                    //else we connect EAST-WEST
                    else if (!rooms[i].corridorWest)
                    {
                        findBendPos1X = (int)(rooms[i].xPos - room.endXPos) / 2;
                        bendPos1X = room.endXPos + findBendPos1X;
                        bendPos1Y = corridorStartY;
                        connectingPointX = rooms[i].xPos - 1;
                        connectingPointY = rooms[i].yPos + 2;
                        bendPos2X = bendPos1X;
                        bendPos2Y = connectingPointY;

                        //TESTING OVRLAP
                        //from startingPoint to bend 1
                        for (int j = 0; j < (bendPos1X - corridorStartX) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX + j][corridorStartY] == true || occupiedTiles[corridorStartX + j][corridorStartY - 1] == true || occupiedTiles[corridorStartX + j][corridorStartY + 1] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bend1 to bend2
                        for (int j = 0; j < (bendPos2Y - bendPos1Y) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X][bendPos1Y + j] == true || occupiedTiles[bendPos1X - 1][bendPos1Y + j] == true || occupiedTiles[bendPos1X + 1][bendPos1Y + j] == true)
                                overlapBend1ToBend2 = true;
                        }
                        //from bend2 to connectingPoint
                        for (int j = 0; j < (connectingPointX - bendPos2X) + 1; j++)
                        {
                            if (occupiedTiles[bendPos2X + j][bendPos2Y] == true || occupiedTiles[bendPos2X + j][bendPos2Y - 1] == true || occupiedTiles[bendPos2X + j][bendPos2Y + 1] == true)
                                overlapBend2ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if (!overlapStartToBend1 && !overlapBend1ToBend2 && !overlapBend2ToEnd)
                        {
                            connectedCorridor.corridorLength += (bendPos1X - corridorStartX + 1);

                            //from startingPoint to bend 1
                            for (int j = 0; j < (bendPos1X - corridorStartX) + 1; j++)
                            {
                                occupiedTiles[corridorStartX + j][corridorStartY] = true;
                                tiles[corridorStartX + j][corridorStartY] = TileType.Floor;
                                floorType[corridorStartX + j][corridorStartY] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX + j);
                                connectedCorridor.corridorYUnits.Add(corridorStartY);
                            }

                            //from bend1 to bend2
                            for (int j = 1; j < (bendPos2Y - bendPos1Y) + 1; j++)
                            {
                                occupiedTiles[bendPos1X][bendPos1Y + j] = true;
                                tiles[bendPos1X][bendPos1Y + j] = TileType.Floor;
                                floorType[bendPos1X][bendPos1Y + j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y + j);
                            }
                            connectedCorridor.corridorLength += (bendPos2Y - bendPos1Y);

                            //from bend2 to connectingPoint
                            for (int j = 1; j < (connectingPointX - bendPos2X) + 1; j++)
                            {
                                occupiedTiles[bendPos2X + j][bendPos2Y] = true;
                                tiles[bendPos2X + j][bendPos2Y] = TileType.Floor;
                                floorType[bendPos2X + j][bendPos2Y] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos2X + j);
                                connectedCorridor.corridorYUnits.Add(bendPos2Y);
                            }
                            connectedCorridor.corridorLength += (connectingPointX - bendPos2X);
                            connectingCorridorPlaced = true;
                            room.corridorEast = true;
                            rooms[i].corridorWest = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                }
                //else test if the rooms are aligned and can make a straight corridor
                else if (!rooms[i].corridorWest && aligned && (rooms[i].xPos - room.endXPos < 14 && rooms[i].xPos - room.endXPos > 3))
                {
                    connectingPointX = rooms[i].xPos - 1;
                    connectingPointY = rooms[i].yPos + 2;

                    //TESTING OVERLAP
                    for (int j = 0; j < (connectingPointX - corridorStartX) + 1; j++)
                    {
                        if (occupiedTiles[corridorStartX + j][corridorStartY] == true || occupiedTiles[corridorStartX + j][corridorStartY - 1] == true || occupiedTiles[corridorStartX + j][corridorStartY + 1] == true)
                            overlapStartToEnd = true;
                    }

                    //PLACING CONNECTING CORRIDOR
                    if (!overlapStartToEnd)
                    {
                        connectedCorridor.corridorLength += (connectingPointX - corridorStartX + 1);

                        for (int j = 0; j < (connectingPointX - corridorStartX) + 1; j++)
                        {
                            occupiedTiles[corridorStartX + j][corridorStartY] = true;
                            tiles[corridorStartX + j][corridorStartY] = TileType.Floor;
                            floorType[corridorStartX + j][corridorStartY] = FloorType.CorridorTile;
                            connectedCorridor.corridorXUnits.Add(corridorStartX + j);
                            connectedCorridor.corridorYUnits.Add(corridorStartY);
                        }
                        connectingCorridorPlaced = true;
                        room.corridorEast = true;
                        rooms[i].corridorWest = true;
                        connectedCorridors.Add(connectedCorridor);
                    }
                }
            }
        }
    }

    //check CheckRoomsNorth comments
    private void CheckRoomsSouth(Room room, int sameRoom)
    {
        for (int i = 1; i < rooms.Length; i++)
        {
            if (rooms[i] == null || i == sameRoom)
            {
                continue;
            }

            int corridorStartX = room.xPos + 2;
            int corridorStartY = room.yPos - 1;
            int bendPos1X;
            int findBendPos1Y;
            int bendPos1Y;
            int bendPos2X;
            int bendPos2Y;
            int connectingPointX;
            int connectingPointY;
            bool foundXGreater = FoundXGreater(rooms[i], room);
            bool foundYGreater = FoundYGreater(rooms[i], room);
            bool aligned = RoomsAligned(rooms[i], room);
            bool overlapStartToBend1 = false;
            bool overlapBend1ToBend2 = false;
            bool overlapBend2ToEnd = false;
            bool overlapBend1ToEnd = false;
            bool overlapStartToEnd = false;
            Corridor connectedCorridor = new Corridor();

            if (!room.corridorSouth && ConnectingRoomsInRange(rooms[i], room,2,foundXGreater,foundYGreater,aligned))
            {
                //testing if found room is to the right of this room
                if ((rooms[i].xPos > room.xPos))
                {
                    //find if we need to connect SOUTH-WEST
                    if (!rooms[i].corridorWest && rooms[i].xPos >= room.endXPos)
                    {
                        connectingPointX = rooms[i].xPos - 1;
                        connectingPointY = rooms[i].yPos + 2;
                        bendPos1X = corridorStartX;
                        bendPos1Y = connectingPointY;

                        //TESTING OVERLAP
                        //from starting position to bend1
                        for (int j = 0; j < (corridorStartY - bendPos1Y) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX][corridorStartY - j] == true || occupiedTiles[corridorStartX-1][corridorStartY - j] == true || occupiedTiles[corridorStartX+1][corridorStartY - j] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bend to connectingPoint
                        for (int j = 0; j < (connectingPointX - bendPos1X) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X + j][bendPos1Y] == true || occupiedTiles[bendPos1X + j][bendPos1Y-1] == true || occupiedTiles[bendPos1X + j][bendPos1Y+1] == true)
                                overlapBend1ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if(!overlapStartToBend1 && !overlapBend1ToEnd)
                        {
                            connectedCorridor.corridorLength += (corridorStartY - bendPos1Y + 1);

                            //from starting position to bend1
                            for (int j = 0; j < (corridorStartY - bendPos1Y) + 1; j++)
                            {
                                occupiedTiles[corridorStartX][corridorStartY - j] = true;
                                tiles[corridorStartX][corridorStartY - j] = TileType.Floor;
                                floorType[corridorStartX][corridorStartY - j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX);
                                connectedCorridor.corridorYUnits.Add(corridorStartY - j);
                            }

                            //from bend to connectingPoint
                            for (int j = 1; j < (connectingPointX - bendPos1X) + 1; j++)
                            {
                                occupiedTiles[bendPos1X + j][bendPos1Y] = true;
                                tiles[bendPos1X + j][bendPos1Y] = TileType.Floor;
                                floorType[bendPos1X + j][bendPos1Y] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X + j);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y);
                            }
                            connectedCorridor.corridorLength += (connectingPointX - bendPos1X);
                            connectingCorridorPlaced = true;
                            room.corridorSouth = true;
                            rooms[i].corridorWest = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                    //else we connect SOUTH-NORTH
                    else if (!rooms[i].corridorNorth)
                    {
                        bendPos1X = corridorStartX;
                        findBendPos1Y = (int)(room.yPos - rooms[i].endYPos) / 2;
                        bendPos1Y = room.yPos - findBendPos1Y;
                        connectingPointX = rooms[i].xPos + 2;
                        connectingPointY = rooms[i].yPos + 5;
                        bendPos2X = connectingPointX;
                        bendPos2Y = bendPos1Y;

                        //TESTING OVERLAP
                        //from start of corridor to first bend
                        for (int j = 0; j < (corridorStartY - bendPos1Y) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX][corridorStartY - j] == true || occupiedTiles[corridorStartX-1][corridorStartY - j] == true || occupiedTiles[corridorStartX+1][corridorStartY - j] == true)
                                overlapStartToBend1 = true;
                        }
                        //from first bend to second bend
                        for (int j = 0; j < (bendPos2X - bendPos1X) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X + j][bendPos1Y] == true || occupiedTiles[bendPos1X + j][bendPos1Y-1] == true || occupiedTiles[bendPos1X + j][bendPos1Y+1] == true)
                                overlapBend1ToBend2 = true;
                        }
                        //from second bend to connectingPoint
                        for (int j = 0; j < (bendPos2Y - connectingPointY) + 1; j++)
                        {
                            if (occupiedTiles[bendPos2X][bendPos2Y - j] == true || occupiedTiles[bendPos2X-1][bendPos2Y - j] == true || occupiedTiles[bendPos2X+1][bendPos2Y - j] == true)
                                overlapBend2ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if(!overlapStartToBend1 && !overlapBend1ToBend2 && !overlapBend2ToEnd)
                        {
                            connectedCorridor.corridorLength += (corridorStartY - bendPos1Y + 1);

                            //from start of corridor to first bend
                            for (int j = 0; j < (corridorStartY - bendPos1Y) + 1; j++)
                            {
                                occupiedTiles[corridorStartX][corridorStartY - j] = true;
                                tiles[corridorStartX][corridorStartY - j] = TileType.Floor;
                                floorType[corridorStartX][corridorStartY - j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX);
                                connectedCorridor.corridorYUnits.Add(corridorStartY - j);

                            }

                            //from first bend to second bend
                            for (int j = 1; j < (bendPos2X - bendPos1X) + 1; j++)
                            {
                                occupiedTiles[bendPos1X + j][bendPos1Y] = true;
                                tiles[bendPos1X + j][bendPos1Y] = TileType.Floor;
                                floorType[bendPos1X + j][bendPos1Y] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X + j);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y);
                            }
                            connectedCorridor.corridorLength += (bendPos2X - bendPos1X);

                            //from second bend to connectingPoint
                            for (int j = 1; j < (bendPos2Y - connectingPointY) + 1; j++)
                            {
                                occupiedTiles[bendPos2X][bendPos2Y - j] = true;
                                tiles[bendPos2X][bendPos2Y - j] = TileType.Floor;
                                floorType[bendPos2X][bendPos2Y - j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos2X);
                                connectedCorridor.corridorYUnits.Add(bendPos2Y - j);
                            }
                            connectedCorridor.corridorLength += (bendPos2Y - connectingPointY);
                            connectingCorridorPlaced = true;
                            room.corridorSouth = true;
                            rooms[i].corridorNorth = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                }
                //testing if found room is to the left of this room
                else if ((rooms[i].xPos < room.xPos))
                {
                    //find if we need to connect SOUTH-EAST
                    if (!rooms[i].corridorEast && rooms[i].endXPos <= room.xPos)
                    {
                        connectingPointX = rooms[i].xPos + 5;
                        connectingPointY = rooms[i].yPos + 2;
                        bendPos1X = corridorStartX;
                        bendPos1Y = connectingPointY;

                        //TESTING OVERLAP
                        //from startingPosition to bend1
                        for (int j = 0; j < (corridorStartY - bendPos1Y) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX][corridorStartY - j] == true || occupiedTiles[corridorStartX-1][corridorStartY - j] == true || occupiedTiles[corridorStartX+1][corridorStartY - j] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bendPos to connectingPoint
                        for (int j = 0; j < (bendPos1X - connectingPointX) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X - j][bendPos1Y] == true || occupiedTiles[bendPos1X - j][bendPos1Y-1] == true || occupiedTiles[bendPos1X - j][bendPos1Y+1] == true)
                                overlapBend1ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if(!overlapStartToBend1 && !overlapBend1ToEnd)
                        {
                            connectedCorridor.corridorLength += (corridorStartY - bendPos1Y + 1);

                            //from startingPosition to bend1
                            for (int j = 0; j < (corridorStartY - bendPos1Y) + 1; j++)
                            {
                                occupiedTiles[corridorStartX][corridorStartY - j] = true;
                                tiles[corridorStartX][corridorStartY - j] = TileType.Floor;
                                floorType[corridorStartX][corridorStartY - j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX);
                                connectedCorridor.corridorYUnits.Add(corridorStartY - j);
                            }

                            //from bendPos to connectingPoint
                            for (int j = 1; j < (bendPos1X - connectingPointX) + 1; j++)
                            {
                                occupiedTiles[bendPos1X - j][bendPos1Y] = true;
                                tiles[bendPos1X - j][bendPos1Y] = TileType.Floor;
                                floorType[bendPos1X - j][bendPos1Y] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X - j);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y);
                            }
                            connectedCorridor.corridorLength += (bendPos1X - connectingPointX);
                            connectingCorridorPlaced = true;
                            room.corridorSouth = true;
                            rooms[i].corridorEast = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                    //else we connect SOUTH-NORTH
                    else if (!rooms[i].corridorNorth)
                    {
                        bendPos1X = corridorStartX;
                        findBendPos1Y = (int)(room.yPos - rooms[i].endYPos) / 2;
                        bendPos1Y = room.yPos - findBendPos1Y;
                        connectingPointX = rooms[i].xPos + 2;
                        connectingPointY = rooms[i].yPos + 5;
                        bendPos2X = connectingPointX;
                        bendPos2Y = bendPos1Y;

                        //TEST OVERLAP
                        //from start of corridor to first bend
                        for (int j = 0; j < (corridorStartY - bendPos1Y) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX][corridorStartY - j] == true || occupiedTiles[corridorStartX-1][corridorStartY - j] == true || occupiedTiles[corridorStartX+1][corridorStartY - j] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bendPos1 to bendPos2
                        for (int j = 0; j < (bendPos1X - bendPos2X) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X - j][bendPos1Y] == true || occupiedTiles[bendPos1X - j][bendPos1Y-1] == true || occupiedTiles[bendPos1X - j][bendPos1Y+1] == true)
                                overlapBend1ToBend2 = true;
                        }
                        //from bendPos2 to connectingPoint
                        for (int j = 0; j < (bendPos2Y - connectingPointY) + 1; j++)
                        {
                            if (occupiedTiles[bendPos2X][bendPos2Y - j] == true || occupiedTiles[bendPos2X-1][bendPos2Y - j] == true || occupiedTiles[bendPos2X+1][bendPos2Y - j] == true)
                                overlapBend2ToEnd = true;
                        }

                        //PLACING CONNECTED CORRIDOR
                        if(!overlapStartToBend1 && !overlapBend1ToBend2 && !overlapBend2ToEnd)
                        {
                            connectedCorridor.corridorLength += (corridorStartY - bendPos1Y + 1);

                            //from start of corridor to first bend
                            for (int j = 0; j < (corridorStartY - bendPos1Y) + 1; j++)
                            {
                                occupiedTiles[corridorStartX][corridorStartY - j] = true;
                                tiles[corridorStartX][corridorStartY - j] = TileType.Floor;
                                floorType[corridorStartX][corridorStartY - j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX);
                                connectedCorridor.corridorYUnits.Add(corridorStartY - j);
                            }

                            //from bendPos1 to bendPos2
                            for (int j = 1; j < (bendPos1X - bendPos2X) + 1; j++)
                            {
                                occupiedTiles[bendPos1X - j][bendPos1Y] = true;
                                tiles[bendPos1X - j][bendPos1Y] = TileType.Floor;
                                floorType[bendPos1X - j][bendPos1Y] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X - j);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y);
                            }
                            connectedCorridor.corridorLength += (bendPos1X - bendPos2X);
                            //from bendPos2 to connectingPoint
                            for (int j = 1; j < (bendPos2Y - connectingPointY) + 1; j++)
                            {
                                occupiedTiles[bendPos2X][bendPos2Y - j] = true;
                                tiles[bendPos2X][bendPos2Y - j] = TileType.Floor;
                                floorType[bendPos2X][bendPos2Y - j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos2X);
                                connectedCorridor.corridorYUnits.Add(bendPos2Y - j);
                            }
                            connectedCorridor.corridorLength += (bendPos2Y - connectingPointY);
                            connectingCorridorPlaced = true;
                            room.corridorSouth = true;
                            rooms[i].corridorNorth = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                }
                //else test if the rooms are aligned and can make a straight corridor
                else if (!rooms[i].corridorNorth && aligned && (room.yPos - rooms[i].endYPos < 14 && room.yPos - rooms[i].endYPos > 3))
                {
                    connectingPointX = rooms[i].xPos + 2;
                    connectingPointY = rooms[i].yPos +5;

                    //TESTING OVERLAP
                    for (int j = 0; j < (corridorStartY - connectingPointY) + 1; j++)
                    {
                        if (occupiedTiles[corridorStartX][corridorStartY - j] == true || occupiedTiles[corridorStartX-1][corridorStartY - j] == true || occupiedTiles[corridorStartX+1][corridorStartY - j] == true)
                            overlapStartToEnd = true;
                    }

                    //PLACING CONNECTING CORRIDOR
                    if (!overlapStartToEnd)
                    {
                        connectedCorridor.corridorLength += (corridorStartY - connectingPointY + 1);

                        for (int j = 0; j < (corridorStartY- connectingPointY) + 1; j++)
                        {
                            occupiedTiles[corridorStartX][corridorStartY - j] = true;
                            tiles[corridorStartX][corridorStartY - j] = TileType.Floor;
                            floorType[corridorStartX][corridorStartY - j] = FloorType.CorridorTile;
                            connectedCorridor.corridorXUnits.Add(corridorStartX);
                            connectedCorridor.corridorYUnits.Add(corridorStartY - j);
                        }
                        connectingCorridorPlaced = true;
                        room.corridorSouth = true;
                        rooms[i].corridorNorth = true;
                        connectedCorridors.Add(connectedCorridor);
                    }
                }
            }
        }
    }

    //check CheckRoomsNorth comments
    private void CheckRoomsWest(Room room, int sameRoom)
    {
        for (int i = 1; i < rooms.Length; i++)
        {
            if (rooms[i] == null || i == sameRoom)
            {
                continue;
            }

            int corridorStartX = room.xPos - 1;
            int corridorStartY = room.yPos + 2;
            int findBendPos1X;
            int bendPos1X;
            int bendPos1Y;
            int bendPos2X;
            int bendPos2Y;
            int connectingPointX;
            int connectingPointY;
            bool foundXGreater = FoundXGreater(rooms[i], room);
            bool foundYGreater = FoundYGreater(rooms[i], room);
            bool aligned = RoomsAligned(rooms[i], room);
            bool overlapStartToBend1 = false;
            bool overlapBend1ToBend2 = false;
            bool overlapBend2ToEnd = false;
            bool overlapBend1ToEnd = false;
            bool overlapStartToEnd = false;
            Corridor connectedCorridor = new Corridor();

            if (!room.corridorWest && ConnectingRoomsInRange(rooms[i], room, 3,foundXGreater,foundYGreater,aligned))
            {
                //testing if found room is below this room
                if ((rooms[i].yPos < room.yPos))
                {
                    //find if we can connect WEST-NORTH
                    if (!rooms[i].corridorNorth && rooms[i].endYPos <= room.yPos)
                    {
                        connectingPointX = rooms[i].xPos + 2;
                        connectingPointY = rooms[i].yPos + 5;
                        bendPos1X = connectingPointX;
                        bendPos1Y = corridorStartY;

                        //TESTING OVERLAP
                        //from startingPos to bend1
                        for (int j = 0; j < (corridorStartX - bendPos1X) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX - j][corridorStartY] == true || occupiedTiles[corridorStartX - j][corridorStartY-1] == true || occupiedTiles[corridorStartX - j][corridorStartY+1] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bend1 to connectingPoint
                        for (int j = 0; j < (bendPos1Y - connectingPointY) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X][bendPos1Y - j] == true || occupiedTiles[bendPos1X-1][bendPos1Y - j] == true || occupiedTiles[bendPos1X+1][bendPos1Y - j] == true)
                                overlapBend1ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if(!overlapStartToBend1 && !overlapBend1ToEnd)
                        {
                            connectedCorridor.corridorLength += (corridorStartX - bendPos1X + 1);

                            //from startingPos to bend1
                            for (int j = 0; j < (corridorStartX - bendPos1X) + 1; j++)
                            {
                                occupiedTiles[corridorStartX - j][corridorStartY] = true;
                                tiles[corridorStartX - j][corridorStartY] = TileType.Floor;
                                floorType[corridorStartX - j][corridorStartY] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX - j);
                                connectedCorridor.corridorYUnits.Add(corridorStartY);
                            }

                            //from bend1 to connectingPoint
                            for (int j = 1; j < (bendPos1Y - connectingPointY) + 1; j++)
                            {
                                occupiedTiles[bendPos1X][bendPos1Y - j] = true;
                                tiles[bendPos1X][bendPos1Y - j] = TileType.Floor;
                                floorType[bendPos1X][bendPos1Y - j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y - j);
                            }
                            connectedCorridor.corridorLength += (bendPos1Y - connectingPointY);
                            connectingCorridorPlaced = true;
                            room.corridorWest = true;
                            rooms[i].corridorNorth = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                    //else we connect WEST-EAST
                    else if (!rooms[i].corridorEast)
                    {
                        findBendPos1X = (int)(room.xPos - rooms[i].endXPos) / 2;
                        bendPos1X = room.xPos - findBendPos1X;
                        bendPos1Y = corridorStartY;
                        connectingPointX = rooms[i].xPos + 5;
                        connectingPointY = rooms[i].yPos + 2;
                        bendPos2X = bendPos1X;
                        bendPos2Y = connectingPointY;

                        //TESTING OVERLAP
                        //from startingPoint to bend 1
                        for (int j = 0; j < (corridorStartX - bendPos1X) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX - j][corridorStartY] == true || occupiedTiles[corridorStartX - j][corridorStartY-1] == true || occupiedTiles[corridorStartX - j][corridorStartY+1] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bend1 to bend2
                        for (int j = 0; j < (bendPos1Y - bendPos2Y) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X][bendPos1Y - j] == true || occupiedTiles[bendPos1X-1][bendPos1Y - j] == true || occupiedTiles[bendPos1X+1][bendPos1Y - j] == true)
                                overlapBend1ToBend2 = true;
                        }
                        //from bend2 to connectingPoint
                        for (int j = 0; j < (bendPos2X - connectingPointX) + 1; j++)
                        {
                            if (occupiedTiles[bendPos2X - j][bendPos2Y] == true || occupiedTiles[bendPos2X - j][bendPos2Y-1] == true || occupiedTiles[bendPos2X - j][bendPos2Y+1] == true)
                                overlapBend2ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if(!overlapStartToBend1 && !overlapBend1ToBend2 && !overlapBend2ToEnd)
                        {
                            connectedCorridor.corridorLength += (corridorStartX - bendPos1X + 1);

                            //from startingPoint to bend 1
                            for (int j = 0; j < (corridorStartX - bendPos1X) + 1; j++)
                            {
                                occupiedTiles[corridorStartX - j][corridorStartY] = true;
                                tiles[corridorStartX - j][corridorStartY] = TileType.Floor;
                                floorType[corridorStartX - j][corridorStartY] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX - j);
                                connectedCorridor.corridorYUnits.Add(corridorStartY);
                            }

                            //from bend1 to bend2
                            for (int j = 1; j < (bendPos1Y - bendPos2Y) + 1; j++)
                            {
                                occupiedTiles[bendPos1X][bendPos1Y - j] = true;
                                tiles[bendPos1X][bendPos1Y - j] = TileType.Floor;
                                floorType[bendPos1X][bendPos1Y - j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y - j);
                            }
                            connectedCorridor.corridorLength += (bendPos1Y - bendPos2Y);

                            //from bend2 to connectingPoint
                            for (int j = 1; j < (bendPos2X - connectingPointX) + 1; j++)
                            {
                                occupiedTiles[bendPos2X - j][bendPos2Y] = true;
                                tiles[bendPos2X - j][bendPos2Y] = TileType.Floor;
                                floorType[bendPos2X - j][bendPos2Y] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos2X - j);
                                connectedCorridor.corridorYUnits.Add(bendPos2Y);
                            }
                            connectedCorridor.corridorLength += (bendPos2X - connectingPointX);
                            connectingCorridorPlaced = true;
                            room.corridorWest = true;
                            rooms[i].corridorEast = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                }
                //testing if found room is above this room
                else if ((rooms[i].yPos > room.yPos))
                {
                    //find if we connect WEST-SOUTH
                    if (!rooms[i].corridorSouth && rooms[i].yPos >= room.endYPos)
                    {
                        connectingPointX = rooms[i].xPos + 2;
                        connectingPointY = rooms[i].yPos - 1;
                        bendPos1X = connectingPointX;
                        bendPos1Y = corridorStartY;

                        //TESTING OVERLAP
                        //from startingPos to bend1
                        for (int j = 0; j < (corridorStartX - bendPos1X) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX - j][corridorStartY] == true || occupiedTiles[corridorStartX - j][corridorStartY-1] == true || occupiedTiles[corridorStartX - j][corridorStartY+1] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bend1 to connectingPoint
                        for (int j = 0; j < (connectingPointY - bendPos1Y) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X][bendPos1Y + j] == true || occupiedTiles[bendPos1X-1][bendPos1Y + j] == true || occupiedTiles[bendPos1X+1][bendPos1Y + j] == true)
                                overlapBend1ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if(!overlapStartToBend1 && !overlapBend1ToEnd)
                        {
                            connectedCorridor.corridorLength += (corridorStartX - bendPos1X + 1);

                            //from startingPos to bend1
                            for (int j = 0; j < (corridorStartX - bendPos1X) + 1; j++)
                            {
                                occupiedTiles[corridorStartX - j][corridorStartY] = true;
                                tiles[corridorStartX - j][corridorStartY] = TileType.Floor;
                                floorType[corridorStartX -j][corridorStartY] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX - j);
                                connectedCorridor.corridorYUnits.Add(corridorStartY);
                            }

                            //from bend1 to connectingPoint
                            for (int j = 1; j < (connectingPointY - bendPos1Y) + 1; j++)
                            {
                                occupiedTiles[bendPos1X][bendPos1Y + j] = true;
                                tiles[bendPos1X][bendPos1Y + j] = TileType.Floor;
                                floorType[bendPos1X][bendPos1Y + j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y + j);
                            }
                            connectedCorridor.corridorLength += (connectingPointY - bendPos1Y);
                            connectingCorridorPlaced = true;
                            room.corridorWest = true;
                            rooms[i].corridorSouth = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                    //else we connect WEST-EAST
                    else if (!rooms[i].corridorEast)
                    {
                        findBendPos1X = (int)(room.xPos - rooms[i].endXPos) / 2;
                        bendPos1X = room.xPos - findBendPos1X;
                        bendPos1Y = corridorStartY;
                        connectingPointX = rooms[i].xPos + 5;
                        connectingPointY = rooms[i].yPos + 2;
                        bendPos2X = bendPos1X;
                        bendPos2Y = connectingPointY;

                        //TEST OVERLAP
                        //from startingPoint to bend 1
                        for (int j = 0; j < (corridorStartX - bendPos1X) + 1; j++)
                        {
                            if (occupiedTiles[corridorStartX - j][corridorStartY] == true || occupiedTiles[corridorStartX - j][corridorStartY-1] == true || occupiedTiles[corridorStartX - j][corridorStartY+1] == true)
                                overlapStartToBend1 = true;
                        }
                        //from bend1 to bend2
                        for (int j = 0; j < (bendPos2Y - bendPos1Y) + 1; j++)
                        {
                            if (occupiedTiles[bendPos1X][bendPos1Y + j] == true || occupiedTiles[bendPos1X-1][bendPos1Y + j] == true || occupiedTiles[bendPos1X+1][bendPos1Y + j] == true)
                                overlapBend1ToBend2 = true;
                        }
                        //from bend2 to connectingPoint
                        for (int j = 0; j < (bendPos2X - connectingPointX) + 1; j++)
                        {
                            if (occupiedTiles[bendPos2X - j][bendPos2Y] == true || occupiedTiles[bendPos2X - j][bendPos2Y-1] == true || occupiedTiles[bendPos2X - j][bendPos2Y+1] == true)
                                overlapBend2ToEnd = true;
                        }

                        //PLACING CONNECTING CORRIDOR
                        if(!overlapStartToBend1 && !overlapBend1ToBend2 && !overlapBend2ToEnd)
                        {
                            connectedCorridor.corridorLength += (corridorStartX - bendPos1X + 1);

                            //from startingPoint to bend 1
                            for (int j = 0; j < (corridorStartX - bendPos1X) + 1; j++)
                            {
                                occupiedTiles[corridorStartX - j][corridorStartY] = true;
                                tiles[corridorStartX - j][corridorStartY] = TileType.Floor;
                                floorType[corridorStartX -j][corridorStartY] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(corridorStartX - j);
                                connectedCorridor.corridorYUnits.Add(corridorStartY);
                            }

                            //from bend1 to bend2
                            for (int j = 1; j < (bendPos2Y - bendPos1Y) + 1; j++)
                            {
                                occupiedTiles[bendPos1X][bendPos1Y + j] = true;
                                tiles[bendPos1X][bendPos1Y + j] = TileType.Floor;
                                floorType[bendPos1X][bendPos1Y + j] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos1X);
                                connectedCorridor.corridorYUnits.Add(bendPos1Y + j);
                            }
                            connectedCorridor.corridorLength += (bendPos2Y - bendPos1Y);

                            //from bend2 to connectingPoint
                            for (int j = 1; j < (bendPos2X - connectingPointX) + 1; j++)
                            {
                                occupiedTiles[bendPos2X - j][bendPos2Y] = true;
                                tiles[bendPos2X - j][bendPos2Y] = TileType.Floor;
                                floorType[bendPos2X - j][bendPos2Y] = FloorType.CorridorTile;
                                connectedCorridor.corridorXUnits.Add(bendPos2X - j);
                                connectedCorridor.corridorYUnits.Add(bendPos2Y);
                            }
                            connectedCorridor.corridorLength += (bendPos2X - connectingPointX);
                            connectingCorridorPlaced = true;
                            room.corridorWest = true;
                            rooms[i].corridorEast = true;
                            connectedCorridors.Add(connectedCorridor);
                        }
                    }
                }
                //else test if the rooms are aligned and can make a straight corridor
                else if (!rooms[i].corridorEast && aligned && (room.xPos - rooms[i].endXPos < 14 && room.xPos - rooms[i].endXPos > 3))
                {
                    connectingPointX = rooms[i].xPos + 5;
                    connectingPointY = rooms[i].yPos + 2;

                    //TEST OVERLAP
                    for (int j = 0; j < (corridorStartX - connectingPointX) + 1; j++)
                    {
                        if (occupiedTiles[corridorStartX - j][corridorStartY] == true || occupiedTiles[corridorStartX - j][corridorStartY-1] == true || occupiedTiles[corridorStartX - j][corridorStartY+1] == true)
                            overlapStartToEnd = true;
                    }

                    //PLACING CONNECTING CORRIDOR
                    if (!overlapStartToEnd)
                    {
                        connectedCorridor.corridorLength += (corridorStartX - connectingPointX + 1);

                        for (int j = 0; j < (corridorStartX - connectingPointX) + 1; j++)
                        {
                            occupiedTiles[corridorStartX - j][corridorStartY] = true;
                            tiles[corridorStartX - j][corridorStartY] = TileType.Floor;
                            floorType[corridorStartX - j][corridorStartY] = FloorType.CorridorTile;
                            connectedCorridor.corridorXUnits.Add(corridorStartX - j);
                            connectedCorridor.corridorYUnits.Add(corridorStartY);
                        }
                        connectingCorridorPlaced = true;
                        room.corridorWest = true;
                        rooms[i].corridorEast = true;
                        connectedCorridors.Add(connectedCorridor);
                    }
                }
            }
        }
    }

    //returns true if the length of the corridor would not exceed 12 units
    private bool ConnectingRoomsInRange(Room foundRoom, Room currentRoom, int direction, bool foundXGreater, bool foundYGreater, bool aligned)
    {
        int corridorLength = 0;
        int lengthX = 0;
        int lengthY = 0;

        switch (direction)
        {
            //direction = North
            case 0:
                //rooms are aligned NORTH-SOUTH
                if (aligned)
                {
                    lengthX = 0;
                    lengthY = (foundRoom.yPos - 1) - (currentRoom.endYPos);
                    break;
                }
                //found room is to the right of current room
                else if (foundXGreater)
                {
                    //connecting NORTH-WEST
                    if(foundRoom.xPos > currentRoom.endXPos)
                    {
                        lengthX = (foundRoom.xPos) - (currentRoom.xPos + 2);
                        lengthY = (foundRoom.yPos + 2) - (currentRoom.endYPos + 1);
                    }
                    //connecting NORTH-SOUTH
                    else
                    {
                        lengthX = (foundRoom.xPos) - (currentRoom.xPos);
                        lengthY = (foundRoom.yPos - 1) - (currentRoom.endYPos);
                    }
                }
                //found room is to the left of current room
                else
                {
                    //connecting NORTH-EAST
                    if(foundRoom.endXPos <= currentRoom.xPos)
                    {
                        lengthX = (currentRoom.xPos + 2) - (foundRoom.endXPos + 1);
                        lengthY = (foundRoom.yPos + 2) - (currentRoom.endYPos);
                    }
                    //connecting NORTH-SOUTH
                    else
                    {
                        lengthX = (currentRoom.xPos) - (foundRoom.xPos);
                        lengthY = (foundRoom.yPos - 1) - (currentRoom.endYPos);
                    }
                }
                break;
            //direction = East
            case 1:
                //rooms are aligned EAST-WEST
                if (aligned)
                {
                    lengthX = (foundRoom.xPos) - (currentRoom.endXPos + 1);
                    lengthY = 0;
                    break;
                }
                //found room is above current room
                else if (foundYGreater)
                {
                    //conecting EAST-SOUTH
                    if(foundRoom.yPos >= currentRoom.endYPos)
                    {
                        lengthX = (foundRoom.xPos + 2) - (currentRoom.endXPos + 1);
                        lengthY = (foundRoom.yPos) - (currentRoom.yPos + 2);
                    }
                    //connecting EAST-WEST
                    else
                    {
                        lengthX = (foundRoom.xPos) - (currentRoom.endXPos + 1);
                        lengthY = (foundRoom.yPos) - (currentRoom.yPos);
                    }
                }
                //found room is below current room
                else
                {
                    //connecting EAST-NORTH
                    if(foundRoom.endYPos <= currentRoom.yPos)
                    {
                        lengthX = (foundRoom.xPos + 2) - (currentRoom.endXPos + 1);
                        lengthY = (currentRoom.yPos + 2) - (foundRoom.endYPos);
                    }
                    //connecting EAST-WEST
                    else
                    {
                        lengthX = (foundRoom.xPos) - (currentRoom.endXPos + 1);
                        lengthY = (currentRoom.yPos) - (foundRoom.yPos);
                    }
                }
                break;
            //direction = South
            case 2:
                //rooms are aligned SOUTH-NORTH
                if (aligned)
                {
                    lengthX = 0;
                    lengthY = (currentRoom.yPos) - (foundRoom.endYPos+1);
                    break;
                }
                //found room is to the right of current room
                else if (foundXGreater)
                {
                    //connecting SOUTH-WEST
                    if (foundRoom.xPos >= currentRoom.endXPos)
                    {
                        lengthX = (foundRoom.xPos) - (currentRoom.xPos + 2);
                        lengthY = (currentRoom.yPos -1) - (foundRoom.yPos + 2);
                    }
                    //conecting SOUTH-NORTH
                    else
                    {
                        lengthX = (foundRoom.xPos) - currentRoom.xPos;
                        lengthY = (currentRoom.yPos-1) - foundRoom.endYPos;
                    }
                }
                //found room is to the left of current room
                else
                {
                    //connecting SOUTH-EAST
                    if(foundRoom.endXPos <= currentRoom.xPos)
                    {
                        lengthX = (currentRoom.xPos + 2) - (foundRoom.endXPos);
                        lengthY = (currentRoom.yPos - 1) - (foundRoom.yPos + 2);
                     }
                    //connecting SOUTH-NORTH
                    else
                    {
                        lengthX = (currentRoom.xPos) - (foundRoom.xPos);
                        lengthY = (currentRoom.yPos) - (foundRoom.endYPos + 1);
                    }
                }
                break;
            //direction = West
            case 3:
                //rooms are aligned, WEST-EAST
                if (aligned)
                {
                    lengthX = currentRoom.xPos - (foundRoom.endXPos + 1);
                    lengthY = 0;
                    break;
                }
                //found room is above current room
                else if (foundYGreater)
                {
                    //connecting WEST-SOUTH
                    if(foundRoom.yPos >= currentRoom.endYPos)
                    {
                        lengthX = (currentRoom.xPos) - (foundRoom.xPos + 2);
                        lengthY = (foundRoom.yPos-1) - (currentRoom.yPos + 2);
                    }
                    //connecting WEST-EAST
                    else
                    {
                        lengthX = (currentRoom.xPos) - (foundRoom.endXPos + 1);
                        lengthY = (foundRoom.yPos) - (currentRoom.yPos);
                    }
                }
                //found room is below current room
                else
                {
                    //connecting WEST-NORTH
                    if (foundRoom.endYPos <= currentRoom.yPos)
                    {
                        lengthX = (currentRoom.xPos) - (foundRoom.xPos + 2);
                        lengthY = ((currentRoom.yPos + 2) - (foundRoom.endYPos + 1));
                    }
                    //connecting WEST-EAST
                    else
                    {
                        lengthX = ((currentRoom.xPos - 1) - (foundRoom.endXPos));
                        lengthY = (currentRoom.yPos - foundRoom.yPos);
                    } 
                }
                break;
        }
        if (lengthX < 0 || lengthY < 0)
            return false;

        corridorLength = lengthX + lengthY;

        if (corridorLength < maxLengthCorridor + 1)
            return true;
        else
            return false;
    }

    //when connecting rooms, checks to see if the found rooms xPos is greater than the current rooms xPos
    private bool FoundXGreater(Room foundRoom, Room currentRoom)
    {
        if (foundRoom.xPos > currentRoom.xPos)
            return true;
        else
            return false;
    }

    //when connecting rooms, checks to see if the found rooms yPos is greater than the current rooms yPos
    private bool FoundYGreater(Room foundRoom, Room currentRoom)
    {
        if (foundRoom.yPos > currentRoom.yPos)
            return true;
        else
            return false;
    }

    //when connecting rooms, checks to see if the found room and current room are aligned
    private bool RoomsAligned(Room foundRoom, Room currentRoom)
    {
        if (foundRoom.xPos == currentRoom.xPos || foundRoom.yPos == currentRoom.yPos)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}

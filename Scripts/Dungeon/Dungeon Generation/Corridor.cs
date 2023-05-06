using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    North, East, South, West,
}

//first and last corridor units are not in the room but flush with them
public class Corridor
{
    public int startXPos;
    public int startYPos;
    public int endPositionX;            //These are being accessed by Room() to find the last X&Y positions of the 
    public int endPositionY;            //corridor after the corridor has already been made (in Room class)
    public int bendXPos;                //BendX&Y start at the last straight corridor unit
    public int bendYPos;
    public int corridorLength;
    public int dungeonbuffer = 27;      //buffer for how far rooms/corridors can be made from the border
    private int corridorBendRange = 25; //prob of corridor to bend
    private int roomToSearch;           //Randomly find a room to search if corridor couln't find a position on the first room
    private int searchLimit = 25;       //Keeps track on the limit of searches one corridor can make on a random room before being scrapped
    public int bendPlacement;           //creates a random number for where a bend can start
    private int minCorridorBendPlacement = 4;
    public int corridorBendLength;      //this is how many units will be branching off in the new bend direction (excluding the bend Pos)
    public bool[] discovered;           //tracks if any tile in the corridor has already been discovered

    public Direction direction;
    public Direction bendDirection;
    public Direction oppositeDirection; //Is the opposite direction that the corridor was placed in the room

    public bool corridorBend = false;   //if the corridor has a bend
    public bool corridorPlaced = false; //if this corridor and (attached) room have found a suitable direction to be placed

    public List<int> corridorXUnits = new List<int>();  //keeps track of each xPos tile of corridor
    public List<int> corridorYUnits = new List<int>();  //keeps track of each yPos tile of corridor
    public List<List<Event.EventType>> possibleEvents = new List<List<Event.EventType>>();    //holds a matrix list for each tile within this corridor, a flag for list of events per tile

    public int GetNoBendEndPositionX()
    {
        if (direction == Direction.North || direction == Direction.South)
        {
            return startXPos;
        }
        if (direction == Direction.East)
        {
            return startXPos + corridorLength - 1;
        }
        return startXPos - corridorLength + 1;
    }

    public int GetNoBendEndPositionY()
    {
        if (direction == Direction.East || direction == Direction.West)
        {
            return startYPos;
        }
        if (direction == Direction.North)
        {
            return startYPos + corridorLength - 1;
        }
        return startYPos - corridorLength + 1;
    }


    //sets up a new corridor
    //room is the previous room placed, what this corridor is coming out from
    public void SetUpCorridor(List<Room> createdRooms, Room room, RandomInt length, int roomWidth, int roomHeight, int columns, int rows, bool firstCorridor, bool[][] occupiedTiles)
    {
        searchLimit--;
        if (!firstCorridor)
        {
            roomToSearch = Random.Range(1, createdRooms.Count - 1);
            room = createdRooms[roomToSearch];
        }
        else
        {
            room = createdRooms[0];
        }

        direction = (Direction)Random.Range(0, 4);

        corridorLength = length.Random;
        //discovered = new bool[corridorLength];

        if (corridorLength > minCorridorBendPlacement)
        {
            bendPlacement = Random.Range(minCorridorBendPlacement, corridorLength - 1);
        }
        CalculateCorridorStartPos(direction, room);

        // test if the corridor can bend
        if (corridorLength > minCorridorBendPlacement)
        {
            int bendRange = UnityEngine.Random.Range(1, 100);

            //test if corridor is the 25% that will bend
            if (bendRange <= corridorBendRange && !OutOfBounds(direction, room, rows, columns, startXPos, startYPos, corridorLength))
            {
                corridorBend = true;

                //this is how many units will be branching off in the new bend direction (excluding the bend Pos)
                corridorBendLength = corridorLength - bendPlacement;

                //change the direction of the corridor from the corridors bendPlacement unit
                bendDirection = CreateCorridorBendDirection(direction, corridorBendLength, rows, columns);
            }
        }

        //here we check if the orignal direction of the corridor (bend or not) will go OOB or overlap
        //if neither then we skip to the end of this method and calculate the end positions of our corridor
        if (!corridorBend)
        {
            if ((!OutOfBounds(direction, room, rows, columns, startXPos, startYPos, corridorLength) && !CalculateOverlap(direction, startXPos, startYPos, corridorLength, occupiedTiles)))
            {
                corridorPlaced = true;
            }
        }
        else if (corridorBend)
        {
            //if the corridor is bending have to check the orignal direction won't go OOB or overlap as well with the bend direction
            if ((!OutOfBoundsBend(bendDirection, room, rows, columns, bendXPos, bendYPos, corridorBendLength) && !OutOfBounds(direction, room, rows, columns, startXPos, startYPos, bendPlacement - 2))
                && (!CalculateOverlap(direction, startXPos, startYPos, bendPlacement - 2, occupiedTiles) && !CalculateBendOverlap(bendDirection, bendXPos, bendYPos, corridorBendLength, occupiedTiles)))
            {
                corridorPlaced = true;
            }
        }

    //The corridor has been placed and set that corridorDirection to true
    if (corridorPlaced)
    {
        if (direction == (Direction)0)
        {
            room.corridorNorth = true;
        }
        else if (direction == (Direction)1)
        {
            room.corridorEast = true;
        }
        else if (direction == (Direction)2)
        {
            room.corridorSouth = true;
        }
        else if (direction == (Direction)3)
        {
            room.corridorWest = true;
        }
    }

    //********THIS PORTION OF THE CODE TESTS IF THE CORRIDOR THAT HAS ALREADY BEEN CREATED HAS NOT ALREADY BEEN PLACED ***********
    /*check to see if placing the corridor (either bending or not) & (attached) room in this direction will go OOB or Overlap
    //if so find a new direction for the corridor in the same room


    //if corridor has a bend and doesn't OOB or Overlap place that corridor in that found direction, updating all required variables
    //bendPlacement-2 because: the corridor has 'bendPlacement' units
    //-2 because the attached room "juts out" 2 units, but out dungeon buffer tests for 5 units for an attached room
    //(realistically bendPlacement-3 should work but i'm playing it safe with bendPlacement-2)*/
    if (!corridorPlaced && corridorBend && ((OutOfBoundsBend(bendDirection, room, rows, columns, bendXPos, bendYPos, corridorBendLength) || OutOfBounds(direction, room, rows, columns, startXPos, startYPos, -2))
            || CalculateBendOverlap(bendDirection, bendXPos, bendYPos, corridorBendLength, occupiedTiles) || CalculateOverlap(direction, startXPos, startYPos, bendPlacement - 2, occupiedTiles)))
        {
            bool goNorth = false;
            if (Random.value < 0.5f)
                goNorth = true;

            //randomly check North first if this room has an available north corridor, then move to east, then south, then west
            if (goNorth)
            {
                CheckNorthAndEast(room, bendDirection, rows, columns, corridorLength, corridorBendLength, occupiedTiles, corridorPlaced);
                //else check if the south and west are open if the corridor hasn't been set yet
                if (!corridorPlaced)
                {
                    CheckSouthAndWest(room, bendDirection, rows, columns, corridorLength, corridorBendLength, occupiedTiles, corridorPlaced);
                }
            }
            //Else we start south then check west, then check north, then check east
            else if (!goNorth)
            {
                CheckSouthAndWest(room, bendDirection, rows, columns, corridorLength, corridorBendLength, occupiedTiles, corridorPlaced);
                if (!corridorPlaced)
                {
                    CheckNorthAndEast(room, bendDirection, rows, columns, corridorLength, corridorBendLength, occupiedTiles, corridorPlaced);
                }
            }
        }

        //if corridor doesn't have a bend, check for other spots in this room 
        //Placing a corridor a found direction if it doesn't OOB or Overalp, updating all required variables
        //corridorLength has a "-1" becuase the method starts at X&YPos and adds the length to that, this isn't a problem for a bend corridor
        else if (!corridorPlaced && !corridorBend && (OutOfBounds(direction, room, rows, columns, startXPos, startYPos, corridorLength) || CalculateOverlap(direction, startXPos, startYPos, corridorLength, occupiedTiles)))
        {

            //checks if a direction has been made, to make sure we don't need to find another room after all the checks
            bool goNorth = false;
            if (Random.value < 0.5f)
                goNorth = true;

            //randomly check North first if this room has an available north corridor, then move to east, then south, then west
            if (goNorth)
            {
                CheckNorthAndEast(room, direction, rows, columns, corridorLength, 0, occupiedTiles, corridorPlaced);
                //else check if the south and west are open if the corridor hasn't been set yet
                if (!corridorPlaced)
                {
                    CheckSouthAndWest(room, direction, rows, columns, corridorLength, 0, occupiedTiles, corridorPlaced);
                }
            }
            //Else we start south then check west, then check north, then check east
            else if (!goNorth)
            {
                CheckSouthAndWest(room, direction, rows, columns, corridorLength, 0, occupiedTiles, corridorPlaced);
                if (!corridorPlaced)
                {
                    CheckNorthAndEast(room, direction, rows, columns, corridorLength, 0, occupiedTiles, corridorPlaced);
                }
            }
        }

        //checking if there is no bend in this corridor then we can default the endX&Y positions
        if (!corridorBend && corridorPlaced)
        {
            endPositionX = GetNoBendEndPositionX();
            endPositionY = GetNoBendEndPositionY();
        }

        //Find the opposite direction (if the corridor bended find that direction instead), to set the next corridor in Board script to true
        if (!corridorBend)
        {
            CalculateOppositeDirection(direction);
        }
        else if (corridorBend)
        {
            CalculateOppositeDirection(bendDirection);
        }

        //If we reach this part of the code and corridorPlaced == false, then every position of this room does not work for this corridor
        //then: go back -1 room position (if that room is not null then) find a potential room-corridor match and keep going until all 
        //rooms have been searched
        if (!corridorPlaced && searchLimit > 0)
        {
            corridorBend = false;
            SetUpCorridor(createdRooms, createdRooms[roomToSearch], length, roomWidth, roomHeight, columns, rows, false, occupiedTiles);
        }

    }

    //set's the corridor's starting coordinates based on what direction it's going
    private void CalculateCorridorStartPos(Direction direction, Room room)
    {
        switch (direction)
        {
            case Direction.North:
                //the starting X location of corridor based off the room that was just created
                startXPos = room.xPos + 2;
                startYPos = room.yPos + room.roomHeight;
                break;
            case Direction.East:
                startXPos = room.xPos + room.roomWidth;
                startYPos = room.yPos + 2;
                break;
            case Direction.South:
                startXPos = room.xPos + 2;
                startYPos = room.yPos - 1;
                break;
            case Direction.West:
                startXPos = room.xPos - 1;
                startYPos = room.yPos + 2;
                break;
        }
    }

    //test to see if the corridor will go out bounds given its length and a attached room size
    //If the corridor and room will go out of bounds, we can say that there "already is a corridor in this direction"
    //to prevent another corridor from going this way
    //int X&YPos is the starting position of the corridor
    //room is the previous room, or the room this corridor is coming out of
    //returns true if the corridor or room goes out of bounds
    private bool OutOfBounds(Direction corridorDirection, Room room, int rows, int columns, int xPos, int yPos, int corridorLength)
    {
        switch (corridorDirection)
        {
            case Direction.North:
                if (rows - yPos < corridorLength + dungeonbuffer)
                {
                    //room.corridorNorth = true;
                    return true;
                }
                break;
            case Direction.East:
                if (columns - xPos < corridorLength + dungeonbuffer)
                {
                    // room.corridorEast = true;
                    return true;
                }
                break;
            case Direction.South:
                if (yPos - corridorLength - dungeonbuffer < 0)
                {
                    //room.corridorSouth = true;
                    return true;
                }
                break;
            case Direction.West:
                if (xPos - corridorLength - dungeonbuffer < 0)
                {
                    //room.corridorWest = true;
                    return true;
                }
                break;
        }

        return false;
    }
     
    //Testing Out Of Bounds only for the bending portion of the corridor
    //As to not set the direction of open Room to true accidently
    private bool OutOfBoundsBend(Direction corridorBendDirection, Room room, int rows, int columns, int xPos, int yPos, int corridorBendLength)
    {
        switch (corridorBendDirection)
        {
            case Direction.North:
                if (rows - yPos < corridorBendLength + dungeonbuffer)
                {
                    return true;
                }
                break;
            case Direction.East:
                if (columns - xPos < corridorBendLength + dungeonbuffer)
                {
                    return true;
                }
                break;
            case Direction.South:
                if (yPos - corridorBendLength - dungeonbuffer < 0)
                {
                    return true;
                }
                break;
            case Direction.West:
                if (xPos - corridorBendLength - dungeonbuffer < 0)
                {
                    return true;
                }
                break;
        }

        return false;
    }

    /*If the corridor is bending test which way it can bend based on where the Dungeonbuffer is
    direction = the original direction of the corridor
    corridorBendLength = how long the bend will be
    corridors startx and y positions are already set
    updates the bend X & Y position (where that bend is happening)
    updates the endPositionX&Y -of corridor 
    return the direction of the bend */
    public Direction CreateCorridorBendDirection(Direction direction, int corridorBendLength, int rows, int columns)
    {
        bool changeDirection = true;

        if (Random.value < 0.5f)
            changeDirection = false;

        switch (direction)
        {
            case Direction.North:
                //set the known bend x & y position
                bendXPos = startXPos;
                bendYPos = startYPos + bendPlacement - 1;
                //if the bend and dungeonbuffer will go out of bounds east, bend west
                if (columns - startXPos < corridorBendLength + dungeonbuffer)
                {
                    bendDirection = (Direction)3;
                    endPositionX = bendXPos - corridorBendLength;
                    endPositionY = bendYPos;
                }
                //if corridor is on top left, bend east
                else if (startXPos < corridorBendLength + dungeonbuffer)
                {
                    bendDirection = (Direction)1;
                    endPositionX = bendXPos + corridorBendLength;
                    endPositionY = bendYPos;
                }
                //else if it's neither randomly choose a direction
                else
                {
                    if (changeDirection)
                    {
                        //bend west
                        bendDirection = (Direction)3;
                        endPositionX = bendXPos - corridorBendLength;
                        endPositionY = bendYPos;
                    }
                    else
                    {
                        //bend east
                        bendDirection = (Direction)1;
                        endPositionX = bendXPos + corridorBendLength;
                        endPositionY = bendYPos;
                    }
                }
                break;
            case Direction.East:
                bendXPos = startXPos + bendPlacement - 1;
                bendYPos = startYPos;
                //if corridor is top then bend south
                if (rows - bendYPos < corridorBendLength + dungeonbuffer)
                {
                    //bend south
                    bendDirection = (Direction)2;
                    endPositionX = bendXPos;
                    endPositionY = bendYPos - corridorBendLength;
                }
                //if corridor is bottom, bend north
                else if (startYPos < corridorBendLength + dungeonbuffer)
                {
                    bendDirection = (Direction)0;
                    endPositionX = bendXPos;
                    endPositionY = bendYPos + corridorBendLength;
                }
                else
                {
                    if (changeDirection)
                    {
                        bendDirection = (Direction)2;
                        endPositionX = bendXPos;
                        endPositionY = bendYPos - corridorBendLength;
                    }
                    else
                    {
                        bendDirection = (Direction)0;
                        endPositionX = bendXPos;
                        endPositionY = bendYPos + corridorBendLength;
                    }
                }
                break;
            case Direction.South:
                bendXPos = startXPos;
                bendYPos = startYPos - bendPlacement + 1;
                //if corridor is right, bend west
                if (columns - startXPos < corridorBendLength + dungeonbuffer)
                {
                    bendDirection = (Direction)3;
                    endPositionX = bendXPos - corridorBendLength;
                    endPositionY = bendYPos;
                }
                //if corridor is left, bend east
                else if (startXPos < corridorBendLength + dungeonbuffer)
                {
                    bendDirection = (Direction)1;
                    endPositionX = bendXPos + corridorBendLength;
                    endPositionY = bendYPos;
                }
                else
                {
                    if (changeDirection)
                    {
                        bendDirection = (Direction)3;
                        endPositionX = bendXPos - corridorBendLength;
                        endPositionY = bendYPos;
                    }
                    else
                    {
                        bendDirection = (Direction)1;
                        endPositionX = bendXPos + corridorBendLength;
                        endPositionY = bendYPos;
                    }
                }
                break;
            case Direction.West:
                bendXPos = startXPos - bendPlacement + 1;
                bendYPos = startYPos;
                //if corridor is top, bend south
                if (rows - startYPos < corridorBendLength + dungeonbuffer)
                {
                    bendDirection = (Direction)2;
                    endPositionX = bendXPos;
                    endPositionY = bendYPos - corridorBendLength;
                }
                //if corridor is bottom, bend north
                else if (startYPos < corridorBendLength + dungeonbuffer)
                {
                    bendDirection = (Direction)0;
                    endPositionX = bendXPos;
                    endPositionY = bendYPos + corridorBendLength;
                }
                else
                {
                    if (changeDirection)
                    {
                        bendDirection = (Direction)2;
                        endPositionX = bendXPos;
                        endPositionY = bendYPos - corridorBendLength;
                    }
                    else
                    {
                        bendDirection = (Direction)0;
                        endPositionX = bendXPos;
                        endPositionY = bendYPos + corridorBendLength;
                    }
                }
                break;
        }
        return bendDirection;
    }

    //check if there will be overlapping with another room or corridor if this corridor and room is placed here
    //startX&Y is starting coords of corridor
    //roomX and roomY are the coords of the potential attached room
    //returns true if there is any overlapping
    private bool CalculateOverlap(Direction direction, int startX, int startY, int corridorLength, bool[][] occupiedTiles)
    {
        int roomX;
        int roomY;

        switch (direction)
        {
            case Direction.North:
                roomX = startX - 2;
                roomY = startY + corridorLength;
                //"corridorLength -1" is to not check for the last unit of the "straight" corridor top and bottom for overlap
                for (int i = 0; i < corridorLength; i++)
                {
                    if (occupiedTiles[startX][startY + i] || (occupiedTiles[startX - 1][startY + i] || occupiedTiles[startX + 1][startY + i] && i != corridorLength - 1))
                    {
                        return true;
                    }
                }
                if (!corridorBend && CalculateRoomOverlap(roomX, roomY, occupiedTiles))
                {
                    return true;
                }

                break;
            case Direction.East:
                roomX = startX + corridorLength;
                roomY = startY - 2;
                for (int i = 0; i < corridorLength; i++)
                {
                    if (occupiedTiles[startX + i][startY] || (occupiedTiles[startX + i][startY + 1] || occupiedTiles[startX + i][startY - 1] && i != corridorLength - 1))
                    {
                        return true;
                    }              
                }
                if (!corridorBend && CalculateRoomOverlap(roomX, roomY, occupiedTiles))
                {
                    return true;
                }

                break;
            case Direction.South:
                roomX = startX - 2;
                roomY = startY - corridorLength - 4;
                for (int i = 0; i < corridorLength; i++)
                {
                    if (occupiedTiles[startX][startY - i] || (occupiedTiles[startX - 1][startY - i] || occupiedTiles[startX + 1][startY - i] && i != corridorLength - 1))
                    {
                        return true;
                    }

                }
                if (!corridorBend && CalculateRoomOverlap(roomX, roomY, occupiedTiles))
                {
                    return true;
                }

                break;
            case Direction.West:
                roomX = startX - corridorLength - 4;
                roomY = startY - 2;
                for (int i = 0; i < corridorLength; i++)
                {
                    if (occupiedTiles[startX - i][startY] || (occupiedTiles[startX - i][startY + 1] || occupiedTiles[startX - i][startY - 1] && i != corridorLength - 1))
                    {
                        return true;
                    }

                }
                if (!corridorBend && CalculateRoomOverlap(roomX, roomY, occupiedTiles))
                {
                    return true;
                }

                break;
        }

        return false;
    }

    //This is only used for testing if the corridor is bending and seeing if there room-
    //attached to this bent corridor will also overlap
    //Are adding the plus one everywhere as the BendX&Y starts at the last unit of the straight corridor-
    //while the bendLength is excluding the starting BendX&Y coord
    private bool CalculateBendOverlap(Direction direction, int startX, int startY, int corridorLength, bool[][] occupiedTiles)
    {
        int roomX;
        int roomY;

        switch (direction)
        {
            case Direction.North:
                roomX = startX - 2;
                roomY = startY + corridorLength + 1;
                for (int i = 0; i < corridorLength + 1; i++)
                {
                    //i!= will not check the bend unit as it will always return true due to straight part
                    if (occupiedTiles[startX][startY + i] || (occupiedTiles[startX - 1][startY + i] || occupiedTiles[startX + 1][startY + i] && i != 0))
                    {
                        return true;
                    }
                    //if i==0, then check if there's an occupied tile 1 unit in the opposite direction (so corridor is not flush)
                    if(i==0 && occupiedTiles[startX][startY-1] == true)
                    {
                        return true;
                    }
                }
                if (CalculateRoomOverlap(roomX, roomY, occupiedTiles))
                {
                    return true;
                }

                break;
            case Direction.East:
                roomX = startX + corridorLength + 1;
                roomY = startY - 2;
                for (int i = 0; i < corridorLength + 1; i++)
                {
                    if (occupiedTiles[startX + i][startY] || (occupiedTiles[startX + i][startY + 1] || occupiedTiles[startX + i][startY - 1] && i != 0))
                    {
                        return true;
                    }
                    if(i==0 && occupiedTiles[startX-1][startY] == true)
                    {
                        return true;
                    }
                }
                if (CalculateRoomOverlap(roomX, roomY, occupiedTiles))
                {
                    return true;
                }

                break;
            case Direction.South:
                roomX = startX - 2;
                roomY = startY - corridorLength - 4 - 1;
                for (int i = 0; i < corridorLength + 1; i++)
                {
                    if (occupiedTiles[startX][startY - i] || (occupiedTiles[startX - 1][startY - i] || occupiedTiles[startX + 1][startY - i] && i != 0))
                    {
                        return true;
                    }
                    if (i == 0 && occupiedTiles[startX][startY + 1] == true)
                    {
                        return true;
                    }
                }
                if (CalculateRoomOverlap(roomX, roomY, occupiedTiles))
                {
                    return true;
                }

                break;
            case Direction.West:
                roomX = startX - corridorLength - 4 - 1;
                roomY = startY - 2;
                for (int i = 0; i < corridorLength + 1; i++)
                {
                    if (occupiedTiles[startX - i][startY] || (occupiedTiles[startX - i][startY + 1] || occupiedTiles[startX - i][startY - 1] && i != 0))
                    {
                        return true;
                    }
                    if(i==0 && occupiedTiles[startX+1][startY] == true)
                    {
                        return true;
                    }

                }
                if (CalculateRoomOverlap(roomX, roomY, occupiedTiles))
                {
                    return true;
                }

                break;
        }
        return false;
    }

    //check if the room attached to corridor will also overlap
    //returns true if there is overlap
    public bool CalculateRoomOverlap(int roomX, int roomY, bool[][] occupiedTiles)
    {
        /*subtracting 1 from both positions and i&j<7 will go around the entire room
        as if it's a 6x6 room. Change these values for a larger birth*/
        roomX -= 1;
        roomY -= 1;
        for (int i = 0; i <= 7; i++)
        {
            for (int j = 0; j <= 7; j++)
            {
                if (occupiedTiles[roomX + i][roomY + j])
                {
                    roomX += 1;
                    roomY += 1;
                    return true;
                }
            }
        }
        roomX += 1;
        roomY += 1;
        return false;
    }

    //given the room and the direction (N,E,S,W) -->(0,1,2,3) 
    //check if this position is available for a new corridor to be placed
    //return true if a corridor can be placed in this rooms direction
    public bool CheckForOpenRoomPosition(Room room, int direction)
    {
        switch (direction)
        {
            case 0:
                if (room.corridorNorth)
                    return false;
                break;
            case 1:
                if (room.corridorEast)
                    return false;
                break;
            case 2:
                if (room.corridorSouth)
                    return false;
                break;
            case 3:
                if (room.corridorWest)
                    return false;
                break;
        }

        return true;
    }

    //Checks if the North and East positions of a room are open to place this corridor
    //if it does not go out of bounds or overlaps
    //If the room position is open and the corridor/room doesn't go out of bounds set the corridor in that direction
    //updating startX&Y positions, update "direction" to that direction, update corridorPlaced = true, update that rooms.corridor[Direction] = true,
    //room is the previous room, what this corridor is attatched to
    public void CheckNorthAndEast(Room room, Direction direction, int rows, int columns, int corridorLength, int bendLength, bool[][] occupiedTiles, bool corridorPlaced)
    {
        //corridor does NOT bend
        if (!corridorBend)
        {
            //check if a corridor is available for this direction
            if (CheckForOpenRoomPosition(room, 0))
            {
                //test if this corridor and room will go out of bounds or overlap
                //checking if they're both false then we can place this corridor here
                //udating the rooms available corridor Pos.
                if ((!OutOfBounds((Direction)0, room, rows, columns, room.xPos + 2, room.yPos + 5, corridorLength) && !CalculateOverlap((Direction)0, room.xPos + 2, room.yPos + 5, corridorLength, occupiedTiles)))
                {
                    this.direction = (Direction)0;
                    startXPos = room.xPos + 2;
                    startYPos = room.yPos + 5;
                    room.corridorNorth = true;
                    this.corridorPlaced = true;
                }
            }
            //else check if the east is open
            if (!this.corridorPlaced && CheckForOpenRoomPosition(room, 1))
            {
                if ((!OutOfBounds((Direction)1, room, rows, columns, room.xPos + 5, room.yPos + 2, corridorLength) && !CalculateOverlap((Direction)1, room.xPos + 5, room.yPos + 2, corridorLength, occupiedTiles)))
                {
                    this.direction = (Direction)1;
                    startXPos = room.xPos + 5;
                    startYPos = room.yPos + 2;
                    room.corridorEast = true;
                    this.corridorPlaced = true;
                }
            }
        }
        //corridor DOES bend
        else
        {
            //corridor going North is open
            if (CheckForOpenRoomPosition(room, 0))
            {
                //checking if corridor goes North and bends East
                if ((!OutOfBoundsBend((Direction)1, room, rows, columns, room.xPos + 2, room.yPos + bendPlacement + 4, corridorBendLength) && !OutOfBounds((Direction)0, room, rows, columns, room.xPos + 2, room.yPos + 5, bendPlacement - 2))
                && (!CalculateBendOverlap((Direction)1, room.xPos + 2, room.yPos + bendPlacement + 4, corridorBendLength, occupiedTiles) && !CalculateOverlap((Direction)0, room.xPos + 2, room.yPos + 5, bendPlacement - 2, occupiedTiles)))
                {
                    this.direction = (Direction)0;
                    bendDirection = (Direction)1;
                    startXPos = room.xPos + 2;
                    startYPos = room.yPos + 5;
                    bendXPos = room.xPos + 2;
                    bendYPos = room.yPos + 4 + bendPlacement;
                    endPositionX = room.xPos + 2 + corridorBendLength;
                    endPositionY = room.yPos + 4 + bendPlacement;
                    room.corridorNorth = true;
                    this.corridorPlaced = true;
                }
                //chekcing if corridor goes North and bends West
                else if ((!OutOfBoundsBend((Direction)3, room, rows, columns, room.xPos + 2, room.yPos + bendPlacement + 4, corridorBendLength) && !OutOfBounds((Direction)0, room, rows, columns, room.xPos + 2, room.yPos + 5, bendPlacement - 2))
                && (!CalculateBendOverlap((Direction)3, room.xPos + 2, room.yPos + bendPlacement + 4, corridorBendLength, occupiedTiles) && !CalculateOverlap((Direction)0, room.xPos + 2, room.yPos + 5, bendPlacement - 2, occupiedTiles)))
                {
                    this.direction = (Direction)0;
                    bendDirection = (Direction)3;
                    startXPos = room.xPos + 2;
                    startYPos = room.yPos + 5;
                    bendXPos = room.xPos + 2;
                    bendYPos = room.yPos + 4 + bendPlacement;
                    endPositionX = room.xPos + 2 - corridorBendLength;
                    endPositionY = room.yPos + 4 + bendPlacement;
                    room.corridorNorth = true;
                    this.corridorPlaced = true;
                }
            }
            //else check if room East is open
            if (!this.corridorPlaced && CheckForOpenRoomPosition(room, 1))
            {
                //checking if corridor goes East and bends North
                if ((!OutOfBoundsBend((Direction)0, room, rows, columns, room.xPos + bendPlacement + 4, room.yPos + 2, corridorBendLength) && !OutOfBounds((Direction)1, room, rows, columns, room.xPos + 5, room.yPos + 2, bendPlacement - 2))
                && (!CalculateBendOverlap((Direction)0, room.xPos + bendPlacement + 4, room.yPos + 2, corridorBendLength, occupiedTiles) && !CalculateOverlap((Direction)1, room.xPos + 5, room.yPos + 2, bendPlacement - 2, occupiedTiles)))
                {
                    this.direction = (Direction)1;
                    bendDirection = (Direction)0;
                    startXPos = room.xPos + 5;
                    startYPos = room.yPos + 2;
                    bendXPos = room.xPos + 4 + bendPlacement;
                    bendYPos = room.yPos + 2;
                    endPositionX = room.xPos + 4 + bendPlacement;
                    endPositionY = room.yPos + 2 + corridorBendLength;
                    room.corridorEast = true;
                    this.corridorPlaced = true;
                }
                //checking if corridor goes East and bends South
                else if ((!OutOfBoundsBend((Direction)2, room, rows, columns, room.xPos + bendPlacement + 4, room.yPos + 2, corridorBendLength) && !OutOfBounds((Direction)1, room, rows, columns, room.xPos + 5, room.yPos + 2, bendPlacement - 2))
                && (!CalculateBendOverlap((Direction)2, room.xPos + bendPlacement + 4, room.yPos + 2, corridorBendLength, occupiedTiles) && !CalculateOverlap((Direction)1, room.xPos + 5, room.yPos + 2, bendPlacement - 2, occupiedTiles)))
                {
                    this.direction = (Direction)1;
                    bendDirection = (Direction)2;
                    startXPos = room.xPos + 5;
                    startYPos = room.yPos + 2;
                    bendXPos = room.xPos + 4 + bendPlacement;
                    bendYPos = room.yPos + 2;
                    endPositionX = room.xPos + 4 + bendPlacement;
                    endPositionY = room.yPos + 2 - corridorBendLength;
                    room.corridorEast = true;
                    this.corridorPlaced = true;
                }
            }
        }
    }

    //Checks if the South and West positions of a room are open to place this corridor
    //if it does not go out of bounds or overlaps
    //If the room position is open and the corridor/room doesn't go out of bounds set the corridor in that direction
    //updating startX&Y positions, update "direction" to that direction, update corridorPlaced = true, update that rooms.corridor[Direction] = true,
    //if the corridor bends check for possible different bends positions
    //if the bend corridor can be set, update its bendX&YPos, endX&YPos and bendDirection
    public void CheckSouthAndWest(Room room, Direction direction, int rows, int columns, int corridorLength, int bendLength, bool[][] occupiedTiles, bool corridorPlaced)
    {
        //This checks if we are checking a corridor that is bending or not
        //This if block checks for a corridor that has NO bend
        if (!corridorBend)
        {
            //checking South first
            if (CheckForOpenRoomPosition(room, 2))
            {
                if ((!OutOfBounds((Direction)2, room, rows, columns, room.xPos + 2, room.yPos - 1, corridorLength) && !CalculateOverlap((Direction)2, room.xPos + 2, room.yPos - 1, corridorLength, occupiedTiles)))
                {
                    this.direction = (Direction)2;
                    startXPos = room.xPos + 2;
                    startYPos = room.yPos - 1;
                    room.corridorSouth = true;
                    this.corridorPlaced = true;
                }
            }
            //else check west
            if (!this.corridorPlaced && CheckForOpenRoomPosition(room, 3))
            {
                if ((!OutOfBounds((Direction)3, room, rows, columns, room.xPos - 1, room.yPos + 2, corridorLength) && !CalculateOverlap((Direction)3, room.xPos - 1, room.yPos + 2, corridorLength, occupiedTiles)))
                {
                    this.direction = (Direction)3;
                    startXPos = room.xPos - 1;
                    startYPos = room.yPos + 2;
                    room.corridorWest = true;
                    this.corridorPlaced = true;
                }
            }
        }
        //This else block check for a corridor WITH a bend
        //Here for each direction we need to change the direction of the bend either left or right when corridor goes south,
        //or up or down when goes west
        //updating the bendX&YPos && endX&YPos if the corridor can be placed there
        else
        {
            //corridor going south is open
            if (CheckForOpenRoomPosition(room, 2))
            {
                //checking if corridor goes South and bends East
                if ((!OutOfBoundsBend((Direction)1, room, rows, columns, room.xPos + 2, room.yPos - bendPlacement, corridorBendLength) && !OutOfBounds((Direction)2, room, rows, columns, room.xPos + 2, room.yPos - 1, bendPlacement - 2))
                && (!CalculateBendOverlap((Direction)1, room.xPos + 2, room.yPos - bendPlacement, corridorBendLength, occupiedTiles) && !CalculateOverlap((Direction)2, room.xPos + 2, room.yPos - 1, bendPlacement - 2, occupiedTiles)))
                {
                    this.direction = (Direction)2;
                    bendDirection = (Direction)1;
                    startXPos = room.xPos + 2;
                    startYPos = room.yPos - 1;
                    bendXPos = room.xPos + 2;
                    bendYPos = room.yPos - bendPlacement;
                    endPositionX = room.xPos + 2 + corridorBendLength;
                    endPositionY = room.yPos - bendPlacement;
                    room.corridorSouth = true;
                    this.corridorPlaced = true;

                }
                //checking if corridor goes south and bends West
                else if ((!OutOfBoundsBend((Direction)3, room, rows, columns, room.xPos + 2, room.yPos - bendPlacement, corridorBendLength) && !OutOfBounds((Direction)2, room, rows, columns, room.xPos + 2, room.yPos - 1, bendPlacement - 2))
                && (!CalculateBendOverlap((Direction)3, room.xPos + 2, room.yPos - bendPlacement, corridorBendLength, occupiedTiles) && !CalculateOverlap((Direction)2, room.xPos + 2, room.yPos - 1, bendPlacement - 2, occupiedTiles)))
                {
                    this.direction = (Direction)2;
                    bendDirection = (Direction)3;
                    startXPos = room.xPos + 2;
                    startYPos = room.yPos - 1;
                    bendXPos = room.xPos + 2;
                    bendYPos = room.yPos - bendPlacement;
                    endPositionX = room.xPos + 2 - corridorBendLength;
                    endPositionY = room.yPos - bendPlacement;
                    room.corridorSouth = true;
                    this.corridorPlaced = true;
                }
            }
            //else check if room West is open
            if (!this.corridorPlaced && CheckForOpenRoomPosition(room, 3))
            {
                //checking if corridor goes West and bends North
                if ((!OutOfBoundsBend((Direction)0, room, rows, columns, room.xPos - bendPlacement, room.yPos + 2, corridorBendLength) && !OutOfBounds((Direction)3, room, rows, columns, room.xPos - 1, room.yPos + 2, bendPlacement - 2))
                && (!CalculateBendOverlap((Direction)0, room.xPos - bendPlacement, room.yPos + 2, corridorBendLength, occupiedTiles) && !CalculateOverlap((Direction)3, room.xPos - 1, room.yPos + 2, bendPlacement - 2, occupiedTiles)))
                {
                    this.direction = (Direction)3;
                    bendDirection = (Direction)0;
                    startXPos = room.xPos - 1;
                    startYPos = room.yPos + 2;
                    bendXPos = room.xPos - bendPlacement;
                    bendYPos = room.yPos + 2;
                    endPositionX = room.xPos - bendPlacement;
                    endPositionY = room.yPos + 2 + corridorBendLength;
                    room.corridorWest = true;
                    this.corridorPlaced = true;
                }
                //checking if corridor goes West and bends South
                else if ((!OutOfBoundsBend((Direction)2, room, rows, columns, room.xPos - bendPlacement, room.yPos + 2, corridorBendLength) && !OutOfBounds((Direction)3, room, rows, columns, room.xPos - 1, room.yPos + 2, bendPlacement - 2))
                && (!CalculateBendOverlap((Direction)2, room.xPos - bendPlacement, room.yPos + 2, corridorBendLength, occupiedTiles) && !CalculateOverlap((Direction)3, room.xPos - 1, room.yPos + 2, bendPlacement - 2, occupiedTiles)))
                {
                    this.direction = (Direction)3;
                    bendDirection = (Direction)2;
                    startXPos = room.xPos - 1;
                    startYPos = room.yPos + 2;
                    bendXPos = room.xPos - bendPlacement;
                    bendYPos = room.yPos + 2;
                    endPositionX = room.xPos - bendPlacement;
                    endPositionY = room.yPos + 2 - corridorBendLength;
                    room.corridorWest = true;
                    this.corridorPlaced = true;
                }
            }
        }
    }

    public void CalculateOppositeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                oppositeDirection = (Direction)2;
                break;
            case Direction.East:
                oppositeDirection = (Direction)3;
                break;
            case Direction.South:
                oppositeDirection = (Direction)0;
                break;
            case Direction.West:
                oppositeDirection = (Direction)1;
                break;
        }
    }

    //sets up the event by calling the event at this current tileNumber at that index's Event method
    //to apply its current function to the button(s)
    public void SetUpEvent(int tileNumber, int eventIndex, EventManager eventManager, ScenesManager sceneManger)
    {
        eventManager.CreateEvent(possibleEvents[tileNumber][eventIndex], sceneManger);
    }

}

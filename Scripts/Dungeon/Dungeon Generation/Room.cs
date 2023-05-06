using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Room
{
    public int xPos;
    public int yPos;
    public int endXPos;
    public int endYPos;
    public int roomWidth;
    public int roomHeight;
    public bool corridorNorth = false;
    public bool corridorEast = false;                          //know if a corridor has been placed in this direction
    public bool corridorSouth = false;
    public bool corridorWest = false;
    public bool discovered;                                    //if this room has already been discovered

    public Direction enteringCorridor;                         //the direction of the corridor that is entering this room
    public List<Event.EventType> possibleEvents = new List<Event.EventType>();     //holds a list of possible events for this room essentially a flag for events held in this room

    //used only for the first room as no corridor is going into it
    public void SetUpRoom(int width, int height, int columns, int rows)
    {
        roomWidth = width;
        roomHeight = height;

        //set position of first room in middle of board
        xPos = Mathf.RoundToInt(columns / 2f - roomWidth / 2f);
        yPos = Mathf.RoundToInt(rows / 2f - roomHeight / 2f);

        endXPos = xPos + 4;
        endYPos = yPos + 4;

    }

    //used for every other room that's not the first room
    //finds where the last corridor was placed and fixing itself to that 
    //corridors end X and Y position
    public void SetUpRoom(int width, int height, Corridor corridor)
    {
        enteringCorridor = corridor.direction;
        roomWidth = width;
        roomHeight = height;

        if (corridor.corridorBend == false)
        {
            //direction of the incoming corridor to this room
            switch (corridor.direction)
            {
                case Direction.North:
                    yPos = corridor.endPositionY + 1;
                    xPos = corridor.endPositionX - 2;
                    break;
                case Direction.East:
                    xPos = corridor.endPositionX + 1;
                    yPos = corridor.endPositionY - 2;
                    break;
                case Direction.South:
                    yPos = corridor.endPositionY - roomHeight;//+1
                    xPos = corridor.endPositionX - 2;
                    break;
                case Direction.West:
                    xPos = corridor.endPositionX - roomWidth;
                    yPos = corridor.endPositionY - 2;
                    break;
            }
        }
        else
        {
            //direction of the incoming corridor to this room
            switch (corridor.bendDirection)
            {
                case Direction.North:
                    yPos = corridor.endPositionY + 1;
                    xPos = corridor.endPositionX - 2;
                    break;
                case Direction.East:
                    xPos = corridor.endPositionX + 1;
                    yPos = corridor.endPositionY - 2;
                    break;
                case Direction.South:
                    yPos = corridor.endPositionY - roomHeight;
                    xPos = corridor.endPositionX - 2;
                    break;
                case Direction.West:
                    xPos = corridor.endPositionX - roomWidth;
                    yPos = corridor.endPositionY - 2;
                    break;
            }

        }
        endXPos = xPos + 4;
        endYPos = yPos + 4;
    }

    //counts how many corridors are coming out of this room
    public int CountCorridorsInRoom()
    {
        int corridorCount = 0;

        if (corridorNorth)
            corridorCount++;
        if (corridorEast)
            corridorCount++;
        if (corridorSouth)
            corridorCount++;
        if (corridorWest)
            corridorCount++;

        return corridorCount;
    }

    //sets up the event by calling the event at this current index's Event method
    //to apply its current function to the button(s)
    public void SetUpEvent(int eventIndex, EventManager eventManager, ScenesManager sceneManger)
    {
        eventManager.CreateEvent(possibleEvents[eventIndex], sceneManger);
    }

}

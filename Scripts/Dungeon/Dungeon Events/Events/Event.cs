using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Event 
{
    //Types of all Events
        //order: combat/choiced events, positive events, decision events, negative events
    public enum EventType
    {
        NoEvent, GruntCombat, MiniBoss, Boss, Choice,
        GainWeapon, GainArmor, GainItem, Rest, HighKilled, LowFighting, LowHorror, Thought1, GoldDeposit, BrokenLight,
        EvadeDanger, LootPile, NaturalPit, TownsfolkAdventurer, TerrifiedTownsfolk, DeadTownsfolk, SmallLake, DeadAnimal, LootHorror, MysteriousFungus,
        CaveIn, Carnage, Stress, Sound, Crevasse, TimeElapsed, HighHorror, LowKilled, EscapeCusedPuddle, BlockedPath, Fog, EatFood
    }

    public abstract void SetUpEvent();
}

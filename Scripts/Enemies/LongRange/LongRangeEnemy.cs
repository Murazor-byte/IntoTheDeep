using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//lets just say an enemy with a sword & shield or just fists
public class LongRangeEnemy : Enemy
{
    public LongRangeEnemy(int minHealth, int maxHealth, int minSpeed, int maxSpeed, int combatRating) 
        : base(minHealth, maxHealth, minSpeed, maxSpeed, combatRating){}

    private void Start()
    {

    }
}

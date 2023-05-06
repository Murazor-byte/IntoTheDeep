using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//lets just say a dude with a bow or a wizard that attacks from range
public class ShortRangeEnemy : Enemy
{

    //calls the Enemy constructor to randomly set the health, damage and speed for this enemy
    public ShortRangeEnemy(int minHealth, int maxHealth, int minSpeed, int maxSpeed, int combatRating) 
        : base(minHealth, maxHealth, minSpeed, maxSpeed, combatRating){}

    private void Start()
    {
    }
}

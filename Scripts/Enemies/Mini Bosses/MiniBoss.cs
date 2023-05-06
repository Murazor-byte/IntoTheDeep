using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniBoss : Enemy
{

    public MiniBoss(int minHealth, int maxHealth, int minSpeed, int maxSpeed, int combatRating)
        : base(minHealth, maxHealth, minSpeed, maxSpeed, combatRating) { }

}

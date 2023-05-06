using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MightOfZeus : Weapon, IRangeWeapon
{
    public MightOfZeus()
    {
        weaponDamage = 1000;
        weaponRange = 100;
        weight = 5;
        value = 9999;
    }
}

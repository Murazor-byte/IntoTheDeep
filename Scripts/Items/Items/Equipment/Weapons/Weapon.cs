using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon : Equipment
{
    protected int weaponDamage;
    protected int weaponRange;

    /*public Weapon(int weaponDamage)
    {
        this.weaponDamage = weaponDamage;
    }*/

    public int GetWeaponDamage()
    {
        return weaponDamage;
    }

    public int GetWeaponRange()
    {
        return weaponRange;
    }

}

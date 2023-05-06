using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Armor : Equipment
{
    private int armorValue;
    private int speedModifier; 

    public Armor(int armorStat, int speedModifier)
    {
        this.armorValue = armorStat;
        this.speedModifier = speedModifier;
    }

    public int GetArmorValue()
    {
        return armorValue;
    }
}

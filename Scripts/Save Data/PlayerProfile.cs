using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProfile
{
    public int layerNumber;

    public int gold;
    public int health;
    public int healthCap;
    public int weaponRange;
    public int speed;

    public int fear;
    public int horror;

    public int carryCapacity;
    public int inventoryCarryLoad;

    public string helmArmor;
    public string bodyArmor;
    public string greavesArmor;
    public string bootArmor;

    public string weapon;
    public int damage;
    public int damageModifier;

    public List<string> inventory = new List<string>();     //inventory item type
    public List<int> itemCount = new List<int>();           //inventory item quantity
}

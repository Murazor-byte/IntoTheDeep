using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* low armor, damage, and health, high speed*/
public class Slave : Enemy
{
    Slave() : base(15, 25, 3, 6, 2)
    {
        helmArmorSelection = new List<Armor> { new ClothHelm() };
        bodyArmorSelection = new List<Armor> { new ClothShirt() };
        greavesArmorSelection = new List<Armor> { new ClothGreaves() };
        bootArmorSelection = new List<Armor> { new ClothBoots() };
        allArmor = new List<List<Armor>> { helmArmorSelection, bodyArmorSelection, greavesArmorSelection, bootArmorSelection };

        helmProb = new float[] { 1f };
        bodyProb = new float[] { 1f };
        greavesProb = new float[] { 1f };
        bootsProb = new float[] { 1f };
        allArmorProb = new float[][] { helmProb, bodyProb, greavesProb, bootsProb };

        //type of weapons to select from, right now only a list as I can determine what type of enemy this easy here (short or long range)
        weaponSelection = new List<Weapon> { new Unarmed(), new Sword() };
        weaponProb = new float[] { 0.5f, 0.5f };
    }
}

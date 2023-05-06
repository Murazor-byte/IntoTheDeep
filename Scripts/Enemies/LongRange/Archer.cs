using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Enemy
{
    Archer() : base(20, 25, 3, 6, 3)
    {
        helmArmorSelection = new List<Armor> { new ClothHelm(), new MailHelm() };
        bodyArmorSelection = new List<Armor> { new ClothShirt(), new ChainMail() };
        greavesArmorSelection = new List<Armor> { new ClothGreaves(), new MailGreaves() };
        bootArmorSelection = new List<Armor> { new ClothBoots(), new MailBoots() };
        allArmor = new List<List<Armor>> { helmArmorSelection, bodyArmorSelection, greavesArmorSelection, bootArmorSelection };

        helmProb = new float[] { 0.5f, 0.5f };
        bodyProb = new float[] { 0.5f, 0.5f };
        greavesProb = new float[] { 0.5f, 0.5f };
        bootsProb = new float[] { 0.5f, 0.5f };
        allArmorProb = new float[][] { helmProb, bodyProb, greavesProb, bootsProb };

        //type of weapons to select from, right now only a list as I can determine what type of enemy this easy here (short or long range)
        weaponSelection = new List<Weapon> { new ShortBow() };
        weaponProb = new float[] { 1f };
    }
}

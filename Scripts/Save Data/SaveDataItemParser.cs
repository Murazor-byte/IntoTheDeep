using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class that takes in the saved JSON string representation of the characters
//items and returns an object instance of that item to be equipped between instances
public static class SaveDataItemParser
{

    public static Item ItemParser(string item)
    {
        switch (item)
        {
            case "Arrow": return new Arrow();
            case "Bandage": return new Bandage();
            case "Bolt": return new Bolt();
            case "Camp": return new Camp();
            case "Candle": return new Candle();
            case "Gold": return new Gold();
            case "Lamp": return new Lamp();
            case "Lockpick": return new Lockpick();
            case "Potion_Fire_Resistance": return new Potion_Fire_Resistance();
            case "Potion_Frost": return new Potion_Frost();
            case "Potion_Greater_Healing": return new Potion_Greater_Healing();
            case "Potion_Healing": return new Potion_Healing();
            case "Potion_Poison": return new Potion_Poison();
            case "Potion_Protection": return new Potion_Protection();
            case "Potion_Speed": return new Potion_Speed();
            case "Potion_Strength": return new Potion_Strength();
            case "Potion_Superior_Healing": return new Potion_Superior_Healing();
            case "Potion_Water_Resistance": return new Potion_Water_Resistance();
            case "Ration": return new Ration();
            case "Rope": return new Rope();
            case "Shovel": return new Shovel();
            case "Torch": return new Torch();
            case "Vial_Frost": return new Vial_Frost();
            case "Vial_Oil": return new Vial_Oil();
            case "Vial_Poison": return new Vial_Poison();
            case "WarHorn": return new WarHorn();

            //Helm Armor
            case "ClothHelm": return new ClothHelm();
            case "LeatherHelm": return new LeatherHelm();
            case "MailHelm": return new MailHelm();
            //Body Armor
            case "ChainMail": return new ChainMail();
            case "ClothShirt": return new ClothShirt();
            case "LeatherBreastPlate": return new LeatherBreastPlate();
            //Greaves Armor
            case "ClothGreaves": return new ClothGreaves();
            case "LeatherGreaves": return new LeatherGreaves();
            case "MailGreaves": return new MailGreaves();
            //Boot Armor
            case "ClothBoots": return new ClothBoots();
            case "LeatherBoots": return new LeatherBoots();
            case "MailBoots": return new MailBoots();

            //Melee Weapons
            case "BattleAxe": return new BattleAxe();
            case "Club": return new Club();
            case "Flail": return new Flail();
            case "Glaive": return new Glaive();
            case "Halberd": return new Halberd();
            case "LongSword": return new LongSword();
            case "Mace": return new Mace();
            case "Maul": return new Maul();
            case "MorningStar": return new MorningStar();
            case "Sword": return new Sword();
            case "Unarmed": return new Unarmed();
            case "Warhammer": return new Warhammer();
            //Ranged Weapon
            case "Crossbow": return new Crossbow();
            case "LongBow": return new LongBow();
            case "MightOfZeus": return new MightOfZeus();
            case "ShortBow": return new ShortBow();
            default: Debug.Log("No item can be found through JSON parser of type = " + item);
                return null;
        }
    }

}

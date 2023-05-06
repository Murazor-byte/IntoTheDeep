using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAssets : MonoBehaviour
{
    public static ItemAssets Instance { get; private set; }

    public void CreateInstance()
    {
        Instance = this;
    }

    public Texture emptySlot;
    public Texture potion_Healing;
    public Texture potion_Greater_Healing;
    public Texture potion_Superior_Healing;
    public Texture potion_Poison;
    public Texture potion_Strength;
    public Texture potion_Frost;
    public Texture potion_Protection;
    public Texture potion_Speed;
    public Texture camp;
    public Texture bandage;
    public Texture gold;
    public Texture potion_Fire_Resistance;
    public Texture potion_Water_Resistance;
    public Texture rope;
    public Texture grappling_Hook;
    public Texture rations;
    public Texture torch;
    public Texture lamp;
    public Texture candle;
    public Texture lockpick;
    public Texture arrows;
    public Texture bolts;
    public Texture shovel;
    public Texture vial_Oil;
    public Texture vial_Poison;
    public Texture vial_Frost;
    public Texture war_Horn;

    public Texture chainmail_Helm;
    public Texture chainmail;
    public Texture chainmail_Greaves;
    public Texture chainmail_Boots;

    public Texture leather_Helm;
    public Texture leather_Breastplate;
    public Texture leather_Greaves;
    public Texture leather_Boots;

    public Texture cloth_Helm;
    public Texture cloth_Shirt;
    public Texture cloth_Greaves;
    public Texture cloth_Boots;

    public Texture unarmed;
    public Texture battleaxe;
    public Texture club;
    public Texture flail;
    public Texture glaive;
    public Texture halberd;
    public Texture longsword;
    public Texture mace;
    public Texture maul;
    public Texture morningstar;
    public Texture sword;
    public Texture warhammer;

    public Texture crossbow;
    public Texture longbow;
    public Texture shortbow;

}

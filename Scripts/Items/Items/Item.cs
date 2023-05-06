using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

//parent class where all items derive from
[System.Serializable]
public class Item
{
    public Character character;
    public GameObject inventorySlot;
    public int inventorySlotIndex;       //what slot this item exists in the characters inventory
    public int quantity = 0;
    protected int weight;
    protected int value;
    protected int itemAmount;           //the quantity of items to give
    //The closer to "1" of these two variables will lead to a smaller drop between percentages
    protected int numProb = 1;          //the starting numerator for a first items probability to occur
    protected int denProb = 1;          //the starting denominator for a first items probability to occur

    public Item()
    {
        weight = 0;
        value = 0;
    }

    public virtual Texture GetItemImage() { return ItemAssets.Instance.emptySlot; }

    public int GetWeight()
    {
        return weight;
    }

    public int GetValue()
    {
        return value;
    }

    //returns how many of this item to give to character
    public virtual int ItemAmount() { return 1; }

    /*Given the amount of items to be given from "itemAmount"
     * implements a geometric descent probability formula that creates descending probabilites 
     * from 1 ... itemAmount starting from "numProb" and "denProb" */
    protected int GenerateItemAmount()
    {
        double x;
        double numerator = 0;
        double denominator = Math.Pow(denProb, itemAmount - 1);
        double gcd = Math.Pow(denProb, itemAmount - 1);

        double[] numerators = new double[itemAmount];

        bool oddNumber = false;

        if (itemAmount % 2 == 1) oddNumber = true;

        for (int i = itemAmount - 1; i >= 0; i--)
        {
            if (i == 0)
            {
                numerators[i] = gcd;
            }
            else if (i == itemAmount - 1)
            {
                numerators[i] = Math.Pow(numProb, i);
            }
            else if (oddNumber && i == (itemAmount / 2))
            {
                numerators[i] = Math.Pow(numProb, i) * Math.Pow(denProb, i);
            }
            else if (!oddNumber && i == itemAmount / 2)
            {
                numerators[i] = Math.Pow(numProb, i) * Math.Pow(denProb, i - 1);
            }
            else if (i > itemAmount / 2)
            {
                int difference = (itemAmount - 1) - i;
                numerators[i] = Math.Pow(numProb, i) * Math.Pow(denProb, difference);
            }
            else if (i < itemAmount / 2)
            {
                int difference = itemAmount - 1 - i;
                numerators[i] = Math.Pow(numProb, i) * Math.Pow(denProb, difference);
            }
        }

        for (int i = 0; i < numerators.Length; i++)
        {
            numerator += numerators[i];
        }

        x = denominator / numerator;    //flip numerator and denominator to get percentage

        float[] itemProb = new float[itemAmount];

        float total = 0;
        for (int i = 0; i < itemAmount; i++)
        {
            if (i == 0)
            {
                itemProb[i] = (float)x;
            }
            else
            {
                itemProb[i] = (float)((Math.Pow(numProb, i) / Math.Pow(denProb, i)) * x);
            }
            total += itemProb[i];
            //Debug.Log("Probability at " + i + " = " + itemProb[i]);
        }
        ProbabilityGenerator itemAmountSelector = new ProbabilityGenerator(itemProb);
        return itemAmountSelector.GenerateNumber() + 1;
    }

    public virtual void UseItem()
    {
        character.inventory.RemoveItem(this);
    }

     //if it's an empty item
    public virtual bool GetIsEmpty()
    {
        return false;
    }
}

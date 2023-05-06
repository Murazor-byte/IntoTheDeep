using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 *given a list of floats that will be == 1.00
 *returns an index of that list
*/
public class ProbabilityGenerator
{
    private float[] probForEntities;    //probability for each entity

    public ProbabilityGenerator(float[] probForEntities)
    {
        this.probForEntities = probForEntities;
    }
    
    public int GenerateNumber()
    {
        if (probForEntities.Length == 1) return 0;

        //range being created for each entityProb with a total of 1.0f
        float probSum = 0;
        List<float> entityRange = new List<float>();

        for (int i = 0; i < probForEntities.Length; i++)
        {
            probSum += probForEntities[i];
            entityRange.Add(probSum);
        }

        //generate our random value where we choose from our range
        float value = Random.value;

        //keep track of index these value falls under to associate with which entity it correlates to
        int foundIndex = 0;

        for (int i = 0; i < entityRange.Count; i++)
        {
             if ((i == 0 && value <= entityRange[i]) || i != 0 && value > entityRange[i - 1] && value <= entityRange[i])
            {
                return i;
            }
        }
        return foundIndex;
    }

}

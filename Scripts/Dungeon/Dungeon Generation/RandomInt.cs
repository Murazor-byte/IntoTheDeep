using System;

[Serializable]
//gets a random value between the min and max values
public class RandomInt
{
    public int m_Min;
    public int m_Max;

   public RandomInt(int min, int max)
    {
        this.m_Min = min;
        this.m_Max = max;
    }

   public int Random
   {
       get { return UnityEngine.Random.Range(m_Min, m_Max); }
   }



}

using System;
using System.Collections.Generic;

static class RandomUtils
{
    private static Random random = new Random();
    /*
    public static T RandomFromHashSet<T>(HashSet<T> hashset)
    {
        int i = 0;
        int randomNumber = random.Next(hashset.Count);
        foreach( T element in hashset)
        {
            if (i == randomNumber)
            {
                return element;
            }
            i++;
        }
        return hashset.GetEnumerator().Current;
    }
    */
    public static WFCPrototype RandomFromHashSet(HashSet<WFCPrototype> hashset)
    {
        float route = 0;
        float totalRoute = 0;
        foreach (WFCPrototype element in hashset)
        {
            totalRoute += element.weight;
        }

        float randomNumber = 0.001f * (float)random.Next((int)(totalRoute * 1000));
        foreach (WFCPrototype element in hashset)
        {
            route += element.weight;
            if (route >= randomNumber)
            {
                return element;
            }
        }
        return hashset.GetEnumerator().Current;
    }
}


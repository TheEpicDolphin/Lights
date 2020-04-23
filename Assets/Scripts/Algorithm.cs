using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlgorithmUtils
{
    public enum CompCondition
    {
        GREATER_THAN,
        LESS_THAN
    }

    public class Algorithm
    {
        //If condition == GREATER_THAN, then value at returned index is GREATER than input val
        //If condition == LESS_THAN, then value at returned index is LESS than input val
        internal static int BinarySearch(List<float> ar, CompCondition condition, float val)
        {
            int n = ar.Count;
            int s = 0;
            int e = n - 1;
            while (e - s > 1)
            {
                int mid = (s + e) / 2;
                if (ar[mid] < val)
                {
                    s = mid;
                }
                else
                {
                    e = mid;
                }
            }
            if(condition == CompCondition.GREATER_THAN)
            {
                return e;
            }
            else if(condition == CompCondition.LESS_THAN)
            {
                return s;
            }
            return s;
        }

    }

    
}

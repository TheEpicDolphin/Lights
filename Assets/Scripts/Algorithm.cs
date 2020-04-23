using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlgorithmUtils
{
    public enum CompCondition
    {
        SMALLEST_GEQUAL,
        LARGEST_LEQUAL
    }

    public class Algorithm
    {
        //If condition == SMALLEST_GEQUAL, then value at returned index is the smallest number that
        //              is greater than or equal to input val
        //If condition == LARGEST_LEQUAL, then value at returned index is the largest number that is 
        //              less than or equal to input val       
        internal static int BinarySearch(List<float> ar, CompCondition condition, float val)
        {
            int n = ar.Count;
            int s = 0;
            int e = n - 1;
            while (e - s > 1)
            {
                int mid = (s + e) / 2;
                
                if(Mathf.Abs(ar[mid] - val) < Mathf.Epsilon)
                {
                    if (condition == CompCondition.LARGEST_LEQUAL)
                    {
                        s = mid + 1;
                    }
                    else if(condition == CompCondition.SMALLEST_GEQUAL)
                    {
                        e = mid - 1;
                    }
                }
                else if (ar[mid] < val)
                {
                    s = mid;
                }
                else if(ar[mid] > val)
                {
                    e = mid;
                }
            }
            
            if(condition == CompCondition.LARGEST_LEQUAL)
            {
                if (Mathf.Abs(ar[e] - val) < Mathf.Epsilon)
                {
                    return e;
                }
                else
                {
                    return e - 1;
                }
            }
            else if(condition == CompCondition.SMALLEST_GEQUAL)
            {
                if (Mathf.Abs(ar[s] - val) < Mathf.Epsilon)
                {
                    return s;
                }
                else
                {
                    return s + 1;
                }
            }
            return s;
        }

    }

    
}

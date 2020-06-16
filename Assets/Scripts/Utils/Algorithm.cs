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
                
                if(ar[mid] == val)
                {
                    if (condition == CompCondition.LARGEST_LEQUAL)
                    {
                        int i = mid + 1;
                        while(i <= e && ar[i] == val)
                        {
                            i += 1;
                        }
                        return i - 1;
                    }
                    else if(condition == CompCondition.SMALLEST_GEQUAL)
                    {
                        int i = mid - 1;
                        while (i >= s && ar[i] == val)
                        {
                            i -= 1;
                        }
                        return i + 1;
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
                return s;
            }
            else if(condition == CompCondition.SMALLEST_GEQUAL)
            {
                return e;
            }
            return -1;
        }

        internal static void Shuffle<T>(ref List<T> input)
        {
            for(int i = 0; i < input.Count; i++)
            {
                int j = Random.Range(i, input.Count);
                T tmp = input[i];
                input[i] = input[j];
                input[j] = tmp;
            }
        }

        internal static void Shuffle<T>(ref T[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                int j = Random.Range(i, input.Length);
                T tmp = input[i];
                input[i] = input[j];
                input[j] = tmp;
            }
        }

        /* If key == 0, then it is impossible to select corresponding candidate */
        internal static T WeightedRandomSelection<T>(List<KeyValuePair<float, T>> scoredCandidates)
        {
            //Normalize
            float z = 0.0f;
            List<KeyValuePair<float, T>> cumScores = new List<KeyValuePair<float, T>>();
            for (int i = 0; i < scoredCandidates.Count; i++)
            {
                KeyValuePair<float, T> scoredCandidate = scoredCandidates[i];
                if(scoredCandidate.Key > 0)
                {
                    z += scoredCandidate.Key;
                    cumScores.Add(new KeyValuePair<float, T>(z, scoredCandidate.Value));
                }
            }

            float x = Random.Range(0, z);
            for (int i = 0; i < cumScores.Count - 1; i++)
            {
                if (x < cumScores[i].Key)
                {
                    return cumScores[i].Value;
                }
            }

            return cumScores[cumScores.Count - 1].Value;
        }
    }

    
}

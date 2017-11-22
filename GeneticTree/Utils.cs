using System;
namespace GeneticTree
{
    public class Utils
    {
        public static int[] shiftRight(int[] arr)
        {
            int[] value = new int[arr.Length];

            for (int i = 1; i < arr.Length; i++)
            {
                value[i] = arr[i - 1];
            }

            value[0] = arr[value.Length - 1];

            return value;
        }
    }
}

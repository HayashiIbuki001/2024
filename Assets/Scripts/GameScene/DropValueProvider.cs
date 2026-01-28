using UnityEngine;

public class DropValueProvider
{
    int[] values = { 2, 4, 8, 16 };
    int[] rates = { 60, 25, 10, 5 };

    public int Get()
    {
        int r = Random.Range(0, 100);
        int sum = 0;

        for (int i = 0; i < values.Length; i++)
        {
            sum += rates[i];
            if (r < sum) return values[i];
        }
        return 2;
    }
}


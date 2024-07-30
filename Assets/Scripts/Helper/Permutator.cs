using System.Collections.Generic;
using UnityEngine;

public class Permutator 
{
    private List<Vector3> positions;

    private List<Vector3> targets;

    private float[,] distances;

    private int permutationLength;

    public int maxPermutationLenth = 9;

    private bool[] currentlyUsedInput;

    private int[] currentPermutation;

    private int[] bestPermutation;

    private float bestScore;

    public int[] Compute(List<Vector3> positions, List<Vector3> targets)
    {
        this.positions = positions;

        this.targets = targets;

        int count = positions.Count;

        permutationLength = Mathf.Min(positions.Count, maxPermutationLenth);

        int num = 0;

        int num2 = 0;

        distances = new float[positions.Count, targets.Count];

        for (num = 0; num < positions.Count; num++)
        {
            for (num2 = 0; num2 < targets.Count; num2++)
            {
                distances[num, num2] = (targets[num2] - positions[num]).magnitude;
            }
        }
        currentlyUsedInput = new bool[permutationLength];
        currentPermutation = new int[permutationLength];
        bestPermutation = new int[count];
        bestScore = 2.1474836E+09f;

        DoPermute(0);

        for (int i = 0; i < count - maxPermutationLenth; i++)
        {
            bestPermutation[maxPermutationLenth + i] = maxPermutationLenth + i;
        }
        return bestPermutation;
    }

    private void DoPermute(int currentPermutationIndex)
    {
        if (currentPermutationIndex == permutationLength)
        {
            float num = ComputeCurrentPermutationScore();
            if (num < bestScore)
            {
                for (int i = 0; i < permutationLength; i++)
                {
                    bestPermutation[i] = currentPermutation[i];
                    bestScore = num;
                }
                return;
            }
        }
        for (int j = 0; j < permutationLength; j++)
        {
            if (!currentlyUsedInput[j])
            {
                currentPermutation[currentPermutationIndex] = j;
                currentlyUsedInput[j] = true;
                DoPermute(currentPermutationIndex + 1);
                currentlyUsedInput[j] = false;
            }
        }
    }

    private float ComputeCurrentPermutationScore()
    {
        float num = 0f;

        for (int i = 0; i < permutationLength; i++)
        {
            num += distances[i, currentPermutation[i]];
        }

        return num;
    }
}

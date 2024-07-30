using System.Collections.Generic;
using UnityEngine;

public class SlowMorphRandom
{
    private LinkedList<int> permutatedIndices = new LinkedList<int>();

    public int Min { get; private set; }

    public int Max { get; private set; }

    public int Next
    {
        get
        {
            int value = permutatedIndices.First.Value;
            permutatedIndices.RemoveFirst();
            Insert(value);
            return value;
        }
    }

    public SlowMorphRandom()
    {
        Min = 0;
        Max = 0;
    }

    public SlowMorphRandom(int min, int max)
    {
        ResetRange(min, max);
    }

    public void ResetRange(int min, int max)
    {
        permutatedIndices.Clear();
        Min = min;
        Max = max;
        int range = max - min;
        int index = Random.Range(0, range);
        for (int i = 0; i < range; i++)
        {
            Insert(Min + (i + index) % range);
        }
    }

    private void Insert(int index)
    {
        if (permutatedIndices.Count == 0)
        {
            permutatedIndices.AddFirst(index);
            return;
        }
        int max = 1 + permutatedIndices.Count / 2;
        int num = Random.Range(0, max);
        LinkedListNode<int> linkedListNode = permutatedIndices.Last;

        for (int i = 0; i < num; i++)
        {
            linkedListNode = linkedListNode.Previous;
        }
        permutatedIndices.AddAfter(linkedListNode, index);
    }
}

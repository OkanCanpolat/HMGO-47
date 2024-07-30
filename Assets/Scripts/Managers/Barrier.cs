using System.Collections.Generic;
using UnityEngine;

public class Barrier
{
    private List<object> objects = new List<object>();

    private List<object> delayedObjects = new List<object>();

    private List<float> delayedObjectsTimers = new List<float>();

    public void Add(object @object)
    {
        objects.Add(@object);
    }

    public void Remove(object @object)
    {
        objects.Remove(@object);
    }

    public void RemoveIn(object @object, float delay)
    {
        delayedObjects.Add(@object);
        delayedObjectsTimers.Add(Time.time + delay);
    }

    public bool IsFinished()
    {
        for (int num = delayedObjects.Count - 1; num >= 0; num--)
        {
            object @object = delayedObjects[num];

            if (Time.time > delayedObjectsTimers[num])
            {
                delayedObjects.RemoveAt(num);
                delayedObjectsTimers.RemoveAt(num);
                Remove(@object);
            }
        }
        return objects.Count == 0;
    }

    public void Reset()
    {
        objects.Clear();
    }
}

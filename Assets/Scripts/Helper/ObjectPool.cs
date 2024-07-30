using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Serializable]
    public class ObjectPoolEntry
    {
        [SerializeField]
        public GameObject Prefab;

        [SerializeField]
        public int Count;
    }

    public ObjectPoolEntry[] Entries;

    [HideInInspector]
    public List<GameObject>[] Pool;


    private void Start()
    {
        Pool = new List<GameObject>[Entries.Length];

        for (int i = 0; i < Entries.Length; i++)
        {
            ObjectPoolEntry objectPoolEntry = Entries[i];

            Pool[i] = new List<GameObject>();

            for (int j = 0; j < objectPoolEntry.Count; j++)
            {
                GameObject obj = CreateObject(objectPoolEntry.Prefab);
                PoolObject(obj);
            }
        }
    }

    private GameObject CreateObject(GameObject aObject)
    {
        GameObject gameObject = Instantiate(aObject);
        gameObject.name = aObject.name;
        return gameObject;
    }

    public GameObject GetObjectForType(string objectType, bool onlyPooled)
    {
        for (int i = 0; i < Entries.Length; i++)
        {
            GameObject prefab = Entries[i].Prefab;
            if (!(prefab.name != objectType))
            {
                if (Pool[i].Count > 0)
                {
                    GameObject gameObject = Pool[i][0];
                    Pool[i].RemoveAt(0);
                    gameObject.transform.parent = null;
                    gameObject.SetActive(true);
                    return gameObject;
                }
                if (!onlyPooled)
                {
                    return CreateObject(Entries[i].Prefab);
                }
            }
        }
        return null;
    }

    public void PoolObject(GameObject obj)
    {
        for (int i = 0; i < Entries.Length; i++)
        {
            if (!(Entries[i].Prefab.name != obj.name))
            {
                obj.SetActive(false);
                obj.transform.parent = transform;
                Pool[i].Add(obj);
                break;
            }
        }
    }
}

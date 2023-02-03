using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class PoolItem
{
    public List<GameObject> prefabs = new List<GameObject>();
    public int amount;
    public bool expandable;
    public PoolItemType poolItemType;
    
    [HideInInspector] public GameObject parent;
}

public class Pool : MonoBehaviour
{
    public static Pool Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public List<PoolItem> poolItems;
    [HideInInspector] public List<GameObject> poolObjects;

    private void Start()
    {
        var parentObject = new GameObject
        {
            name = "Pools",
            transform =
            {
                position = Vector3.zero
            }
        };

        var count = poolItems.Count;
        for (var i = 0; i < count; i++)
        {
            var go = new GameObject
            {
                name = poolItems[i].prefabs[0].name + "Pool",
                transform =
                {
                    position = Vector3.zero,
                    parent = parentObject.transform
                }
            };

            poolItems[i].parent = go;
            poolObjects.Add(go);
        }

        foreach (var item in poolItems)
        {
            var amount = item.amount / item.prefabs.Count;

            if (item.amount % item.prefabs.Count != 0)
            {
                var extension = item.amount - amount * item.prefabs.Count;
                for (var i = 0; i < extension; i++)
                {
                    var random = Random.Range(0, item.prefabs.Count);
                    var obj = Instantiate(item.prefabs[random], item.parent.transform, true);
                    obj.name = item.prefabs[random].name;
                    obj.SetActive(false);
                }
            }

            foreach (var itemObject in item.prefabs)
            {
                for (var i = 0; i < amount; i++)
                {
                    var obj = Instantiate(itemObject, item.parent.transform, true);
                    obj.name = itemObject.name;
                    obj.SetActive(false);
                }
            }

            RandomizeSiblings(item.parent.transform);
        }
    }

    private void OnValidate()
    {
        UpdateEnum();
    }

    private GameObject GetFromPool(PoolItemType poolItemType)
    {
        foreach (var item in poolItems)
        {
            if (item.poolItemType == poolItemType)
            {
                foreach (Transform child in item.parent.transform)
                {
                    if (!child.gameObject.activeInHierarchy)
                    {
                        return child.gameObject;
                    }
                }

                if (item.expandable)
                {
                    var randomObjectNo = Random.Range(0, item.prefabs.Count);
                    var newItem = Instantiate(item.prefabs[randomObjectNo], item.parent.transform);
                    newItem.name = item.prefabs[randomObjectNo].name;
                    return newItem;
                }
            }
        }

        return null;
    }

    private GameObject GetFromPool(PoolItemType poolItemType, int childIndex)
    {
        foreach (var item in poolItems)
        {
            if (item.poolItemType == poolItemType)
            {
                foreach (Transform child in item.parent.transform)
                {
                    if (!child.gameObject.activeInHierarchy && item.prefabs[childIndex].name == child.name)
                    {
                        return child.gameObject;
                    }
                }

                if (item.expandable)
                {
                    var newItem = Instantiate(item.prefabs[childIndex], item.parent.transform);
                    newItem.name = item.prefabs[childIndex].name;
                    return newItem;
                }
            }
        }

        return null;
    }

    public GameObject SpawnObject(Vector3 position, PoolItemType poolItemType, Transform parent)
    {
        var b = GetFromPool(poolItemType);
        if (b != null)
        {
            if (parent != null) b.transform.SetParent(parent);
            if (position != null) b.transform.position = position;
            b.SetActive(true);
        }

        return b;
    }

    public GameObject SpawnObject(Vector3 position, PoolItemType poolItemType, Transform parent, int childIndex)
    {
        var b = GetFromPool(poolItemType, childIndex);
        if (b != null)
        {
            if (parent != null) b.transform.SetParent(parent);
            if (position != null) b.transform.position = position;
            b.SetActive(true);
        }

        return b;
    }

    public void DeactivateObject(GameObject member)
    {
        var memberName = member.tag + "Pool";
        foreach (var pool in poolObjects)
        {
            if (memberName == pool.name)
            {
                member.transform.SetParent(pool.transform);
                member.transform.position = pool.transform.position;
                member.transform.rotation = pool.transform.rotation;
                member.SetActive(false);
            }
        }
    }

    private void RandomizeSiblings(Transform pool)
    {
        for (var i = 0; i < pool.childCount; i++)
        {
            var random = Random.Range(i, pool.childCount);
            pool.GetChild(random).SetSiblingIndex(i);
        }
    }

    private void UpdateEnum()
    {
        var enumList = new List<string>();
        foreach (var item in poolItems.Where(item => item.prefabs.Count > 0)
                     .Where(item => !enumList.Contains(item.prefabs[0].tag)))
        {
            enumList.Add(item.prefabs[0].tag);
        }

        const string filePathAndName = "Assets/Scripts/PoolItemType.cs";

        using (var streamWriter = new StreamWriter(filePathAndName))
        {
            streamWriter.WriteLine("public enum PoolItemType");
            streamWriter.WriteLine("{");
            foreach (var name in enumList)
            {
                streamWriter.WriteLine("\t" + name + ",");
            }

            streamWriter.WriteLine("}");
        }

        AssetDatabase.Refresh();
    }
}
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    
    public List<PoolItem> pooledItems;
    [HideInInspector] public List<GameObject> poolObjects;
    private GameObject _parentObject;

    /// <summary>
    /// Calls a member from the selected pool
    /// </summary>
    /// <param name="poolItemType"></param>
    /// <returns></returns>
    private GameObject GetFromPool(PoolItemType poolItemType)
    {
        foreach (var item in pooledItems)
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

    /// <summary>
    /// Calls a specified member from the selected pool
    /// </summary>
    /// <param name="poolItemType"></param>
    /// <param name="childIndex"></param>
    /// <returns></returns>
    private GameObject GetFromPool(PoolItemType poolItemType, int childIndex)
    {
        foreach (var item in pooledItems)
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

    /// <summary>
    /// Spawns an item from the selected pool
    /// </summary>
    /// <param name="position"></param>
    /// <param name="poolItemType"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Spawns an item from the selected pool with specified member
    /// </summary>
    /// <param name="position"></param>
    /// <param name="poolItemType"></param>
    /// <param name="parent"></param>
    /// <param name="childIndex"></param>
    /// <returns></returns>
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
    
    /// <summary>
    /// Spawns an item from the selected pool for a limited time
    /// </summary>
    /// <param name="position"></param>
    /// <param name="poolItemType"></param>
    /// <param name="parent"></param>
    /// <param name="activeTime"></param>
    /// <returns></returns>
    public GameObject SpawnObject(Vector3 position, PoolItemType poolItemType, Transform parent, float activeTime)
    {
        var b = GetFromPool(poolItemType);
        if (b != null)
        {
            if (parent != null) b.transform.SetParent(parent);
            if (position != null) b.transform.position = position;
            b.SetActive(true);
            StartCoroutine(DeactivationTimer());
        }

        return b;

        IEnumerator DeactivationTimer()
        {
            yield return new WaitForSeconds(activeTime);
            DeactivateObject(b, poolItemType);
        }
    }
        
    /// <summary>
    /// Spawns an item from the selected pool with specified member for a limited time
    /// </summary>
    /// <param name="position"></param>
    /// <param name="poolItemType"></param>
    /// <param name="parent"></param>
    /// <param name="childIndex"></param>
    /// <param name="activeTime"></param>
    public GameObject SpawnObject(Vector3 position, PoolItemType poolItemType, Transform parent, int childIndex,
        float activeTime)
    {
        var b = GetFromPool(poolItemType, childIndex);
        if (b != null)
        {
            if (parent != null) b.transform.SetParent(parent);
            if (position != null) b.transform.position = position;
            b.SetActive(true);
            StartCoroutine(DeactivationTimer());
        }

        return b;

        IEnumerator DeactivationTimer()
        {
            yield return new WaitForSeconds(activeTime);
            DeactivateObject(b, poolItemType);
        }
    }

    /// <summary>
    /// Deactivates the given item and returns it to its' pool
    /// </summary>
    /// <param name="member"></param>
    /// <param name="poolItemType"></param>
    /// <returns></returns>
    public void DeactivateObject(GameObject member, PoolItemType poolItemType)
    {
        foreach (var item in pooledItems)
        {
            if (item.poolItemType == poolItemType)
            {
                member.transform.SetParent(item.parent.transform);
                member.transform.position = item.parent.transform.position;
                member.transform.rotation = item.parent.transform.rotation;
                member.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Randomizes the selected pool
    /// </summary>
    /// <param name="pool"></param>
    /// <returns></returns>
    private void RandomizeSiblings(Transform pool)
    {
        for (var i = 0; i < pool.childCount; i++)
        {
            var random = Random.Range(i, pool.childCount);
            pool.GetChild(random).SetSiblingIndex(i);
        }
    }
        
    /// <summary>
    /// Creates a pool object at runtime
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private void CreatePoolItems(PoolItem item)
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

    /// <summary>
    /// Creates a pool at runtime
    /// </summary>
    /// <param name="newItem"></param>
    /// <param name="amount"></param>
    /// <param name="isExpandable"></param>
    /// <returns></returns>
    public void CreateNewPool(GameObject newItem, int amount, bool isExpandable)
    {
        var pooledItem = new PoolItem
        {
            amount = amount,
            expandable = isExpandable,
        };
            
        pooledItem.prefabs.Add(newItem);
            
        var go = new GameObject
        {
            name = pooledItem.prefabs[0].name + "Pool",
            transform =
            {
                position = Vector3.zero,
                parent = _parentObject.transform
            }
        };

        pooledItem.parent = go;
        poolObjects.Add(go);
        pooledItems.Add(pooledItem);
            
        CreatePoolItems(pooledItem);
        UpdateEnum();
    }
        
    /// <summary>
    /// Creates a pool at runtime with multiple pooled object
    /// </summary>
    /// <param name="newItems"></param>
    /// <param name="amount"></param>
    /// <param name="isExpandable"></param>
    /// <returns></returns>
    public void CreateNewPool(List<GameObject> newItems, int amount, bool isExpandable)
    {
        var pooledItem = new PoolItem
        {
            amount = amount,
            expandable = isExpandable,
        };

        foreach (var newItem in newItems)
        {
            pooledItem.prefabs.Add(newItem);
        }
        
        var go = new GameObject
        {
            name = pooledItem.prefabs[0].name + "Pool",
            transform =
            {
                position = Vector3.zero,
                parent = _parentObject.transform
            }
        };
            
        pooledItem.parent = go;
        poolObjects.Add(go);
        pooledItems.Add(pooledItem);

        CreatePoolItems(pooledItem);
        UpdateEnum();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        
        _parentObject = new GameObject
        {
            name = "Pools",
            transform =
            {
                position = Vector3.zero
            }
        };
            
        var count = pooledItems.Count;
        for (var i = 0; i < count; i++)
        {
            var go = new GameObject
            {
                name = pooledItems[i].prefabs[0].name + "Pool",
                transform =
                {
                    position = Vector3.zero,
                    parent = _parentObject.transform
                }
            };

            pooledItems[i].parent = go;
            poolObjects.Add(go);
        }

        foreach (var item in pooledItems)
        {
            CreatePoolItems(item);
        }
    }
        
#if UNITY_EDITOR        
    private void OnValidate()
    {
        UpdateEnum();
    }
#endif  
        
    private void UpdateEnum()
    {
        if (pooledItems.Count <= 0) return;
            
        var enumList = new List<string>();
            
        foreach (var item in pooledItems)
        {
            if (item.prefabs.Count > 0)
            {
                if (!enumList.Contains(item.prefabs[0].name))
                {
                    enumList.Add(item.prefabs[0].name);
                }
            }
        }

        const string filePathAndName = "Assets/Scripts/Blended/PoolItemType.cs";
 
        using ( var streamWriter = new StreamWriter( filePathAndName ) )
        {
            streamWriter.WriteLine( "public enum PoolItemType");
            streamWriter.WriteLine( "{" );
            for (var i = 0; i < enumList.Count; i ++)
            {
                streamWriter.WriteLine( "\t" + enumList[i] + " = " + i + ",");
            }
            streamWriter.WriteLine( "}" );
        }

        for (var i = 0; i < pooledItems.Count; i++)
        {
            pooledItems[i].poolItemType = (PoolItemType)i;
        }
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}

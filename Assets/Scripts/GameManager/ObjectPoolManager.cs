using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class ObjectPoolManager : NetworkBehaviour
{
    public static List<PooledObjectInfo> ObjectPools = new List<PooledObjectInfo>();

    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        Debug.Log("SpawningObject");
        PooledObjectInfo pool = ObjectPools.Find(p => p.m_lookupString == objectToSpawn.name);

        if(pool == null) 
        {
            pool = new PooledObjectInfo() { m_lookupString = objectToSpawn.name };
            ObjectPools.Add(pool);
        }

        GameObject spawnableObj = pool.m_inactiveObjects.FirstOrDefault();

        if(spawnableObj == null) 
        {
            spawnableObj = Instantiate(objectToSpawn, spawnPosition, spawnRotation);
            Debug.Log("creating a new one");
        }
        else
        {
            spawnableObj.transform.position = spawnPosition;
            spawnableObj.transform.rotation = spawnRotation;
            pool.m_inactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }
        return spawnableObj;
    }

    public static void ReturnObjectToPool(GameObject obj)
    {
        string goName = obj.name.Substring(0, obj.name.Length - 7);

        PooledObjectInfo pool = ObjectPools.Find(p => p.m_lookupString == goName);

        if(pool == null)
        {
            Debug.LogWarning("Trying to release an object that is not pooled: " + obj.name);
        }
        else
        {
            obj.SetActive(false);
            pool.m_inactiveObjects.Add(obj);
        }
    }
}

public class PooledObjectInfo
{
    public string m_lookupString;
    public List<GameObject> m_inactiveObjects = new List<GameObject>();
}

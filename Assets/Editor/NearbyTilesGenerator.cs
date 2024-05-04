#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;


public class NearbyTilesGenerator
{
    [MenuItem("Tool//NearbyTilesGenerator")]
    public static void Execute()
    {
        GameObject[] existedTiles = GameObject.FindGameObjectsWithTag("Tile");

        for (int i = 0; i < existedTiles.Length; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Collider[] targets = Physics.OverlapSphere(existedTiles[i].transform.GetChild(j).position, 0.01f, LayerMask.GetMask("Tile"));
                Debug.Log(targets.Length);
                existedTiles[i].GetComponent<Tiles>().SetTilesNearby(j, (targets.Length == 1) ? targets[0].gameObject : null);
            }
        }

        AssetDatabase.SaveAssets();
    }
}
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagedScreen : MonoBehaviour
{
    public Material screenMat;
    private float maxHealth = 50;
    public float health = 50;
    void Start()
    {
        float startHealth = 50;
        screenMat.SetFloat("_amount", 1 - (startHealth / maxHealth));
    }

    private void Update()
    {
        health = PlayerStatsManager.Instance.m_playersHealthList[GameNetworkManager.Instance.GetPlayerID()];
        screenMat.SetFloat("_amount", 1 - (health / maxHealth));
    }

}

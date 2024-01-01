using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;


public class Tower : NetworkBehaviour
{
    [Header("Setup In Unity")]
    public TowerSO m_towerSO;
    [SerializeField]
    private GameObject m_towerRangeIndiactor;
    [SerializeField]
    private Transform m_shootingPosition;
    [SerializeField]
    private GameObject m_towerCollider;

    //NetworkVariables
    private NetworkVariable<int> m_towerID = new NetworkVariable<int>();

    private Enemy m_targetEnemy;

    //replace by enum state for placing
    public bool m_isPlaced = false;

    //Storing the tile the building build
    public GameObject[] m_usedTiles;

    private float m_nextShootTime = 0;

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        if (!m_isPlaced)
            return;

        if (m_targetEnemy != null)
            Shoot(m_targetEnemy);
    }

    private void Init()
    {
        m_towerRangeIndiactor.SetActive(false);
        m_towerRangeIndiactor.transform.localScale = new Vector3(m_towerSO.m_attackRange, 0.1f, m_towerSO.m_attackRange);

        m_usedTiles = new GameObject[m_towerSO.m_tileToBuild.Length + 1];

        m_towerCollider.GetComponent<SphereCollider>().radius = m_towerSO.m_attackRange;

        m_nextShootTime = Time.time;
    }

    private void Shoot(Enemy target)
    {
        if (target == null || Time.time < m_nextShootTime)
        {
            return;
        }

        target.HurtEnemy(m_towerSO.m_attackPower);
        ShootEffectClientRpc(m_targetEnemy.GetComponent<NetworkObject>().NetworkObjectId);
        m_nextShootTime = Time.time + m_towerSO.m_attackSpeed;
    }

    [ClientRpc]
    private void ShootEffectClientRpc(ulong target)
    {
        var attackObject = Instantiate(m_towerSO.m_attackObject, m_shootingPosition.transform.position, quaternion.identity);

        var networkObjects = FindObjectsOfType<NetworkObject>();
        foreach (var netObj in networkObjects)
        {
            if (netObj.NetworkObjectId != target)
                continue;

            attackObject.SetDestination(netObj.transform);
            break;
        }
    }

    public int GetTowerID() => m_towerID.Value;
    public int SetTowerID(int id) => m_towerID.Value = id;
    public void SetTargetEnemy(Enemy targetEnemy) => m_targetEnemy = targetEnemy;
    public void SetUsedTiles(int index, GameObject tile) => m_usedTiles[index] = tile;
    public void ToggleTowerIsPlaced(bool option) => m_isPlaced = option;
    public bool GetIsPlaced() => m_isPlaced;
    public void AssignID(int id) => m_towerID.Value = id;
}

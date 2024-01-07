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

    public bool m_isPlaced = false;

    //Storing the tile the building build
    public GameObject[] m_usedTiles;

    //tower attributes
    private float m_towerAttackPower;
    private float m_towerAttackSpeed;
    private float m_towerAttackRange;

    private int m_upgradeRequiredGold;

    //Shooting Countdown
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

        m_towerAttackPower = m_towerSO.m_attackPower;
        m_towerAttackSpeed = m_towerSO.m_attackSpeed;
        m_towerAttackRange = m_towerSO.m_attackRange;

        m_upgradeRequiredGold = m_towerSO.m_cost;

        m_towerRangeIndiactor.transform.localScale = new Vector3(m_towerAttackRange, 0.1f, m_towerAttackRange);

        m_usedTiles = new GameObject[m_towerSO.m_tileToBuild.Length + 1];

        m_towerCollider.GetComponent<SphereCollider>().radius = m_towerAttackRange;

        m_nextShootTime = Time.time;
    }

    private void Shoot(Enemy target)
    {
        if (target == null || Time.time < m_nextShootTime)
        {
            return;
        }

        target.HurtEnemy(m_towerAttackPower);
        ShootEffectClientRpc(m_targetEnemy.GetComponent<NetworkObject>().NetworkObjectId);
        m_nextShootTime = Time.time + m_towerAttackSpeed;
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

    public void UpgradeCoreAttackSpeed() => m_towerAttackSpeed = m_towerAttackSpeed - (m_towerAttackSpeed / 10);
    public void UpgradeCoreAttackPower() => m_towerAttackPower = m_towerAttackPower * 1.15f;
    public void UpgradeCoreAttackRange() => m_towerAttackRange = m_towerAttackRange * 1.05f;

    public int GetUpgradeRequiredGold() => m_upgradeRequiredGold;
    public void UpgradeGoldIncrease() => m_upgradeRequiredGold = m_upgradeRequiredGold * 2;
}

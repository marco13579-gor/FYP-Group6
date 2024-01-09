using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private EnemySO m_enemySO;
    [SerializeField] private Collider m_Collider;
    private Transform[] m_wayPointList;
    private Transform m_movingTarget;
    private int m_wayPointIndex;
    private int m_enemyID;

    private NetworkVariable<int> m_playerMapID = new NetworkVariable<int>(0);

    private NetworkVariable<FixedString512Bytes> m_name = new NetworkVariable<FixedString512Bytes>("0", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> m_maxHealth = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> m_health = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> m_attackPower = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> m_movementSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        m_Collider.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) { return; }

        m_enemyID = EnemyManager.Instance.Register(this.gameObject);

        ValueInit();
    }

    private void Update()
    {
        if (!IsServer) { return; }

        EnemyMove();
    }

    private void EnemyMove()
    {
        Vector3 movingDir = m_movingTarget.position - transform.position;
        transform.Translate(movingDir.normalized * m_movementSpeed.Value * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, m_movingTarget.position) <= 0.4f)
        {
            GetNextWayPoint();
        }
    }

    private void GetNextWayPoint()
    {
        if (m_wayPointIndex >= m_wayPointList.Length - 1)
        {
            int newHealthAmount = PlayerStatsManager.Instance.GetPlayerHealth(this.GetPlayMap()) - (int)this.m_attackPower.Value;
            GameEventReference.Instance.OnPlayerModifyHealth.Trigger(newHealthAmount, this.GetPlayMap());

            EnemyManager.Instance.Unregister(this.gameObject);
            return;
        }

        m_wayPointIndex++;
        m_movingTarget = m_wayPointList[m_wayPointIndex];
    }

    public void HurtEnemy(float damage)
    {
        this.m_health.Value -= damage;
        if (m_health.Value <= 0)
        {
            EnemyManager.Instance.Unregister(this.gameObject);
        }
    }

    private void ValueInit()
    {
        m_name.Value = new FixedString512Bytes(m_enemySO.m_name);
        m_maxHealth.Value = m_enemySO.m_maxHealth;
        m_health.Value = m_enemySO.m_health;
        m_attackPower.Value = m_enemySO.m_attackPower;
        m_movementSpeed.Value = m_enemySO.m_movementSpeed;

        //Switch player ID to set spawn on which map
        //m_wayPointList = WaypointReference.Instance.m_wayPoints;
        switch (m_playerMapID.Value)
        {
            case 0:
                m_wayPointList = WaypointReference.Instance.m_wayPoints0;
                break;
            case 1:
                m_wayPointList = WaypointReference.Instance.m_wayPoints1;
                break;
        }

        m_movingTarget = m_wayPointList[0];

        //position init
        this.transform.position = m_wayPointList[0].transform.position;
        m_Collider.enabled = true;
    }

    public int GetEnemyID() => m_enemyID;

    public int SetPlayerMap(int ID) => m_playerMapID.Value = ID;

    public int GetPlayMap() => m_playerMapID.Value;
}

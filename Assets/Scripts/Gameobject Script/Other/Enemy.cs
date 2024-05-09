using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private EnemySO m_enemySO;
    [SerializeField] private Collider m_Collider;

    [SerializeField]
    private GameObject m_ignitedEffect;
    [SerializeField]
    private GameObject m_slowedEffect;
    [SerializeField]
    private GameObject m_stunnedEffect;
    [SerializeField]
    private GameObject m_executeEffect;
    [SerializeField]
    private GameObject m_rupturedEffect;
    [SerializeField]
    private GameObject m_slowResistance;
    [SerializeField]
    private GameObject m_stunResistance;
    [SerializeField]
    private GameObject m_fireResistance;
    [SerializeField]
    private GameObject m_executeResistance;
    [SerializeField]
    private GameObject m_cameraPoint;


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
    private NetworkVariable<float> m_slowedMovementSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> m_rewardGold = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> m_isSlowResistance = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> m_isStunResistance = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> m_isFireResistance = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> m_isExecuteResistance = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private bool isDie = false;
    private bool isDieLogicTrigger = false;

    //Enemy Effect Status
    private Dictionary<int, float> m_ignitedList = new Dictionary<int, float>();
    private bool m_isIgnited = false;

    private float m_slowTimer;
    private float m_slowScale = 0;
    private bool m_isSlowed = false;

    private bool m_isStunned = false;
    private float m_stunTimer;

    private bool m_isRuptured = false;
    private float m_isRupturedRate = 0;
    private void Awake()
    {
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

        if(GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Unregister"))
        {
            this.GetComponent<NetworkObject>().Despawn();
        }

        if (PlayerStatsManager.Instance.m_playersHealthList[m_playerMapID.Value] <= 0 && !isDieLogicTrigger)
        {
            Hurt(m_health.Value);
        }

        DieLogic();
        if (isDie) return;
        ReduceMovementSpeedLogic();
        IgnitedLogic();
        StunEnemyLogic();

        //According to the debuff to move
        if ((!m_isStunned) && (!m_isSlowed)) {
            EnemyMove();
        }
        else if ((m_isStunned && m_isStunResistance.Value) || (m_isSlowed && m_isSlowResistance.Value))
        {
            EnemyMove();
        }
        else if(m_isStunned)
        {
            //Stop moving
            return;
        }else if (m_isSlowed)
        {
            EnemyMoveWithReducedSpeed();
        }

    }

    private void DieLogic()
    {
        if(isDie && !isDieLogicTrigger)
        {
            int newGold = PlayerStatsManager.Instance.GetPlayerGold(this.GetPlayMap()) + m_rewardGold.Value;
            ShowFloatingGoldTextClientRpc(m_rewardGold.Value);
            GameEventReference.Instance.OnPlayerModifyGold.Trigger(newGold, m_playerMapID.Value);
            isDieLogicTrigger = true;
        }
    }

    private void IgnitedLogic()
    {
        if (m_isIgnited)
        {
            ToggleIgnitedEffectClientRpc(true);
        }
        else
        {
            ToggleIgnitedEffectClientRpc(false);
        }
    }

    [ClientRpc]
    private void ToggleIgnitedEffectClientRpc(bool option)
    {
        m_ignitedEffect.SetActive(option);
    }

    private void EnemyMove()
    {
        Vector3 movingDir = m_movingTarget.position - transform.position;
        transform.Translate(movingDir.normalized * m_movementSpeed.Value * Time.deltaTime, Space.World);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(movingDir, Vector3.up), Time.deltaTime * m_movementSpeed.Value * 2);

        if (Vector3.Distance(transform.position, m_movingTarget.position) <= 0.4f)
        {
            GetNextWayPoint();
        }
    }

    private void EnemyMoveWithReducedSpeed()
    {
        Vector3 movingDir = m_movingTarget.position - transform.position;
        transform.Translate(movingDir.normalized * m_slowedMovementSpeed.Value * Time.deltaTime, Space.World);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(movingDir, Vector3.up), Time.deltaTime * m_slowedMovementSpeed.Value * 2);

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

            TriggerEnemyDeathClientRpc();
            EnemyManager.Instance.Unregister(this.gameObject);
            GetComponent<NetworkObject>().Despawn();
            return;
        }

        m_wayPointIndex++;
        m_movingTarget = m_wayPointList[m_wayPointIndex];
    }

    public void Hurt(float damage)
    {
        if(m_isRuptured)
        {
            damage *= (1 + m_isRupturedRate);
        }

        this.m_health.Value -= damage;
        int enemyID = this.GetEnemyID();

        if (NetworkManager.Singleton.IsServer)
        { 
            ShowFloatingTextClientRpc(m_enemyID, damage);
        }
        if (isDie) return;
        if (m_health.Value <= 0)
        {
            if (EnemyManager.Instance.m_SpawnedEnemies.ContainsKey(m_enemyID))
            {
                EnemyManager.Instance.Unregister(this.gameObject);
                isDie = true;
                this.m_movementSpeed.Value = 0;
                GetComponent<NetworkAnimator>().SetTrigger("die");
                GetComponent<DissolveController>().TriggerDie();
                TriggerEnemyDeathClientRpc();
                EnemyDieSoundTriggerClientRpc();
            }
        }
    }

    [ClientRpc]
    private void TriggerEnemyDeathClientRpc()
    {
        EnemySelectManager.Instance.TriggerEnemyDeath(this.gameObject);
    }

    public void Executed()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            ExecutedEffectClientRpc();
        }
    }
    [ClientRpc]
    private void ExecutedEffectClientRpc()
    {
        m_executeEffect.SetActive(true);
    }

    [ClientRpc]
    private void EnemyDieSoundTriggerClientRpc()
    {
        //if (this.GetPlayMap() == GameNetworkManager.Instance.GetPlayerID())
        //{
        //    GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.);
        //}
    }

    [ClientRpc]
    private void ToggleSlowedEffectClientRpc(bool option)
    {
        m_slowedEffect.SetActive(option);
    }

    [ClientRpc]
    private void ToggleStunnedEffectClientRpc(bool option)
    {
        m_stunnedEffect.SetActive(option);
    }

    [ClientRpc]
    private void ToggleRupturedEffectClientRpc()
    {
        m_rupturedEffect.SetActive(true);
    }

    private void EnemyDie()
    {

    }

    private void ReduceMovementSpeedLogic()
    {
        if (!m_isSlowed) return;

        if (m_slowTimer >= Time.time)
        {
            print("Slowed");
            print("m_slowedMovementSpeed.Value before" + m_slowedMovementSpeed.Value);
            float reducedMovementSpeed = m_enemySO.m_movementSpeed * m_slowScale;
            m_slowedMovementSpeed.Value = reducedMovementSpeed;
            print("m_slowedMovementSpeed.Value after" + m_slowedMovementSpeed.Value);
            ToggleSlowedEffectClientRpc(true);
        }
        else
        {
            m_slowedMovementSpeed.Value = m_enemySO.m_movementSpeed;
            m_isSlowed = false;
            ToggleSlowedEffectClientRpc(false);
        }
    }

    private void StunEnemyLogic()
    {
        if (m_stunTimer >= Time.time)
        {
            ToggleStunnedEffectClientRpc(true);
        }
        else
        {
            m_isStunned = false;
            ToggleStunnedEffectClientRpc(false);
        }
    }

    [ClientRpc]
    private void ShowFloatingTextClientRpc(int enemyID, float damage)
    {
        if(this.GetPlayMap() == GameNetworkManager.Instance.GetPlayerID())
        {
            GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_hitSound);
        }

        var go = Instantiate(GameObjectReference.Instance.m_floatingText, transform.position, Quaternion.identity, transform);
        int damageToInt = (int)damage;
        go.GetComponent<TextMesh>().text = damageToInt.ToString();
    }

    [ClientRpc]
    private void ShowFloatingGoldTextClientRpc(int gold)
    {
        var go = Instantiate(GameObjectReference.Instance.m_floatingRewardGoldText, transform.position, Quaternion.identity, transform);
        go.GetComponent<TextMesh>().text = $"${gold.ToString()}";
    }

    [ClientRpc]
    private void EnableResistanceSheildClientRpc(bool fire, bool slow, bool stun, bool execute)
    {
        if (m_isFireResistance.Value) m_fireResistance.SetActive(fire);
        if (m_isSlowResistance.Value) m_slowResistance.SetActive(slow);
        if (m_isStunResistance.Value) m_stunResistance.SetActive(stun);
        if (m_isExecuteResistance.Value) m_executeResistance.SetActive(execute);
    }

    private void ValueInit()
    {
        m_name.Value = new FixedString512Bytes(m_enemySO.m_name);
        m_maxHealth.Value = m_enemySO.m_maxHealth;
        m_health.Value = m_enemySO.m_health;
        m_attackPower.Value = m_enemySO.m_attackPower;
        m_movementSpeed.Value = m_enemySO.m_movementSpeed;
        m_slowedMovementSpeed.Value = m_enemySO.m_movementSpeed;
        m_rewardGold.Value = m_enemySO.m_rewardGold;
        m_isSlowResistance.Value = m_enemySO.m_isSlowResistance;
        m_isStunResistance.Value = m_enemySO.m_isStunResistance;
        m_isFireResistance.Value = m_enemySO.m_isFireResistance;
        m_isExecuteResistance.Value = m_enemySO.m_isExecuteResistance;

        EnableResistanceSheildClientRpc(m_isFireResistance.Value, m_isSlowResistance.Value, m_isStunResistance.Value, m_isExecuteResistance.Value);

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
            case 2:
                m_wayPointList = WaypointReference.Instance.m_wayPoints2;
                break;
            case 3:
                m_wayPointList = WaypointReference.Instance.m_wayPoints3;
                break;
        }

        m_movingTarget = m_wayPointList[0];

        //position init
        this.transform.position = m_wayPointList[0].transform.position;
        m_Collider.enabled = true;
    }
    public float GetEnemyHealth() => m_health.Value;
    public float GetEnemyMaxHealth() => m_maxHealth.Value;
    public float GetEnemySpeed() => m_movementSpeed.Value;
    public int GetEnemyID() => m_enemyID;

    public int SetPlayerMap(int ID) => m_playerMapID.Value = ID;

    public int GetPlayMap() => m_playerMapID.Value;

    //Enemy effect status
    public bool GetEnemyIgnited() => m_isIgnited;
    public bool GetEnemyisSlowed() => m_isSlowed;
    public bool GetEnemyisStuned() => m_isStunned;
    public void SetEnemyIgnited(bool option) => m_isIgnited = option;
    public void SetEnemyisSlowed(bool option) => m_isSlowed = option;
    public void SetEnemyisStunned(bool option) => m_isStunned = option;
    public void SetEnemyIsRupture(float rupturedRate)
    {
        if(m_isRupturedRate > rupturedRate)
        {
            return;
        }

        m_isRuptured = true;
        m_isRupturedRate = rupturedRate;
        ToggleRupturedEffectClientRpc();
    }
    public void SetEnemySlowParam(float time, float slowScale)
    {
        m_slowTimer = Time.time + time;
        m_slowScale = slowScale;
    }
    public void SetEnemyStunParam(float time)
    {
        m_stunTimer = Time.time + time;
    }

    public void SetEnemyIgnitedDictionary(int towerID, float damage)
    {
        m_ignitedList.Add(towerID, damage);
    }

    public Dictionary<int, float> GetEnemyIgnitedDictionary() => m_ignitedList;
    public bool GetDieStatus() => isDieLogicTrigger;
    public void RemoveIgnitedDictionaryKey(int ID) => m_ignitedList.Remove(ID);
    public bool GetExecuteResistance() => m_isExecuteResistance.Value;
    public bool GetFireResistance() => m_isFireResistance.Value;
    public EnemySO GetEnemySo() => m_enemySO;
    public Vector3 GetCameraPoint() => m_cameraPoint.transform.position;
}

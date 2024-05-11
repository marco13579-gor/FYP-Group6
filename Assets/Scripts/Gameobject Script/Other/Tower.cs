using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Netcode;

public abstract class Tower : NetworkBehaviour
{
    [Header("Setup In Unity")]
    public TowerSO m_towerSO;
    [SerializeField]
    private GameObject m_towerRangeIndiactor;
    [SerializeField]
    private Transform m_shootingPosition;
    [SerializeField]
    private GameObject m_towerCollider;
    [SerializeField]
    private GameObject m_attackObject;
    [SerializeField]
    private List<GameObject> m_meshRenderer;

    //NetworkVariables
    private NetworkVariable<int> m_towerID = new NetworkVariable<int>();

    private Enemy m_targetEnemy;
    private List<GameObject> m_enemyList;

    public bool m_isPlaced = false;

    //Storing the tile the building build
    public GameObject[] m_usedTiles;

    //tower attributes
    private float m_towerAttackPower;
    private float m_towerAttackSpeed;
    private float m_towerAttackRange;

    private bool m_isAreaAttack;

    private int m_upgradeRequiredGold;

    //Shooting Countdown
    private float m_nextShootTime = 0;

    //Upgrade Effect
    [SerializeField]
    private GameObject m_towerUpgradeEffect;
    private float m_effectEnableTimer = 0;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        GameEventReference.Instance.OnEnterReposeState.AddListener(OnEnterReposeState);
    }

    private void Update()
    {
        if (!m_isPlaced)
            return;

        if (IsServer && m_targetEnemy != null && m_targetEnemy.TryGetComponent<NetworkObject>(out NetworkObject nobj))
            Shoot(m_targetEnemy);

        //Upgrade Effect logic
        if (m_effectEnableTimer >= Time.time)
        {
            if (m_towerUpgradeEffect && m_towerUpgradeEffect.activeSelf == false)
            {
                m_towerUpgradeEffect.SetActive(true);
            }
        }
        else
        {
            if (m_towerUpgradeEffect && m_towerUpgradeEffect.activeSelf == true)
            {
                m_towerUpgradeEffect.SetActive(false);
            }
        }

    }

    private void Init()
    {
        m_towerRangeIndiactor.SetActive(false);

        m_towerAttackPower = m_towerSO.m_attackPower;
        m_towerAttackSpeed = m_towerSO.m_attackSpeed;
        m_towerAttackRange = m_towerSO.m_attackRange;

        m_isAreaAttack = m_towerSO.m_isAreaAttack;

        m_upgradeRequiredGold = m_towerSO.m_cost;

        m_towerRangeIndiactor.transform.localScale = new Vector3(m_towerAttackRange, 1f, m_towerAttackRange);

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

        if (!m_isAreaAttack)
        {
            if (target.gameObject == null) return;
            if (!target.GetDieStatus())
                ShootEffectClientRpc(target);
            m_nextShootTime = Time.time + m_towerAttackSpeed;
        }
        else if (m_isAreaAttack)
        {
            foreach (GameObject eTarget in m_enemyList)
            {
                if (eTarget.gameObject == null) return;
                if (!eTarget.GetComponent<Enemy>().GetDieStatus())
                    ShootEffectClientRpc(eTarget.GetComponent<Enemy>());
            }
            m_nextShootTime = Time.time + m_towerAttackSpeed;
        }
    }

    private void OnEnterReposeState(params object[] param)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            ReposeAction();
        }
    }

    protected virtual void ReposeAction()
    {

    }

    [ClientRpc]
    private void ShootEffectClientRpc(NetworkBehaviourReference target)
    {
        Enemy enemyToShoot;
        target.TryGet(out enemyToShoot);

        GameObject attackObject = Instantiate(m_attackObject, m_shootingPosition.transform.position, quaternion.identity);
        attackObject.GetComponent<Projectile>().SetEnenyToShoot(enemyToShoot);
        attackObject.GetComponent<Projectile>().SetAttackPower(m_towerAttackPower);
        attackObject.GetComponent<Projectile>().SetShootTowerID(GetTowerID());
    }

    public int GetTowerID() => m_towerID.Value;
    public int SetTowerID(int id) => m_towerID.Value = id;
    public void SetTargetEnemy(Enemy targetEnemy) => m_targetEnemy = targetEnemy;
    public void SetTargetEnemyList(List<GameObject> enemyList) => m_enemyList = enemyList;
    public void SetUsedTiles(int index, GameObject tile) => m_usedTiles[index] = tile;
    public void ToggleTowerIsPlaced(bool option) => m_isPlaced = option;
    public bool GetIsPlaced() => m_isPlaced;
    public void AssignID(int id) => m_towerID.Value = id;

    public void UpgradeCoreAttackSpeed() => m_towerAttackSpeed = m_towerAttackSpeed - (m_towerAttackSpeed / 10);
    public void UpgradeCoreAttackPower() => m_towerAttackPower = m_towerAttackPower * 1.15f;
    public void UpgradeCoreAttackRange()
    {
        m_towerAttackRange = m_towerAttackRange * 1.05f;
        m_towerRangeIndiactor.transform.localScale = new Vector3(m_towerAttackRange, 1f, m_towerAttackRange);
        m_towerCollider.GetComponent<SphereCollider>().radius = m_towerAttackRange;
    }

    public void EnableUpgradeEffect()
    {
        m_effectEnableTimer = Time.time + 2f;
    }

    public float GetAttackPower() => m_towerAttackPower;

    public float GetAttackRange() => m_towerAttackRange;
    public float GetAttackSpeed() => m_towerAttackSpeed;

    public int GetUpgradeRequiredGold() => m_upgradeRequiredGold;

    public void UpgradeGoldIncrease() => m_upgradeRequiredGold = (int)(m_upgradeRequiredGold * 1.3f);

    public void TurnOnRangeIndiactor() => m_towerRangeIndiactor.SetActive(true);

    public void TurnOffRangeIndiactor() => m_towerRangeIndiactor.SetActive(false);

    public List<GameObject> GetMeshRenderer() => m_meshRenderer;

    public void RemoveTarget() => m_targetEnemy = null;
    public Enemy GetTargetEnemy() => m_targetEnemy;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemyManager : NetworkedSingleton<EnemyManager>
{
    private const float m_spawnEnemyCooldown = 1.2f;

    private WaveSO m_currentWave;
    private int[] m_waveEnemyRemainingList = new int[50];

    public Dictionary<int, GameObject> m_SpawnedEnemies = new Dictionary<int, GameObject>();

    private float m_NextSpawnTime = 0f;

    private int[] m_mobsCounter = new int[1000];
    private int m_allPlayerMobsCounter = 0;

    private bool isEnteredBattleState = false;

    override protected void Init()
    {
        for (int i = 0; i < 50; i++)
        {
            m_waveEnemyRemainingList[i] = 0;
        }
    }

    private void Start()
    {
        SetUpListeners();
    }

    private void SetUpListeners()
    {
        GameEventReference.Instance.OnEnterBattleState.AddListener(OnEnterBattleState);
        GameEventReference.Instance.OnEnemyDestroyed.AddListener(OnEnemyDestroyed);
        GameEventReference.Instance.OnEnemyHurt.AddListener(OnEnemyHurt);
        GameEventReference.Instance.OnEnemyIgnited.AddListener(OnEnemyIgnited);
        GameEventReference.Instance.OnDealScaredDamageOnIgnitedTarget.AddListener(OnDealScaredDamageOnIgnitedTarget);
        GameEventReference.Instance.OnDealScaredDamageOnIgnitedAmountTarget.AddListener(OnDealScaredDamageOnIgnitedAmountTarget);
        GameEventReference.Instance.OnDealScaredDamageOnSlowedTarget.AddListener(OnDealScaredDamageOnSlowedTarget);
        GameEventReference.Instance.OnDealScaredDamageOnStunnedTarget.AddListener(OnDealScaredDamageOnStunnedTarget);

        GameEventReference.Instance.OnDealScaredDamageOnStunnedAndSlowedTarget.AddListener(OnDealScaredDamageOnStunnedAndSlowedTarget);
        GameEventReference.Instance.OnDealScaredDamageOnSlowedTargetOrSlowTarget.AddListener(OnDealScaredDamageOnSlowedTargetOrSlowTarget);
        GameEventReference.Instance.OnIgniteStunnedTarget.AddListener(OnIgniteStunnedTarget);
        GameEventReference.Instance.OnDealScaredDamageAndSlowTargetOnStunnedTarget.AddListener(OnDealScaredDamageAndSlowTargetOnStunnedTarget);
        GameEventReference.Instance.OnDealScaredDamageOnIgnitedAndStunnedTarget.AddListener(OnDealScaredDamageOnIgnitedAndStunnedTarget);
        GameEventReference.Instance.OnDealScaredDamageAndSlowOnStunnedEnemy.AddListener(OnDealScaredDamageAndSlowOnStunnedEnemy);
        GameEventReference.Instance.OnIgniteStunnedEnemy.AddListener(OnIgniteStunnedEnemy);

        GameEventReference.Instance.OnExecuteIgnitedEnemy.AddListener(OnExecuteIgnitedEnemy);
        GameEventReference.Instance.OnExecuteSlowedEnemy.AddListener(OnExecuteSlowedEnemy);
        GameEventReference.Instance.OnExecuteStunnedEnemy.AddListener(OnExecuteStunnedEnemy);
        GameEventReference.Instance.OnExecuteEnemy.AddListener(OnExecuteEnemy);

        GameEventReference.Instance.OnRuptureEnemy.AddListener(OnRuptureEnemy);

        GameEventReference.Instance.OnEnemySlowed.AddListener(OnEnemySlowed);
        GameEventReference.Instance.OnEnemyStunned.AddListener(OnEnemyStunned);
    }

    private void OnEnemySlowed(params object[] param)
    {
        int enemyID = (int)param[0];
        float scale = (float)param[1];
        float slowDuration = (float)param[2];

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            if (!m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyisSlowed())
            {
                m_SpawnedEnemies[enemyID].GetComponent<Enemy>().SetEnemyisSlowed(true);
                m_SpawnedEnemies[enemyID].GetComponent<Enemy>().SetEnemySlowParam(slowDuration, scale);
            }
        }
    }

    private void OnEnemyIgnited(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];
        int TowerID = (int)param[2];
        int triggerCount = (int)param[3];


        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            if (m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetFireResistance()) return;
        }

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            if (m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyIgnitedDictionary().ContainsKey(TowerID))
            {
            }
        }

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            if (m_SpawnedEnemies[enemyID] != null)
            {
                if(!m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyIgnitedDictionary().ContainsKey(TowerID))
                    m_SpawnedEnemies[enemyID].GetComponent<Enemy>().SetEnemyIgnitedDictionary(TowerID, damage);
                m_SpawnedEnemies[enemyID].GetComponent<Enemy>().SetEnemyIgnited(true);
                StartCoroutine(IHurtEnemy(enemyID, damage, triggerCount, TowerID));
            }
        }
    }

    [ClientRpc]
    private void UpdataIgnitedDictionaryClientRpc(int enemyID, int towerID, float damage)
    {
        m_SpawnedEnemies[enemyID].GetComponent<Enemy>().SetEnemyIgnitedDictionary(towerID, damage);
    }

    private IEnumerator IHurtEnemy(int ID, float damage, int triggerTime, int towerID)
    {
        int attackCount = triggerTime;
        float nextTriggerTime = 0.9f;
        float timer = nextTriggerTime;
        while (true)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = nextTriggerTime;
                --attackCount;
                if (m_SpawnedEnemies.ContainsKey(ID))
                {
                    m_SpawnedEnemies[ID].GetComponent<Enemy>().Hurt(damage);
                }

                if (attackCount == 0)
                {
                    if (m_SpawnedEnemies.ContainsKey(ID))
                    {
                        m_SpawnedEnemies[ID].GetComponent<Enemy>().RemoveIgnitedDictionaryKey(towerID);
                    }
                    break;
                }
            }
            yield return null;
        }
        yield return null;
    }

    private void OnDealScaredDamageOnIgnitedTarget(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];
        float extraDamageScale = (float)param[2];

        bool isIgnited = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            if (m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyIgnitedDictionary().Count >= 1)
            {
                isIgnited = true;
            }
        }

        if (!m_SpawnedEnemies.ContainsKey(enemyID))
            return;

        float extraDamage = damage * extraDamageScale;
        if (isIgnited)
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(extraDamage);
        }
        else
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage);
        }
    }

    private void OnDealScaredDamageOnIgnitedAmountTarget(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];
        float extraDamageScale = (float)param[2];

        int ignitedAmount = 0;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            ignitedAmount = m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyIgnitedDictionary().Count;
        }

        if (!m_SpawnedEnemies.ContainsKey(enemyID))
            return;

        if (ignitedAmount > 0)
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage * extraDamageScale * ignitedAmount);
        }
        else
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage);
        }
    }

    private void OnDealScaredDamageOnSlowedTarget(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];
        float extraDamageScale = (float)param[2];

        bool isSlowed = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            isSlowed = m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyisSlowed();
        }

        if (!m_SpawnedEnemies.ContainsKey(enemyID))
            return;

        if (isSlowed)
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage * extraDamageScale);
        }
        else
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage);
        }
    }

    private void OnEnemyStunned(params object[] param)
    {
        int enemyID = (int)param[0];
        float duration = (float)param[1];

        bool isStunned = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            isStunned = m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyisStuned();
        }

        if (!m_SpawnedEnemies.ContainsKey(enemyID))
            return;

        m_SpawnedEnemies[enemyID].GetComponent<Enemy>().SetEnemyisStunned(true);
        m_SpawnedEnemies[enemyID].GetComponent<Enemy>().SetEnemyStunParam(duration);
    }

    private void OnDealScaredDamageOnStunnedTarget(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];
        float extraDamageScale = (float)param[2];

        bool isStunned = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            isStunned = m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyisStuned();
        }

        if (!m_SpawnedEnemies.ContainsKey(enemyID))
            return;

        if (isStunned)
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage * extraDamageScale);
        }
        else
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage);
        }
    }

    private void OnDealScaredDamageOnStunnedAndSlowedTarget(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];
        float extraDamageScale = (float)param[2];

        bool isSlowed = false;
        bool isStunned = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            isStunned = m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyisStuned();
            isSlowed = m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyisSlowed();
        }

        if (!m_SpawnedEnemies.ContainsKey(enemyID))
            return;

        if (isStunned && isSlowed)
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage * extraDamageScale);
        }
        else
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage);
        }
    }

    private void OnDealScaredDamageOnSlowedTargetOrSlowTarget(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];
        float extraDamageScale = (float)param[2];
        float slowScale = (float)param[3];
        float slowDuration = (float)param[4];

        bool isSlowed = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            isSlowed = m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyisSlowed();
        }

        if (!m_SpawnedEnemies.ContainsKey(enemyID))
            return;

        if (isSlowed)
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage * extraDamageScale);
        }
        else
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage);
            GameEventReference.Instance.OnEnemySlowed.Trigger(enemyID, slowScale, slowDuration);
        }
    }
    private void OnDealScaredDamageAndSlowTargetOnStunnedTarget(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];
        float extraDamageScale = (float)param[2];

        float slowScale = (float)param[3];
        float slowDuration = (float)param[4];

        bool isStunned = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            isStunned = m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyisStuned();
        }

        if (!m_SpawnedEnemies.ContainsKey(enemyID))
            return;

        if (isStunned)
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage * extraDamageScale);
            GameEventReference.Instance.OnEnemySlowed.Trigger(enemyID, slowScale, slowDuration);
        }
        else
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage);
        }
    }

    private void OnIgniteStunnedTarget(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];
        int TowerID = (int)param[2];
        int triggerCount = (int)param[3];

        bool isStunned = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            isStunned = m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyisStuned();
        }

        if (!m_SpawnedEnemies.ContainsKey(enemyID))
            return;

        if (isStunned)
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage);
            GameEventReference.Instance.OnEnemyIgnited.Trigger(enemyID, damage, TowerID, triggerCount);
        }
        else
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage);
        }
    }

    private void OnDealScaredDamageOnIgnitedAndStunnedTarget(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];
        float extraDamageScale = (float)param[2];

        bool isStunned = false;
        bool isIgnited = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            if (m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyIgnitedDictionary().Count > 0)
            {
                isIgnited = true;
            }

            isStunned = m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyisStuned();
        }

        if (!m_SpawnedEnemies.ContainsKey(enemyID))
            return;

        if (isStunned && isIgnited)
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage * extraDamageScale);
        }
        else
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage);
        }
    }

    private void OnDealScaredDamageAndSlowOnStunnedEnemy(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];
        float extraDamageScale = (float)param[2];
        float slowScale = (float)param[3];
        float slowDuration = (float)param[4];

        bool isStunned = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            if (m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyIgnitedDictionary().Count > 0)
            {
                isStunned = true;
            }

            if (!m_SpawnedEnemies.ContainsKey(enemyID))
                return;

            if (isStunned)
            {
                GameEventReference.Instance.OnEnemySlowed.Trigger(enemyID, slowScale, slowDuration);
                m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage * extraDamageScale);
            }
            else
            {
                m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage);
            }
        }
    }

    private void OnIgniteStunnedEnemy(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];

        int TowerID = (int)param[2];
        float burnDamage = (float)param[3];
        int burnCount = (int)param[4];

        bool isStunned = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            if (m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyIgnitedDictionary().Count > 0)
            {
                isStunned = true;
            }

            if (!m_SpawnedEnemies.ContainsKey(enemyID))
                return;

            if (isStunned)
            {
                GameEventReference.Instance.OnEnemyIgnited.Trigger(enemyID, burnDamage, TowerID, burnCount);
            }
        }
    }

    private void OnExecuteIgnitedEnemy(params object[] param)
    {
        int enemyID = (int)param[0];

        int TowerID = (int)param[1];
        float scaleHpToExecute = (float)param[2];

        bool isIgnited = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            if (m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyIgnitedDictionary().Count > 0)
            {
                isIgnited = true;
            }

            if (!m_SpawnedEnemies.ContainsKey(enemyID))
                return;

            if (isIgnited && (m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyHealth() / m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyMaxHealth() <= scaleHpToExecute) && !m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetExecuteResistance())
            {
                m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyHealth());
            }
        }
    }

    private void OnExecuteSlowedEnemy(params object[] param)
    {
        int enemyID = (int)param[0];

        int TowerID = (int)param[1];
        float scaleHpToExecute = (float)param[2];

        bool isSlowed = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            if (m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyisSlowed())
            {
                isSlowed = true;
            }

            if (!m_SpawnedEnemies.ContainsKey(enemyID))
                return;

            if (isSlowed && (m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyHealth() / m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyMaxHealth() <= scaleHpToExecute) && !m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetExecuteResistance())
            {
                m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyHealth());
            }
        }
    }

    private void OnExecuteStunnedEnemy(params object[] param)
    {
        int enemyID = (int)param[0];

        int TowerID = (int)param[1];
        float scaleHpToExecute = (float)param[2];

        bool isStunned = false;

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            if (m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyisStuned())
            {
                isStunned = true;
            }

            if (!m_SpawnedEnemies.ContainsKey(enemyID))
                return;

            if (isStunned && (m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyHealth() / m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyMaxHealth() <= scaleHpToExecute) && !m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetExecuteResistance())
            {
                m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyHealth());
            }
        }
    }

    private void OnExecuteEnemy(params object[] param)
    {
        int enemyID = (int)param[0];

        int TowerID = (int)param[1];
        float scaleHpToExecute = (float)param[2];

        if (!m_SpawnedEnemies.ContainsKey(enemyID))
            return;

        if ((m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyHealth() / m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyMaxHealth() <= scaleHpToExecute) && !m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetExecuteResistance())
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Executed();
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(m_SpawnedEnemies[enemyID].GetComponent<Enemy>().GetEnemyHealth());
        }

    }

    private void OnRuptureEnemy(params object[] param)
    {
        int enemyID = (int)param[0];
        float ruturedRate = (float)param[1];

        if (!m_SpawnedEnemies.ContainsKey(enemyID))
            return;
        m_SpawnedEnemies[enemyID].GetComponent<Enemy>().SetEnemyIsRupture(ruturedRate);
    }

    private void OnEnemyHurt(params object[] param)
    {
        int enemyID = (int)param[0];
        float damage = (float)param[1];

        if (m_SpawnedEnemies.ContainsKey(enemyID))
        {
            m_SpawnedEnemies[enemyID].GetComponent<Enemy>().Hurt(damage);
        }
    }

    private void OnEnemyDie(params object[] param)
    {

    }

    private void OnEnterBattleState(params object[] param)
    {
        WaveSO wave = (WaveSO)param[0];
        isEnteredBattleState = true;
        m_currentWave = wave;

        for (int i = 0; i < m_waveEnemyRemainingList.Length; i++)
        {
            m_waveEnemyRemainingList[i] = 0;
        }

        SetUpEnemyRemainListClientRpc(wave.m_enemiesCount.ToArray());
        m_NextSpawnTime = Time.time;
    }

    [ClientRpc]
    private void SetUpEnemyRemainListClientRpc(int[] enemyCount)
    {
        for (int i = 0; i < enemyCount.Length; i++)
        {
            m_waveEnemyRemainingList[i] = enemyCount[i];

            //Set Different Map Counter
            for (int j = 0; j < GameNetworkManager.Instance.GetPlayerNumber(); j++)
            {
                m_mobsCounter[j] += enemyCount[i];
                m_allPlayerMobsCounter += enemyCount[i];
            }
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        int m_waveEnemyRemaining = 0;

        for (int i = 0; i < m_waveEnemyRemainingList.Length; i++)
        {
            m_waveEnemyRemaining += m_waveEnemyRemainingList[i];
        }

        if (GameStateManager.Instance.GetGameState() == GameState.Battle && m_waveEnemyRemaining != 0 && Time.time >= m_NextSpawnTime)
        {
            SpawnWaveEnemy();
            m_NextSpawnTime = Time.time + m_spawnEnemyCooldown;
        }
    }

    public void SpawnWaveEnemy()
    {
        int enemyTypeIndex = 0;
        do
        {
            enemyTypeIndex = Mathf.Clamp(Random.Range(0, m_currentWave.m_enemiesCount.Count), 0, m_currentWave.m_enemiesCount.Count - 1);
        } while (m_waveEnemyRemainingList[enemyTypeIndex] == 0);

        for (int i = 0; i < GameNetworkManager.Instance.GetPlayerNumber(); i++)
        {
            Enemy enemy = Instantiate(m_currentWave.m_enemiesType[enemyTypeIndex]);
            enemy.SetPlayerMap(i);

            enemy.GetComponent<NetworkObject>().Spawn();
        }

        --m_waveEnemyRemainingList[enemyTypeIndex];
        print(m_waveEnemyRemainingList[enemyTypeIndex]);
    }

    public int Register(GameObject gameObj)
    {
        if (gameObj.TryGetComponent(out Enemy enemy))
        {
            //Detect if the list was full
            if (m_SpawnedEnemies.Count >= 16777217)
            {
                Debug.LogError("Failed To Register Enemy: " + gameObj.name + "\nTiles List Is Full!");
            }

            //arrange valid index
            int randomIndex = Random.Range(0, 16777216);
            while (m_SpawnedEnemies.ContainsKey(randomIndex))
            {
                randomIndex = Random.Range(0, 16777216);
            }

            //Add to list2
            m_SpawnedEnemies.Add(randomIndex, gameObj);

            return randomIndex;
        }
        else
        {
            Debug.LogError("Invalid Tiles: " + gameObj.name);
            return -1;
        }
    }

    public void Unregister(GameObject gameObj)
    {
        if (gameObj.TryGetComponent(out Enemy enemy))
        {
            m_SpawnedEnemies.Remove(enemy.GetEnemyID());
        }

        if (enemy.TryGetComponent<NetworkObject>(out var networkObject) && networkObject.IsSpawned)
        {
            GameEventReference.Instance.OnEnemyDestroyed.Trigger(enemy.gameObject, enemy.GetComponent<Enemy>().GetPlayMap());
        }
    }

    public void OnEnemyDestroyed(params object[] param)
    {
        GameObject enemyToDestroy = (GameObject)param[0];

        --m_allPlayerMobsCounter;

        ReduceMobClientRpc(enemyToDestroy.GetComponent<Enemy>().GetPlayMap());

        if (m_allPlayerMobsCounter <= 0)
        {
            GameEventReference.Instance.OnEnterReposeState.Trigger();
        }
    }

    [ClientRpc]
    private void ReduceMobClientRpc(int enemyMapID)
    {
        --m_mobsCounter[enemyMapID];
    }

    public int GetEnenyRemaining(int playerID) => m_mobsCounter[playerID];
}

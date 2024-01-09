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

        print($"enemy.GetComponent<Enemy>().GetPlayMap(): {enemy.GetComponent<Enemy>().GetPlayMap()}");
        if (enemy.TryGetComponent<NetworkObject>(out var networkObject) && networkObject.IsSpawned)
        {
            GameEventReference.Instance.OnEnemyDestroyed.Trigger(enemy.gameObject, enemy.GetComponent<Enemy>().GetPlayMap());
            networkObject.Despawn();
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

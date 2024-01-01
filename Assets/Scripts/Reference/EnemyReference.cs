using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyReference : Singleton<EnemyReference>
{
    public LayerMask m_enemyMask;
    public GameObject m_testingEnemy;
}

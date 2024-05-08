using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class Projectile : NetworkBehaviour
{
    [SerializeField]
    protected float m_speed = 0.25f;
    protected Enemy m_enemyToShoot;
    protected float m_attackPower;

    protected int m_shootTowerID;
    protected Vector3 m_enemyPosition;

    protected bool isTargetEnemyDie = false;
    protected virtual void Start()
    {
        if (m_enemyToShoot != null)
        {
            m_enemyPosition = m_enemyToShoot.transform.position;
            isTargetEnemyDie = true;
        }
    }

    protected virtual void Update()
    {
        if (Vector3.Distance(m_enemyPosition, transform.position) >= 0.2f)
        {
            transform.LookAt(m_enemyPosition);
            transform.position = Vector3.MoveTowards(this.transform.position, m_enemyPosition, m_speed);
        }
        else
        {
            if (NetworkManager.Singleton.IsServer)
            {
                OnHitTarget();
            }
            OnDestroyObject();
            return;
        }
    }

    protected virtual void OnHitTarget()
    {

    }

    protected virtual void OnDestroyObject()
    {
        
    }

    protected virtual void LateUpdate()
    {
        if (m_enemyToShoot != null)
        {
            m_enemyPosition = m_enemyToShoot.transform.position;
        }
    }

    public void SetEnenyToShoot(Enemy enemyToShoot) => m_enemyToShoot = enemyToShoot;
    public void SetAttackPower(float damage) => m_attackPower = damage;

    public void SetShootTowerID(int ID) => m_shootTowerID = ID;
}

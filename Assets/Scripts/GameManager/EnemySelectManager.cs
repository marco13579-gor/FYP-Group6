using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemySelectManager : Singleton<EnemySelectManager>
{
    private GameObject m_selectedEnemyObj;
    [SerializeField]
    private GameObject m_uiEnemyNameObj;
    [SerializeField]
    private GameObject m_uihealthPowerObj;
    [SerializeField]
    private GameObject m_uiAttackPowerObj;
    [SerializeField]
    private GameObject m_uiMovementSpeedObj;
    [SerializeField]
    private GameObject m_uiRewardGoldObj;
    [SerializeField]
    private GameObject m_uiMobDescriptionObj;
    [SerializeField]
    private GameObject m_targetCamera;
    [SerializeField]
    private GameObject m_enemyStatusUI;

    void Update()
    {
        print("Before SelectEnemyLogic");
        SelectEnemyLogic();
    }

    private void SelectEnemyLogic()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        print(" SelectEnemyLogic 1");

        if (Physics.Raycast(ray, out RaycastHit hit, 100000f, LayerMask.GetMask("Enemy")) && Input.GetKeyDown(KeyCode.Mouse0))
        {
            print("SelectEnemyLogic 2");
            if (!hit.transform.gameObject.GetComponent<Enemy>().GetDieStatus())
            {
                m_selectedEnemyObj = hit.transform.gameObject;
                m_enemyStatusUI.SetActive(true);
            }
        }

        if(m_selectedEnemyObj)
            UpdateEnemyStatusPanel(m_selectedEnemyObj);

        if (Input.GetKeyDown(KeyCode.Mouse1) && m_selectedEnemyObj != null)
        {
            m_selectedEnemyObj = null;
            m_enemyStatusUI.SetActive(false);
        }
        CameraFollow();
    }

    public void TriggerEnemyDeath(GameObject enemyToDestroyObj)
    {
        if (enemyToDestroyObj == m_selectedEnemyObj)
        {
            m_selectedEnemyObj = null;
            m_enemyStatusUI.SetActive(false);
        }
    }
    public void ForceClosePanel()
    {
        m_enemyStatusUI.SetActive(false);
    }
    private void CameraFollow()
    {
        if (m_selectedEnemyObj != null && !m_selectedEnemyObj.GetComponent<Enemy>().GetDieStatus())
        {
            m_targetCamera.transform.position = m_selectedEnemyObj.GetComponent<Enemy>().GetCameraPoint();
            m_targetCamera.transform.LookAt(m_selectedEnemyObj.transform);
        }
    }

    private void UpdateEnemyStatusPanel(GameObject targetEnemyObj)
    {
        EnemySO targetEnemySO = targetEnemyObj.GetComponent<Enemy>().GetEnemySo();
        m_uiEnemyNameObj.GetComponent<TMP_Text>().text = $"{targetEnemySO.m_name}";
        m_uihealthPowerObj.GetComponent<TMP_Text>().text = $"{Mathf.Floor(targetEnemyObj.GetComponent<Enemy>().GetEnemyHealth())} / {targetEnemySO.m_maxHealth}";
        m_uiAttackPowerObj.GetComponent<TMP_Text>().text = $"{targetEnemySO.m_attackPower.ToString()}";
        m_uiMovementSpeedObj.GetComponent<TMP_Text>().text = $"{targetEnemySO.m_movementSpeed.ToString()}";
        m_uiRewardGoldObj.GetComponent<TMP_Text>().text = $"{targetEnemySO.m_rewardGold.ToString()}";
        m_uiMobDescriptionObj.GetComponent<TMP_Text>().text = targetEnemySO.m_description;
    }
}

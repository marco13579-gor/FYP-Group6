using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectReference : Singleton<GameObjectReference>
{
    public GameObject m_mainCamera;
    public GameObject m_spawnPoint0;
    public GameObject m_spawnPoint1;
    public GameObject m_spawnPoint2;
    public GameObject m_spawnPoint3;

    public GameObject m_floatingText;
    public GameObject m_floatingRewardGoldText;

    public GameObject cameraFocusPoint;

    public AudioSource m_audioSource;
}

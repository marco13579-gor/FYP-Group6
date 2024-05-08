using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class LocalPlayerObject : MonoBehaviour
{
    private bool m_spawnTriggered;
    private bool m_serverTriggered;

    void Start()
    {
        
    }

    private void Update()
    {
        if (GetComponent<NetworkObject>().IsOwner && !m_spawnTriggered)
        {
            Debug.Log($"GetComponent<NetworkObject>().OwnerClientId {GetComponent<NetworkObject>().OwnerClientId}");
            GameNetworkManager.Instance.SetplayerID((int)NetworkManager.Singleton.LocalClientId);
            SetPlayerLocation();
            m_spawnTriggered = true;
        }

        if (NetworkManager.Singleton.IsServer && !m_serverTriggered)
        {
            GameNetworkManager.Instance.UpdatePlayerIDSetter();
            GameNetworkManager.Instance.UpdatePlayerNumber();
            print($"GetPlayerNumber() {GameNetworkManager.Instance.GetPlayerNumber()}");
            m_serverTriggered = true;
        }
    }

    private void SetPlayerLocation()
    {
        switch (GetComponent<NetworkObject>().OwnerClientId)
        {
            case 0:
                Camera.main.transform.position = GameObjectReference.Instance.m_spawnPoint0.transform.position;
                break;
            case 1:
                Camera.main.transform.position = GameObjectReference.Instance.m_spawnPoint1.transform.position;
                break;
        }
    }


}

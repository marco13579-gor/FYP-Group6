using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TreeEditor;
using static PlayerNetwork;
using Unity.Collections;
using Unity.VisualScripting;

public class PlayerNetwork : NetworkBehaviour
{
    public GameObject m_Cube;

    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<MyCustomData> myCustomData = new NetworkVariable<MyCustomData>(new MyCustomData
    {
        m_int = 56,
        m_bool = true,
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct MyCustomData : INetworkSerializable
    {
        public int m_int;
        public bool m_bool;
        public FixedString128Bytes m_message;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_int);
            serializer.SerializeValue(ref m_bool);
            serializer.SerializeValue(ref m_message);
        }
    }
    public override void OnNetworkSpawn()
    {
        myCustomData.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log($"{OwnerClientId} Random Number: {newValue.m_int} + {newValue.m_bool} + {newValue.m_message}");
        };
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T))
            SpawnWaveEnemyServerRpc();

        Vector3 moveDir = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    [ServerRpc]
    private void SpawnWaveEnemyServerRpc()
    {
        var cube = Instantiate(m_Cube);
        cube.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    private void TestServerRPC(ServerRpcParams serverRpcParams)
    {
        Debug.Log($"TestServerRPC + {OwnerClientId} + {serverRpcParams.Receive.SenderClientId}");
    }

    [ClientRpc]
    private void TestClientRPC()
    {
        Debug.Log($"TestServerRPC");
    }
}

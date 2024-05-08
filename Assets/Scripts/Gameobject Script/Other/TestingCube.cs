using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TestingCube : NetworkBehaviour
{
    private void Update()
    {
        if(this.transform.position.y <= -10)
        {
            if (IsServer)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    [ServerRpc]
    private void DespawnCubeServerRpc()
    {
        ObjectPoolManager.ReturnObjectToPool(this.gameObject);
    }

}

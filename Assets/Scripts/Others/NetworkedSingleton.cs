using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkedSingleton<T> : NetworkBehaviour where T : Component
{
    public static T Instance { get; private set; }

    protected void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            Init();
        }
        else
        {
            Destroy(this);
        }
    }

    protected virtual void Init()
    {
    }
}

using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameNetworkManager : NetworkedSingleton<GameNetworkManager>
{
    private NetworkVariable<int> m_totalPlayerJoined = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private int m_playerIDSetter = 0;
    private int m_localPlayerID = 0;

    public int UpdatePlayerNumber() => ++m_totalPlayerJoined.Value;
    public int GetPlayerNumber() => m_totalPlayerJoined.Value;
    public int SetplayerID(int ID) => m_localPlayerID = ID;
    public int GetPlayerID() => m_localPlayerID;
    public int UpdatePlayerIDSetter() => ++m_playerIDSetter;
    public int GetPlayerIDSetter() => m_localPlayerID;
}

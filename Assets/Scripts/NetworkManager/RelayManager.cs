using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using TMPro;

public class RelayManager : NetworkBehaviour
{
    private static readonly Regex k_RelayCodeRegex = new Regex(@"^[\w]{6}$");
    private static readonly Regex k_IPv4AddressRegex = new Regex(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");

    [Header("UI")]
    [SerializeField]
    private TMP_Text m_RoomCodeText;
    [SerializeField]
    private Button m_CopyRoomCodeButton;
    [SerializeField]
    private TMP_Text m_PlayerCountText;
    [SerializeField]
    private TMP_Text[] m_PlayerStatusText;
    [SerializeField]
    private Button m_StartGameButton;
    [SerializeField]
    private TMP_Text m_NotEnoughPlayerText;

    [SerializeField]
    private GameObject m_tutPanel;
    [SerializeField]
    private GameObject m_relayPanel;

    private string m_RoomCode;

    private IEnumerator Start()
    {
        yield return UnityServices.InitializeAsync();
        yield return AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void OnEnable()
    {
        m_CopyRoomCodeButton.onClick.AddListener(OnCopyRoomCode);
        m_StartGameButton.onClick.AddListener(OnStartGame);
    }

    private void OnDisable()
    {
        m_CopyRoomCodeButton.onClick.RemoveListener(OnCopyRoomCode);
        m_StartGameButton.onClick.RemoveListener(OnStartGame);
    }

    private void OnCopyRoomCode()
    {
        if (string.IsNullOrWhiteSpace(m_RoomCode))
            return;

        GUIUtility.systemCopyBuffer = m_RoomCode;
    }

    private void OnStartGame()
    {
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null || (!networkManager.IsServer && !networkManager.IsHost))
            return;

        //if (networkManager.ConnectedClientsList.Count < 2)
        //{
        //    m_NotEnoughPlayerText.gameObject.SetActive(true);
        //    return;
        //}

        GameStateManager.Instance.ToggleReadyButtonClick();
        StartGameClientRpc();
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            m_RoomCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);

            m_RoomCodeText.text = m_RoomCode;
            m_StartGameButton.gameObject.SetActive(true);

            UIElementReference.Instance.m_relayPanel.SetActive(false);
            UIElementReference.Instance.m_roomPanel.SetActive(true);

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinRelay()
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(m_RoomCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData);

            m_RoomCodeText.text = m_RoomCode;

            UIElementReference.Instance.m_relayPanel.SetActive(false);
            UIElementReference.Instance.m_roomPanel.SetActive(true);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public void CreateLocal()
    {
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null || !networkManager.TryGetComponent<UnityTransport>(out var transport))
            return;

        var addresses = GetInterfaceAddresses();
        if (addresses.Count <= 0)
        {
            Debug.LogError("No active network address, unable to start server.");
            return;
        }

        var listenAddress = addresses[0];
        m_RoomCode = listenAddress.ToString();
        transport.SetConnectionData("127.0.0.1", 7777, listenAddress);

        m_RoomCodeText.text = listenAddress;
        m_StartGameButton.gameObject.SetActive(true);

        UIElementReference.Instance.m_relayPanel.SetActive(false);
        UIElementReference.Instance.m_roomPanel.SetActive(true);

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void JoinLocal()
    {
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null || !networkManager.TryGetComponent<UnityTransport>(out var transport))
            return;

        transport.SetConnectionData(m_RoomCode, 7777);

        m_RoomCodeText.text = m_RoomCode;

        UIElementReference.Instance.m_relayPanel.SetActive(false);
        UIElementReference.Instance.m_roomPanel.SetActive(true);

        NetworkManager.Singleton.StartClient();
    }

    public void TryJoinRoom()
    {
        m_RoomCode = m_RoomCode.Trim();
        if (string.IsNullOrWhiteSpace(m_RoomCode))
            return;

        if (k_RelayCodeRegex.IsMatch(m_RoomCode))
            JoinRelay();
        else if (k_IPv4AddressRegex.IsMatch(m_RoomCode))
            JoinLocal();
        else
            Debug.LogWarning($"Invalid room code {m_RoomCode}");
    }

    public void ChangeRoomCode(string s)
        => m_RoomCode = s;

    [ClientRpc]
    private void UpdatePlayerStatusClientRpc(int numPlayers)
    {
        m_PlayerCountText.text = $"Player {numPlayers} / 4";
        for (int i = 0; i < m_PlayerStatusText.Length; i++)
        {
            var playerStatus = m_PlayerStatusText[i];
            var state = (i + 1) <= numPlayers;
            playerStatus.text = $"Player{i + 1} Status: {(state ? "Join" : "Not Join")}";
        }
    }

    [ClientRpc]
    private void StartGameClientRpc()
        => UIElementReference.Instance.m_roomPanel.SetActive(false);

    private void OnClientConnected(ulong client)
        => UpdatePlayerStatusClientRpc(NetworkManager.Singleton.ConnectedClients.Count);

    private void OnClientDisconnected(ulong client)
        => UpdatePlayerStatusClientRpc(NetworkManager.Singleton.ConnectedClients.Count);

    private static List<string> GetInterfaceAddresses()
    {
        var addresses = new List<string>();
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            // Ignore inactive interfaces
            if (networkInterface.OperationalStatus != OperationalStatus.Up)
                continue;

            foreach (var entry in networkInterface.GetIPProperties().UnicastAddresses)
            {
                // Ignore loopback address and non-IPv4 addresses
                if (IPAddress.IsLoopback(entry.Address) || entry.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                    continue;

                addresses.Add(entry.Address.ToString());
            }
        }
        return addresses;
    }

    public void OnClickHowToPlayButton()
    {
        m_relayPanel.SetActive(false);
        m_tutPanel.SetActive(true);
    }
}

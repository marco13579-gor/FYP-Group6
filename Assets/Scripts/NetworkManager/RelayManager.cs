using System.Collections;
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
    private string m_roomCode;
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Sign In");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"JoinCode: {joinCode}");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);

            UIElementReference.Instance.m_clickToCopyButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                GUIUtility.systemCopyBuffer = joinCode;
            });

            UIElementReference.Instance.m_startGameButton.SetActive(true);
            UIElementReference.Instance.m_startGameButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (GameNetworkManager.Instance.GetPlayerNumber() >= 2)
                {
                    GameStateManager.Instance.ToggleReadyButtonClick();
                    OnClickStartButtonClientRpc();
                }
                else
                {
                    UIElementReference.Instance.m_morePlayerJoinWarningText.SetActive(true);
                }
            });

            UIElementReference.Instance.m_roomCodeText.GetComponent<TMP_Text>().text = joinCode;
            UIElementReference.Instance.m_relayPanel.SetActive(false);
            UIElementReference.Instance.m_roomPanel.SetActive(true);

            NetworkManager.Singleton.StartHost();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinRelay()
    {
        string joinCode = m_roomCode;

        if (joinCode.Length != 6)
        {
            UIElementReference.Instance.m_warningText.SetActive(true);
            return;
        }

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData);

            UIElementReference.Instance.m_clickToCopyButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                GUIUtility.systemCopyBuffer = joinCode;
            });

            UIElementReference.Instance.m_roomCodeText.GetComponent<TMP_Text>().text = joinCode;
            UIElementReference.Instance.m_relayPanel.SetActive(false);
            UIElementReference.Instance.m_roomPanel.SetActive(true);
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void ChangeRoomCode(string s)
    {
        m_roomCode = s;
        Debug.Log(m_roomCode);
    }

    [ClientRpc]
    private void UpdatePlayerStatusClientRpc(int totalPlayerNumber)
    {
        UIElementReference.Instance.m_playerNumberText.GetComponent<TMP_Text>().text = $"Player {totalPlayerNumber} / 4";
        switch (totalPlayerNumber)
        {
            case 1:
                UIElementReference.Instance.m_player1StatusText.GetComponent<TMP_Text>().text = $"Player1 Status: Join";
                break;
            case 2:
                UIElementReference.Instance.m_player1StatusText.GetComponent<TMP_Text>().text = $"Player1 Status: Join";
                UIElementReference.Instance.m_player2StatusText.GetComponent<TMP_Text>().text = $"Player2 Status: Join";
                break;
            case 3:
                UIElementReference.Instance.m_player1StatusText.GetComponent<TMP_Text>().text = $"Player1 Status: Join";
                UIElementReference.Instance.m_player2StatusText.GetComponent<TMP_Text>().text = $"Player2 Status: Join";
                UIElementReference.Instance.m_player3StatusText.GetComponent<TMP_Text>().text = $"Player3 Status: Join";
                break;
            case 4:
                UIElementReference.Instance.m_player1StatusText.GetComponent<TMP_Text>().text = $"Player1 Status: Join";
                UIElementReference.Instance.m_player2StatusText.GetComponent<TMP_Text>().text = $"Player2 Status: Join";
                UIElementReference.Instance.m_player3StatusText.GetComponent<TMP_Text>().text = $"Player3 Status: Join";
                UIElementReference.Instance.m_player4StatusText.GetComponent<TMP_Text>().text = $"Player3 Status: Join";
                break;
        }
    }

    [ClientRpc]
    private void OnClickStartButtonClientRpc() 
    {
        UIElementReference.Instance.m_roomPanel.SetActive(false);
    }

    private void OnClientConnected(ulong client)
    {
        print("OnClientConnected");
        UpdatePlayerStatusClientRpc(GameNetworkManager.Instance.GetPlayerNumber());
    }
}

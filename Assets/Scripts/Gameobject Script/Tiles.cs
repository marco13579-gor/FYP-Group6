using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Tiles : Node
{
    private int m_tilesID;
    public GameObject[] m_tilesNearby = new GameObject[8];

    private bool isRegisterTrigger = false;

    public void Awake()
    {
        Init();
    }

    private void Start()
    {
        //m_tilesID = TileManager.Instance.Register(this.gameObject);
    }

    private void Update()
    {
        if (!isRegisterTrigger && (IsHost || IsServer) && GameStateManager.Instance.GetReadyStatus())
        {
            m_tilesID = TileManager.Instance.Register(this.gameObject);
            isRegisterTrigger = true;
        }

    }

    private void Init()
    {
        m_isPlaced = false;
    }

    public void SetTilesNearby(int index, GameObject tile)
    {
        m_tilesNearby[index] = tile;
    }

    public int GetTilesID() => m_tilesID;
    public void SetTilesID(int ID) => m_tilesID = ID;
}
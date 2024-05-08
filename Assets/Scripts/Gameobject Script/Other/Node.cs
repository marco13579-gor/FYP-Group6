using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Node : NetworkBehaviour
{
    public bool m_isPlaced;

    [SerializeField]
    private int m_possibleBuilderID;

    public int GetPossibleBuilderID() => m_possibleBuilderID;
}

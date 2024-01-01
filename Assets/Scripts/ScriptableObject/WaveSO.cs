using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "new Wave", menuName = "Battle System/Wave Scriptable Object", order = 2)]
public class WaveSO : ScriptableObject
{
    public List<int> m_enemiesCount;
    public List<Enemy> m_enemiesType;
}

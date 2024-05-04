using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipReference : Singleton<AudioClipReference>
{
    public AudioClip m_hitSound; 
    public AudioClip m_wrongSound; 
    public AudioClip m_buildSound; 
    public AudioClip m_upgradeSound; 
    public AudioClip m_towerTargetSelectChangeSound; 
    public AudioClip m_removeBuildingSound; 
    public AudioClip m_enterBattleStateSound; 
}

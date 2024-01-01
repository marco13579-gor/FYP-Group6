using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvent
{
    public delegate void m_gameEventHandler(params object[] param);
    public event m_gameEventHandler m_onGameEvent;

    public void AddListener(m_gameEventHandler func) => m_onGameEvent += func;
    public void RemoveListener(m_gameEventHandler func) => m_onGameEvent -= func;
    public void Trigger(params object[] param) => m_onGameEvent?.Invoke(param);


}

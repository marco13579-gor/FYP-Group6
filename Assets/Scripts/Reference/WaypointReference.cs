using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointReference : Singleton<WaypointReference>
{
    public Transform m_wayPointObject0;
    public Transform m_wayPointObject1;
    public Transform m_wayPointObject2;
    public Transform m_wayPointObject3;

    [Space]
    public Transform[] m_wayPoints0;
    public Transform[] m_wayPoints1;
    public Transform[] m_wayPoints2;
    public Transform[] m_wayPoints3;

    private void Start()
    {
        m_wayPoints0 = new Transform[m_wayPointObject0.childCount];
        for(int i = 0; i < m_wayPoints0.Length; i++)
        {
            m_wayPoints0[i] = m_wayPointObject0.GetChild(i);
        }

        m_wayPoints1 = new Transform[m_wayPointObject1.childCount];
        for (int i = 0; i < m_wayPoints1.Length; i++)
        {
            m_wayPoints1[i] = m_wayPointObject1.GetChild(i);
        }

        m_wayPoints2 = new Transform[m_wayPointObject2.childCount];
        for (int i = 0; i < m_wayPoints2.Length; i++)
        {
            m_wayPoints2[i] = m_wayPointObject2.GetChild(i);
        }

        m_wayPoints3 = new Transform[m_wayPointObject3.childCount];
        for (int i = 0; i < m_wayPoints3.Length; i++)
        {
            m_wayPoints3[i] = m_wayPointObject3.GetChild(i);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float m_destroyTime = 0.2f;
    public Vector3 m_offset = new Vector3(0,10,0);

    void Start()
    {
        float randomOffsetX = Random.Range(-1f, 1f);
        float randomOffsetY = Random.Range(-1f, 1f);

        m_offset.x = randomOffsetX;
        m_offset.y = randomOffsetY;

        transform.localPosition += m_offset;
        Destroy(gameObject, m_destroyTime);
    }

    private void LateUpdate()
    {
        var cameraToLookAt = Camera.main;
        transform.LookAt(cameraToLookAt.transform);
        transform.rotation = Quaternion.LookRotation(cameraToLookAt.transform.forward);
    }

}

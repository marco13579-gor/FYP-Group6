using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float m_speed;

    private bool m_Active;

    public void SetDestination(Transform destination)
    {
        if (m_Active)
            return;

        StartCoroutine(Animation(destination));
        m_Active = true;
    }

    private IEnumerator Animation(Transform destination)
    {
        float distance = Vector3.Distance(transform.position, destination.position);
        float duration = distance / m_speed;

        Vector3 startPos = transform.position;
        float time = 0f;
        while (time < duration)
        {
            if (destination == null)
            {
                Destroy(gameObject);
                yield break;
            }

            transform.position = Vector3.Lerp(startPos, destination.position, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        //transform.position = destination.position;
        Destroy(gameObject);
    }
}

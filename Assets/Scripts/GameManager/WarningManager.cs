using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningManager : Singleton<WarningManager>
{
    [SerializeField]
    private GameObject m_cardSlotWarningText;

    private TMP_Text m_warningText;
    private Coroutine m_currentCoroutine; // Track the currently running coroutine

    private void Start()
    {
        m_warningText = m_cardSlotWarningText.GetComponent<TMP_Text>();
        m_cardSlotWarningText.SetActive(false);
    }

    public void ModifyCardSlotWarningText(string textToModify)
    {
        // Stop the current coroutine if it's running
        if (m_currentCoroutine != null)
        {
            StopCoroutine(m_currentCoroutine);
        }

        // Start a new coroutine
        m_currentCoroutine = StartCoroutine(ShowAndFadeOutWarningText(textToModify));
    }

    private IEnumerator ShowAndFadeOutWarningText(string text)
    {
        m_warningText.text = text;
        m_cardSlotWarningText.SetActive(true);

        // Gradual appearance
        float duration = 0.75f; // Adjust the duration as needed
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            m_warningText.color = new Color(m_warningText.color.r, m_warningText.color.g, m_warningText.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Wait for a brief period (e.g., 1 second)
        yield return new WaitForSeconds(1f);

        // Gradual fade-out
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            m_warningText.color = new Color(m_warningText.color.r, m_warningText.color.g, m_warningText.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        m_cardSlotWarningText.SetActive(false);
    }
}

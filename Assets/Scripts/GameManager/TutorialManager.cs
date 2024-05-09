using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField]
    private Sprite[] m_tutImageList;
    [SerializeField]
    private string[] m_tutDesriptionText;

    [SerializeField]
    private GameObject m_tutImageSprite;
    [SerializeField]
    private GameObject m_descriptionText;
    [SerializeField]
    private GameObject m_tutPanel;
    [SerializeField]
    private GameObject m_relayPanel;

    [SerializeField]
    private int m_tutIndex = 0;

    private void Start()
    {
        m_tutImageSprite.GetComponent<Image>().sprite = m_tutImageList[0];
        m_descriptionText.GetComponent<TMP_Text>().text = m_tutDesriptionText[0];
    }

    public void OnClickNextPage()
    {
        m_tutIndex++;
        if(m_tutImageList.Length == m_tutIndex)
        {
            m_tutIndex = 0;
            m_tutImageSprite.GetComponent<Image>().sprite = m_tutImageList[0];
            m_descriptionText.GetComponent<TMP_Text>().text = m_tutDesriptionText[0];
        }
        else
        {
            m_tutImageSprite.GetComponent<Image>().sprite = m_tutImageList[m_tutIndex];
            m_descriptionText.GetComponent<TMP_Text>().text = m_tutDesriptionText[m_tutIndex];
        }
    }

    public void OnClickLastPage() 
    {
        m_tutIndex--;
        if (m_tutIndex == -1)
        {
            m_tutIndex = m_tutImageList.Length - 1;
            m_tutImageSprite.GetComponent<Image>().sprite = m_tutImageList[m_tutIndex];
            m_descriptionText.GetComponent<TMP_Text>().text = m_tutDesriptionText[m_tutIndex];
        }
        else
        {
            m_tutImageSprite.GetComponent<Image>().sprite = m_tutImageList[m_tutIndex];
            m_descriptionText.GetComponent<TMP_Text>().text = m_tutDesriptionText[m_tutIndex];
        }
    }

    public void OnClickGoBackToMainPage()
    {
        m_tutPanel.SetActive(false);
        m_relayPanel.SetActive(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gamelosing : MonoBehaviour
{
    public void OnClickClosePanel() 
    { 
        UIElementReference.Instance.m_loseScreen.SetActive(false);
    }
}

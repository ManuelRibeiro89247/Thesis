using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISDA : MonoBehaviour
{
    public GameObject text;
    public GameObject screen;
    public GameObject textICDA;
    public void ShowISDA()
    {
        if (!screen.activeSelf || !text.activeSelf)
        {
            text.SetActive(true);
            screen.SetActive(true);
            textICDA.SetActive(false);
        } else
        {
            text.SetActive(false);
            screen.SetActive(false);
            textICDA.SetActive(true);
        }
    }
}

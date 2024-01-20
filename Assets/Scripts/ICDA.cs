 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICDA : MonoBehaviour
{

    public GameObject text;
    public GameObject screen;
    public GameObject textICDA;

    public void HideICDA()
    {
        if (screen.activeSelf || text.activeSelf)
        {
            text.SetActive(false);
            screen.SetActive(false);
        }
    }

    public void HideICDAShowText()
    {
        if (!textICDA.activeSelf)
        {
            textICDA.SetActive(true);
        }
    }

    public void HideICDAHideText()
    {
        if (!textICDA.activeSelf)
        {
            textICDA.SetActive(false);
        }
    }

}

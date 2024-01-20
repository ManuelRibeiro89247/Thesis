using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpPop : MonoBehaviour
{

    public GameObject helpPop;
    public GameObject menuBar;
    public GameObject dicText;
    public void Pop()
    {
        helpPop.SetActive(true);
        menuBar.SetActive(false);
        dicText.SetActive(false);
    }

    public void ClosePop() 
    { 
        helpPop.SetActive(false);
        menuBar.SetActive(true);
        dicText.SetActive(true);
    }


}

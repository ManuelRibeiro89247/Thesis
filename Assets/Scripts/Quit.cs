using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Quit : MonoBehaviour
{
    public GameObject exitPanel;
    public GameObject dicText;
    public GameObject menuBar;
    public void Pop()
    {
        exitPanel.SetActive(true);
        dicText.SetActive(false);
        menuBar.SetActive(false);
    }
    public void QuitNow()
    {
        Application.Quit();
    }

    public void Continue()
    {
        exitPanel.SetActive(false);
        dicText.SetActive(true);
        menuBar.SetActive(true);
    }
}

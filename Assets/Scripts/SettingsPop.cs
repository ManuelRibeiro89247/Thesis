using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsPop : MonoBehaviour
{
    public GameObject slate;
    public GameObject buttonShow;
    public GameObject recText;

    public void ToggleSettings()
    {
        if (slate.activeSelf)
        {
            slate.SetActive(false);
            recText.SetActive(!recText.activeSelf);
        } 
        else 
        {
            slate.SetActive(!slate.activeSelf);
            recText.SetActive(false);
        }        
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HttpPostConfig : MonoBehaviour
{
    private string url = "https://cloud1cvig.ccg.pt:16301/config/";

    void Start()
    {
        StartCoroutine(SendPostRequest());
    }

    IEnumerator SendPostRequest()
    {
        // Create a UnityWebRequest instance for the POST request
        UnityWebRequest www = UnityWebRequest.Post(url, "");

        // Send the request
        yield return www.SendWebRequest();

        // Check for errors
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("POST request error: " + www.error);
        }
        else
        {
            // Request was successful, and you can access the response here
            string responseText = www.downloadHandler.text;
            Debug.Log("POST request response: " + responseText);
        }
    }
}

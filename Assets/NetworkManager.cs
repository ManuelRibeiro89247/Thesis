using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using System;

public class NetworkManager : MonoBehaviour
{
    private string serverUrl = "http://127.0.0.1:8888/receive";
    /* public float delayInSeconds = 2.0f;*/

    public IEnumerator SendPostRequest(string jsonPayload, Action<string> onResponseReceived)
    {
       /* yield return new WaitForSeconds(delayInSeconds);*/

        if (string.IsNullOrEmpty(jsonPayload))
        {
            Debug.LogWarning("jsonPayload is empty or null. Skipping POST request.");
            yield break; // Exit the method early
        }
        Debug.Log("SENT: " + jsonPayload);

        UnityWebRequest webRequest = new UnityWebRequest(serverUrl, UnityWebRequest.kHttpVerbPOST);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));

        /*webRequest.certificateHandler = new BypassCertificateHandler();*/

        /**/DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();
       /**/ webRequest.downloadHandler = downloadHandler;


        Debug.Log("11111111111" + jsonPayload);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {

            if (webRequest.responseCode == 422)
            {
                /*
                if (!string.IsNullOrEmpty(webRequest.downloadHandler.text))
                {

                    ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(webRequest.downloadHandler.text);
                    if (errorResponse != null && errorResponse.detail.Length > 0)
                    {
                        ErrorDetail firstError = errorResponse.detail[0];
                        Debug.LogError("Error Type: " + firstError.type);
                        Debug.LogError("Error Message: " + firstError.msg);
                    }
                }
                Debug.LogError("POST request error: HTTP " + webRequest.responseCode);
                Debug.LogError("Error Message: " + webRequest.error);
            }
            else
            {*/
                Debug.LogError("POST request error: HTTP " + webRequest.responseCode);
                Debug.LogError("Error Message: " + webRequest.error);
            }
        }
        else
        {
            /*Debug.Log("POST request successful");
            if (!string.IsNullOrEmpty(webRequest.downloadHandler.text))
            {
                Debug.Log("Response: " + webRequest.downloadHandler.text);
                string response = webRequest.downloadHandler.text;
                onResponseReceived?.Invoke(response);
            }*/
            Debug.Log("POST request successful");
            string response = downloadHandler.text;
            Debug.Log("Response: " + response);

            if (!string.IsNullOrEmpty(response))
            {
                onResponseReceived?.Invoke(response);
            }

            else
            {
                Debug.LogWarning("Response is empty or null.");
            }
        }

        yield return new WaitForSeconds(5.0f);
    }
}

public class BypassCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}

[System.Serializable]
public class ErrorResponse
{
    public ErrorDetail[] detail;
}

[System.Serializable]
public class ErrorDetail
{
    public string[] loc;
    public string msg;
    public string type;
}

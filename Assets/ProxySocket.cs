using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine.Networking;
public class ProxySocket : MonoBehaviour
{
    /* public string proxyHost = "127.0.0.1"; // Proxy server IP address
     public int proxyPort = 8888; // Proxy server port (must match the proxy server port)

     private string jsonPayload = "{\"key1\": \"value1\", \"key2\": \"value2\"}"; // Replace with your JSON data

     private void Start()
     {
         SendHttpRequest(jsonPayload);
     }

     public IEnumerator SendHttpRequest(string jsonPayload)
     {
         try
         {
             using (TcpClient client = new TcpClient(proxyHost, proxyPort))
             using (NetworkStream stream = client.GetStream())
             {
                 // Create an HTTP POST request with the JSON payload
                 string request = "POST /path/to/resource HTTP/1.1\r\n" +
                                 "Host: example.com\r\n" +
                                 "Content-Type: application/json\r\n" +
                                 "Content-Length: " + jsonPayload.Length + "\r\n" +
                                 "\r\n" +
                                 jsonPayload;

                 byte[] requestBytes = Encoding.ASCII.GetBytes(request);
                 stream.Write(requestBytes, 0, requestBytes.Length);

                 // Read and handle the response from the proxy server
                 // ...
             }
         }
         catch (Exception e)
         {
             Debug.LogError("Error sending request: " + e.Message);
         }
     }*/
    private string targetHost = "http://localhost:8888";
    private TesteHolistic testeHolistic;
    private void Start()
    {
        // Find the JSONDataGenerator script on a GameObject in the scene
        testeHolistic = FindObjectOfType<TesteHolistic>();
    }

    /*private IEnumerator SendData()
    {
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        };

        string jsonData = testeHolistic.getjson;

        using (UnityWebRequest www = UnityWebRequest.Post(targetHost, jsonData))
        {
            foreach (var header in headers)
            {
                www.SetRequestHeader(header.Key, header.Value);
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Data sent successfully!");
                Debug.Log("Response: " + www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Failed to send data. Error: " + www.error);
            }
        }
    }*/

    public void SendDataButton()
    {
        //StartCoroutine(SendData());
    }

}

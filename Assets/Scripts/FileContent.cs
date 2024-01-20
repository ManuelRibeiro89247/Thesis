using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileContent : MonoBehaviour
{
    private void Update()
    {
        string filePath = "Assets/StreamingAssets/hand_landmark_full.bytes";
        
        if (File.Exists(filePath))
        {
            // Read the content of the file
            byte[] fileContent = File.ReadAllBytes(filePath);

            // Do something with the file content
            // For example, convert the byte array to a string
            string contentAsString = System.Text.Encoding.UTF8.GetString(fileContent);

            // Print the content to the console
            Debug.Log(contentAsString);
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
    }
}

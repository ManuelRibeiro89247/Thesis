using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;
using TMPro;

public class NewBehaviourScript : MonoBehaviour
{
    public TextMeshProUGUI texto;
    public TextMeshProUGUI alerta;
    private string path;
    public void LoadLibrary()
    {
#if UNITY_EDITOR_WIN
        path = Path.Combine("Packages", "com.github.homuler.mediapipe", "Runtime", "Plugins", "mediapipe_c.dll");
        Debug.Log("The UNITY_EDITOR_WIN block was used.");
        alerta.text = "editor";
#elif UNITY_STANDALONE_WIN
    var path = Path.Combine(Application.dataPath, "Plugins", "WSAPlayer", "ARM64", "mediapipe_c.dll");
    Debug.Log("The UNITY_STANDALONE_WIN block was used.");
    alerta.text = "standalone";
#endif
        Debug.Log(1);
        var handle = LoadLibraryW(path);

        if (handle != IntPtr.Zero)
        {
            
            Debug.Log(2);
            texto.text = "sucesso!";
            if (!FreeLibrary(handle))
            {
                Debug.LogError($"Failed to unload {path}: {Marshal.GetLastWin32Error()}");
            }
        }
        else
        {
            
            https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-
            var errorCode = Marshal.GetLastWin32Error();
            Debug.LogError($"Failed to load {path}: {errorCode}");
            texto.text = $"Failed to load {path}: {errorCode}";

            if (errorCode == 126)
            {
                texto.text = "Check missing dependencies using";
                Debug.LogError("Check missing dependencies using [Dependencies](https://github.com/lucasg/Dependencies). If you're sure that required libraries exist, open the plugin inspector for those libraries and check `Load on startup`.");
            }
        }
    }

    [DllImport("kernel32", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibraryW(string path);

    [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeLibrary(IntPtr handle);
}

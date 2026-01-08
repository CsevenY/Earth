using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class shutdownpc : MonoBehaviour
{
    void Start()
    {
        
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
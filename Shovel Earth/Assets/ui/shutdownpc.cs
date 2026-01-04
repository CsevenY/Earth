using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class shutdownpc : MonoBehaviour
{
    void Start()
    {
        
    }

    public void ShutdownPC()
    {
        // Windows系统
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        Process.Start("shutdown", "/s /t 0");

        // 或者使用其他参数：
        // /s 关闭计算机
        // /r 重启计算机
        // /t xxx 延迟xxx秒（0表示立即）
        // /f 强制关闭应用程序

#else
        Debug.LogWarning("关机功能仅支持Windows平台");
#endif
    }
}
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class PauseMenuManager : MonoBehaviour
{
    // 对应你的pauseMenu面板
    public GameObject pauseMenu;
    // 对应你的btn_continue按钮
    public Button btn_continue;
    // 对应你的btn_exit按钮
    public Button btn_exit;

    private bool isPaused = false;

    void Start()
    {
        // 先检查面板是否赋值
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        else
        {
            UnityEngine.Debug.LogError("请给pauseMenu面板赋值！");
        }

        // 绑定继续按钮事件
        if (btn_continue != null)
        {
            btn_continue.onClick.AddListener(ContinueGame);
        }
        else
        {
            UnityEngine.Debug.LogError("请给btn_continue按钮赋值！");
        }

        // 绑定退出按钮事件
        if (btn_exit != null)
        {
            btn_exit.onClick.AddListener(QuitGame);
        }
        else
        {
            UnityEngine.Debug.LogError("请给btn_exit按钮赋值！");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ContinueGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void PauseGame()
    {
        if (pauseMenu == null) return; // 避免空引用
        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ContinueGame()
    {
        if (pauseMenu == null) return; // 避免空引用
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
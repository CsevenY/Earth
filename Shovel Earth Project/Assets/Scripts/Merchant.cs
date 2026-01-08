using UnityEngine;

public class Merchant : MonoBehaviour
{
    [Header("商店UI配置")]
    public GameObject shopUIRoot; // 商店UI根对象

    private void Start()
    {
        // 初始隐藏商店UI
        if (shopUIRoot != null)
        {
            shopUIRoot.SetActive(false);
        }
        else
        {
            Debug.LogError("商人未绑定商店UI！请赋值shopUIRoot");
        }
    }

    // 切换商店UI（public，供PlayerController调用）
    public void ToggleShopUI()
    {
        if (shopUIRoot != null)
        {
            shopUIRoot.SetActive(!shopUIRoot.activeSelf);
            // 暂停/恢复游戏时间缩放
            Time.timeScale = shopUIRoot.activeSelf ? 0f : 1f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // 同步固定帧率
            // 显示/隐藏鼠标
            Cursor.lockState = shopUIRoot.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = shopUIRoot.activeSelf;
        }
    }

    // 关闭商店（核心修复：强制恢复所有状态）
    public void CloseShop()
    {
        if (shopUIRoot != null && shopUIRoot.activeSelf)
        {
            shopUIRoot.SetActive(false);

            // 1. 强制恢复游戏时间缩放（解决人物不动的关键）
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            // 2. 恢复光标锁定状态（解决光标消失/异常）
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 3. 强制重置玩家的商店状态标记
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.isInShop = false;
                player.OnShopClosed(); // 调用玩家自身的恢复逻辑
            }

            Debug.Log("[商店系统] 商店已关闭，所有游戏状态恢复正常");
        }
    }
}
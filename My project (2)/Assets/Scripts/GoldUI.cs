using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间（Text在这个命名空间下）

public class GoldUI : MonoBehaviour
{
    [Header("UI引用")]
    public Text goldText; // 替换为原生Text组件
    public Sprite goldIcon; // 金币图标（可选）

    private void Start()
    {
        // 查找GoldManager并监听金币变更事件
        if (GoldManager.Instance != null)
        {
            // 初始显示
            UpdateGoldDisplay(GoldManager.Instance.GetCurrentGold());
            // 监听金币变化
            GoldManager.Instance.OnGoldChanged += UpdateGoldDisplay;
        }
        else
        {
            goldText.text = "金币：0（未找到GoldManager）";
            Debug.LogError("未找到GoldManager！请在场景中添加GoldManager组件");
        }
    }

    // 更新金币显示
    private void UpdateGoldDisplay(int currentGold)
    {
        if (goldText != null)
        {
            // 兼容低版本C#，替换字符串插值
            goldText.text = string.Format("金币：{0}", currentGold);
        }
    }

    // 移除事件监听（避免内存泄漏）
    private void OnDestroy()
    {
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
        }
    }
}
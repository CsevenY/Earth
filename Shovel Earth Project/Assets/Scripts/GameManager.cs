using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    [Header("金币设置")]
    public int currentGold = 100; // 初始金币
    public int maxGold = 9999; // 最大金币数

    // 新增：金币变更事件（供GoldUI监听）
    public delegate void GoldChanged(int newGold);
    public event GoldChanged OnGoldChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 新增：获取当前金币的方法
    public int GetCurrentGold()
    {
        return currentGold;
    }

    // 增加金币（修改：触发金币变更事件）
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        
        currentGold += amount;
        if (currentGold > maxGold)
        {
            currentGold = maxGold;
        }
        
        // 触发事件，通知UI更新
        OnGoldChanged?.Invoke(currentGold);
        Debug.Log($"获得{amount}金币，当前金币：{currentGold}");
    }

    // 花费金币（修改：触发金币变更事件）
    public bool SpendGold(int amount)
    {
        if (amount <= 0 || currentGold < amount)
        {
            Debug.LogWarning($"金币不足！需要{amount}，当前{currentGold}");
            return false;
        }
        
        currentGold -= amount;
        // 触发事件，通知UI更新
        OnGoldChanged?.Invoke(currentGold);
        Debug.Log($"花费{amount}金币，当前金币：{currentGold}");
        return true;
    }

    // 检查金币是否足够
    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    // 重置金币（调试用）
    [ContextMenu("重置金币为100")]
    public void ResetGold()
    {
        currentGold = 100;
        OnGoldChanged?.Invoke(currentGold); // 触发事件
        Debug.Log("金币已重置为100");
    }
}
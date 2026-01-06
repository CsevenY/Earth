using UnityEngine;
using UnityEngine.UI;

public class GoldSystem : MonoBehaviour
{
    // 单例（全局访问）
    public static GoldSystem Instance;
    
    [Header("金币配置")]
    public int initialGold = 100; // 初始金币
    public Text goldText; // 绑定UI的金币显示文本（如GoldText）

    private int _currentGold;
    public int CurrentGold
    {
        get => _currentGold;
        set
        {
            _currentGold = Mathf.Max(0, value); // 金币不能为负
            UpdateGoldUI();
        }
    }

    void Awake()
    {
        // 单例初始化
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CurrentGold = initialGold;
        UpdateGoldUI();
    }

    // 增加金币
    public void AddGold(int amount)
    {
        CurrentGold += amount;
        Debug.Log($"获得{amount}金币，当前金币：{CurrentGold}");
    }

    // 减少金币（返回是否成功）
    public bool SpendGold(int amount)
    {
        if (CurrentGold >= amount)
        {
            CurrentGold -= amount;
            Debug.Log($"花费{amount}金币，当前金币：{CurrentGold}");
            return true;
        }
        Debug.LogWarning($"金币不足！当前{CurrentGold}，需要{amount}");
        return false;
    }

    // 更新金币UI
    private void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = $"金币：{CurrentGold}";
    }
}
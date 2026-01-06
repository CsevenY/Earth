using UnityEngine;
using UnityEngine.UI; // 只保留原生UI引用，删除TMPro引用

public class PlayerEconomy : MonoBehaviour
{
    [Header("金钱设置")]
    public int startingGold = 1000;
    private int currentGold;
    
    [Header("UI引用")]
    public Text goldText; // 关键：替换为原生Text，删除legacyGoldText备用字段
                          // 无需再保留备用字段，直接用原生Text
    
    private void Start()
    {
        currentGold = startingGold;
        UpdateGoldUI();
    }
    
    // 增加金币（逻辑不变，日志保留中文）
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
        Debug.Log($"获得 {amount} 金币，当前: {currentGold}");
    }
    
    // 花费金币（逻辑不变）
    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateGoldUI();
            Debug.Log($"花费 {amount} 金币，剩余: {currentGold}");
            return true;
        }
        
        Debug.Log($"金币不足！需要: {amount}，当前: {currentGold}");
        return false;
    }
    
    // 获取当前金币（逻辑不变）
    public int GetCurrentGold()
    {
        return currentGold;
    }
    
    // 检查是否买得起（逻辑不变）
    public bool CanAfford(int price)
    {
        return currentGold >= price;
    }
    
    // 更新UI（只改Text赋值，保留中文）
    private void UpdateGoldUI()
    {
        string goldString = $"金币: {currentGold}"; // 保留中文
        
        // 直接赋值给原生Text，无需判断TMPro
        if (goldText != null)
        {
            goldText.text = goldString;
        }
    }
    
    // 测试方法（逻辑不变）
    public void TestAddGold()
    {
        AddGold(100);
    }
    
    public void TestSpendGold()
    {
        SpendGold(50);
    }
}
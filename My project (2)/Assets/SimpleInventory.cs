using UnityEngine;
using System.Collections.Generic;

public class SimpleInventory : MonoBehaviour
{
    // 单个物品的结构（保留原有字段，兼容旧逻辑）
    [System.Serializable]
    public class InventoryItem
    {
        public string itemID;      // 物品唯一标识（英文，用于代码识别）
        public string itemName;    // 物品显示名称（中文，如"胡萝卜"）
        public int quantity;       // 物品数量
        public Sprite icon;        // 物品图标（现在支持了！）
        
        // 构造函数（保留原有+新增重载，兼容新旧逻辑）
        public InventoryItem(string id, string name, int count = 1, Sprite icon = null)
        {
            itemID = id;
            itemName = name;
            quantity = count;
            this.icon = icon;
        }
        
        // 新增：支持传入配置类
        public InventoryItem(ItemConfig config, int count = 1)
        {
            itemID = config.itemID;
            itemName = config.itemName;
            quantity = count;
            icon = config.icon;
        }
    }

    // ========== 新增：通用物品配置类（核心，新增物品只需加配置，不改代码） ==========
    [System.Serializable]
    public class ItemConfig
    {
        public string itemID;      // 物品唯一ID（如wheat、wheat_seed、carrot）
        public string itemName;    // 中文名称（如小麦、小麦种子、胡萝卜）
        public Sprite icon;        // 图标
        public int defaultCount = 1; // 默认数量
        
        // 作物专属扩展（非作物留空即可）
        public GameObject cropPrefab; // 对应的作物预制体
        public float growTime = 5f;   // 生长时间
    }

    [Header("库存设置")]
    public List<InventoryItem> items = new List<InventoryItem>(); // 物品列表
    public int maxCapacity = 20;                                  // 背包最大容量
    
    [Header("初始物品")]
    public InventoryItem[] startingItems;                         // 编辑器可配置的初始物品

    // ========== 新增：全局物品配置表（新增物品只需在这里加配置，不用改其他代码） ==========
    [Header("全局物品配置（新增物品加这里）")]
    public List<ItemConfig> allItemConfigs = new List<ItemConfig>()
    {
        // 默认添加小麦种子、小麦
        new ItemConfig() { itemID = "wheat_seed", itemName = "小麦种子", defaultCount = 1 },
        new ItemConfig() { itemID = "wheat", itemName = "小麦", defaultCount = 1 },
        
        // 苹果相关（新增）
        new ItemConfig() { itemID = "apple_seed", itemName = "苹果种子", defaultCount = 1 },
        new ItemConfig() { itemID = "apple", itemName = "苹果", defaultCount = 1 }
    };
    
    private void Start()
    {
        // 手动添加初始物品（使用通用方法，会自动从配置获取图标）
        AddItem("wheat_seed", 5);  // 5个小麦种子
        AddItem("apple_seed", 3);  // 3个苹果种子
        AddItem("wheat", 2);       // 2个小麦（收获物）
        
        // 同时加载编辑器配置的初始物品（可选）
        if (startingItems != null)
        {
            foreach (var item in startingItems)
            {
                // 从配置获取图标
                ItemConfig config = allItemConfigs.Find(c => c.itemID == item.itemID);
                Sprite icon = config != null ? config.icon : null;
                AddItem(item.itemID, item.itemName, item.quantity, icon);
            }
        }
        
        Debug.Log("库存初始化完成");
        PrintInventory(); // 启动时打印背包内容
    }
    
    // 核心方法：添加物品（保留原有，兼容旧调用）- 新增图标参数
    public void AddItem(string itemID, string itemName, int count = 1, Sprite icon = null)
    {
        // 检查背包是否已满
        if (items.Count >= maxCapacity)
        {
            Debug.LogWarning("背包已满！");
            return;
        }
        
        // 查找已有物品，堆叠数量
        InventoryItem existingItem = items.Find(item => item.itemID == itemID);
        if (existingItem != null)
        {
            existingItem.quantity += count;
            // 如果现有物品没有图标但新物品有图标，更新图标
            if (existingItem.icon == null && icon != null)
            {
                existingItem.icon = icon;
            }
        }
        else
        {
            items.Add(new InventoryItem(itemID, itemName, count, icon));
        }
        
        Debug.Log($"获得 {count} 个 {itemName}");
    }

    // ========== 新增：通用添加物品（根据配置ID，无需传名称，避免写错） ==========
    public void AddItem(string itemID, int count = 1)
    {
        // 从配置表找物品信息
        ItemConfig config = allItemConfigs.Find(c => c.itemID == itemID);
        if (config == null)
        {
            Debug.LogError($"物品配置不存在：{itemID}，请在allItemConfigs中添加！");
            return;
        }
        
        AddItem(itemID, config.itemName, count, config.icon);
    }
    
    // 核心方法：移除物品
    public bool RemoveItem(string itemID, int count = 1)
    {
        InventoryItem existingItem = items.Find(item => item.itemID == itemID);
        if (existingItem != null && existingItem.quantity >= count)
        {
            existingItem.quantity -= count;
            
            // 数量为0时移除物品
            if (existingItem.quantity <= 0)
            {
                items.Remove(existingItem);
            }
            
            Debug.Log($"使用 {count} 个 {existingItem.itemName}");
            return true;
        }
        
        Debug.Log($"没有足够的物品: {itemID}");
        return false;
    }
    
    // 辅助方法：检查是否拥有某物品
    public bool HasItem(string itemID)
    {
        return items.Exists(item => item.itemID == itemID && item.quantity > 0);
    }
    
    // 辅助方法：获取物品数量
    public int GetItemCount(string itemID)
    {
        InventoryItem item = items.Find(i => i.itemID == itemID);
        return item != null ? item.quantity : 0;
    }
    
    // 辅助方法：获取物品图标
    public Sprite GetItemIcon(string itemID)
    {
        InventoryItem item = items.Find(i => i.itemID == itemID);
        return item != null ? item.icon : null;
    }

    // ========== 新增：获取物品配置（给耕地/作物脚本用） ==========
    public ItemConfig GetItemConfig(string itemID)
    {
        return allItemConfigs.Find(c => c.itemID == itemID);
    }
    
    // ========== 新增：设置物品图标 ==========
    public void SetItemIcon(string itemID, Sprite icon)
    {
        InventoryItem item = items.Find(i => i.itemID == itemID);
        if (item != null)
        {
            item.icon = icon;
        }
    }
    
    // 调试方法：打印背包内容（右键菜单可调用）
    [ContextMenu("打印背包内容")]
    public void PrintInventory()
    {
        Debug.Log("=== 背包内容 ===");
        if (items.Count == 0)
        {
            Debug.Log("背包空空如也");
            return;
        }
        
        foreach (var item in items)
        {
            string iconInfo = item.icon != null ? "有图标" : "无图标";
            Debug.Log($"{item.itemName} ({item.itemID}): {item.quantity}个 [{iconInfo}]");
        }
    }
    
    // 新增：检查物品图标状态
    [ContextMenu("检查物品图标状态")]
    public void CheckItemIcons()
    {
        Debug.Log("=== 物品图标状态检查 ===");
        
        foreach (var item in items)
        {
            if (item.icon == null)
            {
                Debug.LogWarning($"{item.itemName} 没有图标！");
                
                // 尝试从配置获取图标
                ItemConfig config = GetItemConfig(item.itemID);
                if (config != null && config.icon != null)
                {
                    item.icon = config.icon;
                    Debug.Log($"已为 {item.itemName} 设置图标");
                }
            }
        }
        
        Debug.Log("检查完成");
    }
    
    // 新增：从配置同步所有图标
    [ContextMenu("从配置同步所有图标")]
    public void SyncIconsFromConfig()
    {
        Debug.Log("=== 从配置同步图标 ===");
        int updatedCount = 0;
        
        foreach (var item in items)
        {
            ItemConfig config = GetItemConfig(item.itemID);
            if (config != null && config.icon != null && item.icon == null)
            {
                item.icon = config.icon;
                updatedCount++;
                Debug.Log($"为 {item.itemName} 设置了图标");
            }
        }
        
        Debug.Log($"同步完成，更新了 {updatedCount} 个物品的图标");
    }
    
    // 辅助方法：检查背包是否已满
    public bool IsFull()
    {
        return items.Count >= maxCapacity;
    }
    
    // 辅助方法：获取所有物品
    public List<InventoryItem> GetAllItems()
    {
        return items;
    }
    
    // 新增：获取物品信息（供UI使用）
    public InventoryItem GetItemInfo(string itemID)
    {
        return items.Find(item => item.itemID == itemID);
    }

    // ========== 仅新增以下2个方法（适配商店买卖，不改动原有任何逻辑） ==========
    /// <summary>
    /// 购买物品（扣金币+加物品，供商店调用）
    /// </summary>
    /// <param name="itemID">物品ID</param>
    /// <param name="count">数量</param>
    /// <param name="error">错误信息</param>
    /// <returns>是否成功</returns>
    public bool BuyItemWithGold(string itemID, int count, out string error)
    {
        error = "";
        
        // 检查物品配置
        ItemConfig config = GetItemConfig(itemID);
        if (config == null)
        {
            error = $"物品配置不存在：{itemID}";
            Debug.LogError(error);
            return false;
        }
        
        // 检查背包容量
        if (IsFull())
        {
            error = "背包已满，无法购买";
            Debug.LogWarning(error);
            return false;
        }
        
        // 调用原有AddItem方法添加物品
        AddItem(itemID, count);
        return true;
    }

    /// <summary>
    /// 出售物品（加金币+减物品，供商店调用）
    /// </summary>
    /// <param name="itemID">物品ID</param>
    /// <param name="count">数量</param>
    /// <param name="goldEarned">获得的金币数</param>
    /// <returns>是否成功</returns>
    public bool SellItemForGold(string itemID, int count, out int goldEarned)
    {
        goldEarned = 0;
        
        // 检查物品是否足够
        if (!RemoveItem(itemID, count))
        {
            Debug.LogWarning($"物品不足，无法出售：{itemID}×{count}");
            return false;
        }
        
        // 返回成功（金币由ShopManager计算）
        return true;
    }
}
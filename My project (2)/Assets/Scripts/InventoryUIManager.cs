using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    // 外部绑定
    public SimpleInventory playerInventory;
    public GameObject inventoryPanel;
    public GameObject itemSlotPrefab;
    public Transform slotParent;

    // 选中状态管理
    public string selectedItemID = "";
    public string lastSelectedItemID = ""; // 改为public，让FarmPlot可以访问
    private Dictionary<string, GameObject> itemSlotDict = new Dictionary<string, GameObject>();
    private Dictionary<string, string> itemNameMap = new Dictionary<string, string>();
    private Dictionary<string, Sprite> itemIconMap = new Dictionary<string, Sprite>();

    void Start()
    {
        Debug.Log("【调试日志】InventoryUIManager已启动");
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        ClearAllSlotsInParent();
        RefreshInventoryUI();
    }

    void Update()
    {
        // 手动背包开关逻辑
        if (Input.GetKeyDown(KeyCode.I) && inventoryPanel != null)
        {
            ToggleInventory();
        }
    }

    // 自动开关背包（供FarmPlot调用）
    public void AutoToggleInventory()
    {
        if (inventoryPanel == null) return;

        // 先关闭背包
        inventoryPanel.SetActive(false);
        Debug.Log("【调试日志】自动关闭背包");

        // 下一帧重新打开背包（避免帧内操作冲突）
        Invoke("OpenInventory", 0.1f);
    }

    // 打开背包（单独封装）
    private void OpenInventory()
    {
        if (inventoryPanel == null) return;
        inventoryPanel.SetActive(true);
        
        // 先设置选中ID，再刷新UI
        selectedItemID = lastSelectedItemID;
        RefreshInventoryUI();
        
        Debug.Log($"【调试日志】自动打开背包，恢复历史选中ID：{selectedItemID}");
    }

    // 手动切换背包状态
    private void ToggleInventory()
    {
        bool wasActive = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!wasActive);
        if (!wasActive)
        {
            // 先设置选中ID，再刷新UI
            selectedItemID = lastSelectedItemID;
            RefreshInventoryUI();
            Debug.Log($"【调试日志】手动打开背包，恢复历史选中ID：{selectedItemID}");
        }
    }

    // 彻底清空背包格子
    private void ClearAllSlotsInParent()
    {
        if (slotParent == null)
        {
            Debug.LogError("【调试日志】slotParent未绑定！");
            return;
        }

        for (int i = slotParent.childCount - 1; i >= 0; i--)
        {
            Destroy(slotParent.GetChild(i).gameObject);
        }

        itemSlotDict.Clear();
        itemNameMap.Clear();
        itemIconMap.Clear();
    }

    // 刷新背包UI（修复图标显示问题）
    public void RefreshInventoryUI()
    {
        if (playerInventory == null || itemSlotPrefab == null || slotParent == null)
        {
            Debug.LogError("【调试日志】核心引用未绑定！");
            return;
        }

        // 清空前先保存选中的物品ID
        string previouslySelectedID = selectedItemID;
        bool hadSelection = !string.IsNullOrEmpty(previouslySelectedID);
        
        ClearAllSlotsInParent();

        Dictionary<string, int> itemTotalCount = new Dictionary<string, int>();
        itemNameMap.Clear();
        itemIconMap.Clear();

        // 统计物品数量并收集图标
        foreach (var item in playerInventory.items)
        {
            if (item == null || string.IsNullOrEmpty(item.itemID) || item.quantity <= 0) continue;

            if (itemTotalCount.ContainsKey(item.itemID))
            {
                itemTotalCount[item.itemID] += item.quantity;
            }
            else
            {
                itemTotalCount.Add(item.itemID, item.quantity);
                itemNameMap.Add(item.itemID, item.itemName);
                itemIconMap.Add(item.itemID, item.icon);
            }
        }

        // 创建新的格子
        foreach (var kvp in itemTotalCount)
        {
            string itemID = kvp.Key;
            int totalQuantity = kvp.Value;
            if (totalQuantity <= 0) continue;

            string itemName = itemNameMap.ContainsKey(itemID) ? itemNameMap[itemID] : "未知物品";
            Sprite itemIcon = itemIconMap.ContainsKey(itemID) ? itemIconMap[itemID] : null;
            
            GameObject slot = Instantiate(itemSlotPrefab, slotParent);
            slot.name = $"Slot_{itemID}";

            // ========== 修复：设置物品图标（关键修复） ==========
            Transform iconTrans = slot.transform.Find("ItemIcon");
            if (iconTrans != null)
            {
                Image iconImage = iconTrans.GetComponent<Image>();
                if (iconImage != null)
                {
                    // 关键修复：始终激活GameObject
                    iconImage.gameObject.SetActive(true);
                    iconImage.enabled = true;
                    
                    if (itemIcon != null)
                    {
                        iconImage.sprite = itemIcon;
                        iconImage.color = Color.white; // 确保白色
                        iconImage.preserveAspect = true;
                        Debug.Log($" 为 {itemName} 设置图标: {itemIcon.name}");
                    }
                    else
                    {
                        // 没有图标时显示占位，不要隐藏！
                        iconImage.sprite = null;
                        iconImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // 半透明深灰色
                        Debug.LogWarning($"{itemName} 没有图标，显示占位");
                    }
                }
                else
                {
                    Debug.LogError($" ItemIcon没有Image组件: {itemName}");
                }
            }
            else
            {
                Debug.LogWarning($" ItemSlot预制体没有找到ItemIcon子对象！");
            }

            // 设置物品名称
            Transform nameTrans = slot.transform.Find("ItemName");
            if (nameTrans != null)
            {
                Text nameText = nameTrans.GetComponent<Text>();
                if (nameText != null) nameText.text = itemName;
            }

            // 设置物品数量
            Transform countTrans = slot.transform.Find("ItemCount");
            if (countTrans != null)
            {
                Text countText = countTrans.GetComponent<Text>();
                if (countText != null) countText.text = totalQuantity.ToString();
            }

            // 强制绑定Button
            Button slotBtn = slot.GetComponent<Button>();
            if (slotBtn == null) slotBtn = slot.AddComponent<Button>();
            slotBtn.interactable = true;

            // 绑定点击事件
            slotBtn.onClick.RemoveAllListeners();
            string tempID = itemID;
            slotBtn.onClick.AddListener(() =>
            {
                Debug.Log($"【调试日志】点击物品格子：{tempID}");
                SelectItem(tempID);
            });

            itemSlotDict.Add(itemID, slot);
        }

        // 恢复选中状态
        RestoreSelectionState(previouslySelectedID, hadSelection);
        
        // 应用高亮
        ApplyHighlightToAllSlots();
        
        // 通知所有耕地刷新提示
        NotifyAllFarmPlots();

        Debug.Log($"【调试日志】背包UI刷新完成，当前选中：{selectedItemID}，历史选中：{lastSelectedItemID}，物品数量：{itemTotalCount.Count}");
    }

    // 恢复选中状态
    private void RestoreSelectionState(string previouslySelectedID, bool hadSelection)
    {
        // 如果之前有选中物品
        if (hadSelection)
        {
            // 检查之前选中的物品是否还在背包中
            if (itemSlotDict.ContainsKey(previouslySelectedID))
            {
                // 物品还在，恢复选中
                selectedItemID = previouslySelectedID;
                lastSelectedItemID = previouslySelectedID;
                Debug.Log($"【调试日志】恢复选中：{selectedItemID}");
            }
            else
            {
                // 物品不在了，尝试选中其他种子
                string fallbackSeedID = FindAnySeedItemID();
                if (!string.IsNullOrEmpty(fallbackSeedID))
                {
                    selectedItemID = fallbackSeedID;
                    lastSelectedItemID = fallbackSeedID;
                    Debug.Log($"【调试日志】选中备用种子：{selectedItemID}");
                }
                else
                {
                    // 没有种子可选，清空选中
                    selectedItemID = "";
                    Debug.Log($"【调试日志】无可选种子，清空选中");
                }
            }
        }
        else
        {
            // 如果之前没有选中，尝试使用历史记录
            if (!string.IsNullOrEmpty(lastSelectedItemID) && itemSlotDict.ContainsKey(lastSelectedItemID))
            {
                selectedItemID = lastSelectedItemID;
                Debug.Log($"【调试日志】使用历史选中：{selectedItemID}");
            }
        }
    }

    // 应用高亮到所有格子
    private void ApplyHighlightToAllSlots()
    {
        foreach (var kvp in itemSlotDict)
        {
            if (kvp.Value == null) continue;
            Image slotImage = kvp.Value.GetComponent<Image>();
            if (slotImage != null)
            {
                bool shouldHighlight = kvp.Key == selectedItemID;
                slotImage.color = shouldHighlight ? Color.yellow : Color.gray;
            }
        }
    }

    // 查找任意种子物品ID
    private string FindAnySeedItemID()
    {
        // 先找种子类物品（以_seed结尾）
        foreach (var kvp in itemSlotDict)
        {
            if (kvp.Key.EndsWith("_seed"))
            {
                return kvp.Key;
            }
        }
        
        // 如果没有种子，返回第一个物品
        foreach (var kvp in itemSlotDict)
        {
            return kvp.Key;
        }
        
        return "";
    }

    // 核心补充：仅更新物品数量，不销毁格子
    public void UpdateItemCountOnly()
    {
        if (playerInventory == null || slotParent == null)
        {
            Debug.LogError("【调试日志】UpdateItemCountOnly：核心引用为空！");
            return;
        }

        // 重新统计物品数量
        Dictionary<string, int> itemTotalCount = new Dictionary<string, int>();
        foreach (var item in playerInventory.items)
        {
            if (item == null || string.IsNullOrEmpty(item.itemID) || item.quantity <= 0) continue;

            if (itemTotalCount.ContainsKey(item.itemID))
            {
                itemTotalCount[item.itemID] += item.quantity;
            }
            else
            {
                itemTotalCount.Add(item.itemID, item.quantity);
            }
        }

        // 仅更新数量文本，不销毁格子
        foreach (var kvp in itemSlotDict)
        {
            string itemID = kvp.Key;
            GameObject slot = kvp.Value;
            if (slot == null) continue;

            // 更新数量
            Transform countTrans = slot.transform.Find("ItemCount");
            if (countTrans != null)
            {
                Text countText = countTrans.GetComponent<Text>();
                if (countText != null)
                {
                    int newCount = itemTotalCount.ContainsKey(itemID) ? itemTotalCount[itemID] : 0;
                    countText.text = newCount.ToString();
                    // 数量为0则隐藏格子
                    slot.SetActive(newCount > 0);
                }
            }
        }

        // 清理数量为0的物品
        List<string> itemsToRemove = new List<string>();
        foreach (var kvp in itemSlotDict)
        {
            string itemID = kvp.Key;
            int currentCount = itemTotalCount.ContainsKey(itemID) ? itemTotalCount[itemID] : 0;
            if (currentCount <= 0)
            {
                itemsToRemove.Add(itemID);
            }
        }
        
        foreach (string itemID in itemsToRemove)
        {
            if (itemSlotDict.ContainsKey(itemID))
            {
                itemSlotDict.Remove(itemID);
            }
        }

        // 更新高亮状态
        ApplyHighlightToAllSlots();

        Debug.Log("【调试日志】仅更新物品数量，未销毁格子，保留选中状态");
    }

    // 选中物品逻辑
    void SelectItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return;

        selectedItemID = itemID.Trim().ToLower();
        lastSelectedItemID = selectedItemID;
        Debug.Log($"【调试日志】选中物品ID：{selectedItemID}");

        // 高亮选中格子
        ApplyHighlightToAllSlots();

        // 通知所有耕地更新
        NotifyAllFarmPlots();
    }

    // 新增：通知所有耕地更新提示
    public void NotifyAllFarmPlots()
    {
        FarmPlot[] allFarmPlots = FindObjectsOfType<FarmPlot>();
        Debug.Log($"【调试日志】通知 {allFarmPlots.Length} 个耕地更新提示");
        
        foreach (var plot in allFarmPlots)
        {
            plot.ForceUpdatePrompt();
        }
    }

    // 清空当前选中（保留历史）
    public void ClearSelectedWithoutHistory()
    {
        selectedItemID = "";
        ApplyHighlightToAllSlots();
        Debug.Log($"【调试日志】清空当前选中，保留历史ID：{lastSelectedItemID}");
    }

    // 清空所有选中（包括历史）
    public void ClearSelectedItem()
    {
        selectedItemID = "";
        lastSelectedItemID = "";
        ApplyHighlightToAllSlots();
    }
    
    // 强制设置选中状态（供外部调用）
    public void ForceSelectItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return;
        
        selectedItemID = itemID.Trim().ToLower();
        lastSelectedItemID = selectedItemID;
        
        // 立即应用高亮
        ApplyHighlightToAllSlots();
        
        // 通知耕地更新
        NotifyAllFarmPlots();
        
        Debug.Log($"【调试日志】强制选中物品ID：{selectedItemID}");
    }
    
    // 新增：调试方法
    [ContextMenu("调试背包UI状态")]
    public void DebugInventoryUIState()
    {
        Debug.Log($"=== 背包UI状态调试 ===");
        Debug.Log($"当前选中: {selectedItemID}");
        Debug.Log($"历史选中: {lastSelectedItemID}");
        Debug.Log($"背包格子数量: {itemSlotDict.Count}");
        Debug.Log($"物品名称映射数量: {itemNameMap.Count}");
        Debug.Log($"物品图标映射数量: {itemIconMap.Count}");
        
        // 检查每个格子的图标状态
        foreach (var kvp in itemSlotDict)
        {
            string itemID = kvp.Key;
            GameObject slot = kvp.Value;
            
            Transform iconTrans = slot.transform.Find("ItemIcon");
            bool hasIconObject = iconTrans != null;
            bool iconActive = false;
            Sprite currentSprite = null;
            
            if (hasIconObject && iconTrans.gameObject.activeSelf)
            {
                Image iconImage = iconTrans.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconActive = iconImage.enabled && iconImage.sprite != null;
                    currentSprite = iconImage.sprite;
                }
            }
            
            Debug.Log($"  - {itemID}: 有图标对象={hasIconObject}, 激活={iconTrans?.gameObject.activeSelf}, 图片={(currentSprite != null ? currentSprite.name : "NULL")}");
        }
        
        Debug.Log($"=====================");
    }
    
    // 新增：测试图标显示
    [ContextMenu("测试强制显示图标")]
    public void TestForceShowIcons()
    {
        Debug.Log("=== 强制显示图标测试 ===");
        
        if (slotParent == null) return;
        
        foreach (Transform child in slotParent)
        {
            Transform iconTrans = child.Find("ItemIcon");
            if (iconTrans != null)
            {
                Image iconImage = iconTrans.GetComponent<Image>();
                if (iconImage != null)
                {
                    // 强制设置为测试颜色
                    if (iconImage.sprite == null)
                    {
                        iconImage.color = Color.blue;
                        iconImage.gameObject.SetActive(true);
                        iconImage.enabled = true;
                        Debug.Log($"为 {child.name} 设置蓝色测试");
                    }
                    else
                    {
                        Debug.Log($"{child.name} 已有图片: {iconImage.sprite.name}");
                    }
                }
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FarmPlot : MonoBehaviour
{
    // 外部绑定
    [Header("耕地配置")]
    public ItemDataSO[] availableSeeds; // 改为数组，这个耕地可用的种子类型
    public GameObject plantPromptWindow;
    public Text promptText;
    public InventoryUIManager inventoryUI;
    public Transform player;
    public float detectRange = 2f;
    public float plantYOffset = 0.1f;

    // 状态管理
    public bool isPlanted = false;
    private bool isPromptShowing = false;
    private SimpleInventory playerInventory;
    private bool isProcessing = false;

    // 当前检测到的可用种子
    private ItemDataSO detectedSeed = null;
    
    // 新增：记录当前耕地上的作物
    private Crop currentCrop = null;

    void Start()
    {
        // 初始化提示窗
        if (plantPromptWindow != null) plantPromptWindow.SetActive(false);
        if (promptText != null) promptText.text = "";

        // 获取背包引用
        playerInventory = FindObjectOfType<SimpleInventory>();
        if (playerInventory == null)
        {
            Debug.LogError($"【调试日志】FarmPlot {gameObject.name}：找不到SimpleInventory组件！");
        }

        // 校验player绑定
        if (player == null)
        {
            Debug.LogError($"【调试日志】FarmPlot {gameObject.name}：player未绑定！请拖入玩家对象");
        }

        // 校验seedItems绑定
        if (availableSeeds == null || availableSeeds.Length == 0)
        {
            Debug.LogError($"【调试日志】FarmPlot {gameObject.name}：availableSeeds未绑定！请拖入种子配置");
        }
        
        // 检查是否已有作物
        CheckExistingCrop();
    }

    void Update()
    {
        // 检测玩家距离
        bool isPlayerNear = CheckPlayerDistance();

        // 更新提示窗
        UpdatePromptWindow(isPlayerNear);

        // 按E播种（只有靠近且未种植时才响应）
        if (Input.GetKeyDown(KeyCode.E) && isPlayerNear && !isProcessing)
        {
            TryPlantCrop();
        }
        if (Input.GetKeyDown(KeyCode.Q) && isPlayerNear && !isProcessing)
        {
            TryWaterCrop();
        }
    }
    public void TryWaterCrop()
    {
        if (isProcessing) return;

        isProcessing = true;

        try
        {
            // 检查当前耕地上是否有作物
            if (currentCrop == null)
            {
                // 如果有子作物对象但currentCrop未赋值，尝试获取
                Crop existingCrop = GetComponentInChildren<Crop>();
                if (existingCrop != null)
                {
                    currentCrop = existingCrop;
                }
                else
                {
                    Debug.Log("当前耕地没有作物，无法浇水");
                    if (promptText != null)
                    {
                        promptText.text = "没有作物可浇水";
                        promptText.color = Color.red;
                        StartCoroutine(ClearWateringPrompt());
                    }
                    return;
                }
            }

            // 检查作物是否已成熟
            if (currentCrop.CurrentState == Crop.CropState.Mature)
            {
                Debug.Log("作物已成熟，无需浇水");
                if (promptText != null)
                {
                    promptText.text = "作物已成熟，无需浇水";
                    promptText.color = Color.yellow;
                    StartCoroutine(ClearWateringPrompt());
                }
                return;
            }

            // 执行浇水
            currentCrop.WaterCrop();

            // 显示浇水成功提示
            if (promptText != null)
            {
                promptText.text = "浇水成功！生长加速";
                promptText.color = Color.blue;
                StartCoroutine(ClearWateringPrompt());
            }

            Debug.Log($"为作物 {currentCrop.CropName} 浇水成功");
        }
        finally
        {
            isProcessing = false;
        }
    }
    private IEnumerator ClearWateringPrompt()
    {
        if (plantPromptWindow != null)
        {
            plantPromptWindow.SetActive(true);
        }

        yield return new WaitForSeconds(2f); // 显示2秒

        // 强制更新提示窗，恢复正常状态
        ForceUpdatePrompt();
    }
    // 检查是否已有作物
    private void CheckExistingCrop()
    {
        Crop existingCrop = GetComponentInChildren<Crop>();
        if (existingCrop != null)
        {
            currentCrop = existingCrop;
            isPlanted = true;
            Debug.Log($"耕地 {gameObject.name} 已有作物: {currentCrop.CropName}");
        }
    }

    // 独立的玩家距离检测
    private bool CheckPlayerDistance()
    {
        if (player == null)
        {
            Debug.LogError($"【调试日志】{gameObject.name} CheckPlayerDistance：player为空！");
            return false;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool isNear = distance <= detectRange;
        return isNear;
    }

    // 强制更新提示窗（供外部调用）
    public void ForceUpdatePrompt()
    {
        bool isPlayerNear = CheckPlayerDistance();
        UpdatePromptWindow(isPlayerNear);
    }

    // 更新提示窗核心逻辑
    public void UpdatePromptWindow(bool isPlayerNear)
    {
        // 基础校验
        if (plantPromptWindow == null)
        {
            Debug.LogError($"【调试日志】{gameObject.name}：plantPromptWindow未绑定！");
            return;
        }

        if (promptText == null)
        {
            Debug.LogError($"【调试日志】{gameObject.name}：promptText未绑定！");
            return;
        }

        if (inventoryUI == null)
        {
            Debug.LogError($"【调试日志】{gameObject.name}：inventoryUI未绑定！");
            return;
        }

        if (availableSeeds == null || availableSeeds.Length == 0)
        {
            plantPromptWindow.SetActive(false);
            return;
        }
        if (currentCrop != null && currentCrop.CurrentState != Crop.CropState.Mature && isPlayerNear)
        {
            // 显示浇水提示
            plantPromptWindow.SetActive(true);
            promptText.text = "按Q键浇水加速生长";
            promptText.color = Color.blue;
            isPromptShowing = true;
            return;
        }

        // 先检查是否有成熟作物在范围内（优先显示收获提示）
        Crop nearbyMatureCrop = CheckForNearbyMatureCrop();
        if (nearbyMatureCrop != null && isPlayerNear)
        {
            plantPromptWindow.SetActive(true);
            promptText.text = "按E键收获";
            promptText.color = Color.red;
            isPromptShowing = true;
            return;
        }

        // 已种植但未成熟则隐藏提示
        if (isPlanted)
        {
            if (isPromptShowing)
            {
                plantPromptWindow.SetActive(false);
                isPromptShowing = false;
            }
            return;
        }

        // 检测玩家背包中是否有可用种子且被选中
        detectedSeed = GetAvailableSelectedSeed();
        if (isPlayerNear && detectedSeed != null)
        {
            plantPromptWindow.SetActive(true);
            promptText.text = $"按E键播种（消耗1个{detectedSeed.itemName}）";
            promptText.color = Color.black;
            isPromptShowing = true;
        }
        else
        {
            plantPromptWindow.SetActive(false);
            isPromptShowing = false;
        }
    }

    // 获取当前可用且被选中的种子
    private ItemDataSO GetAvailableSelectedSeed()
    {
        if (inventoryUI == null || string.IsNullOrEmpty(inventoryUI.selectedItemID))
            return null;

        string selectedID = inventoryUI.selectedItemID.Trim().ToLower();

        // 检查选中的物品是否是当前耕地可用的种子
        if (availableSeeds != null)
        {
            foreach (ItemDataSO seed in availableSeeds)
            {
                if (seed == null) continue;
                string seedID = seed.itemID.Trim().ToLower();
                if (seedID == selectedID)
                {
                    // 还需要检查背包中是否有这个种子
                    if (playerInventory != null && playerInventory.GetItemCount(seedID) > 0)
                    {
                        Debug.Log($"找到可用种子: {seed.itemName}");
                        return seed;
                    }
                }
            }
        }
        return null;
    }

    // 检查附近是否有成熟作物
    private Crop CheckForNearbyMatureCrop()
    {
        if (player == null) return null;

        // 检查当前耕地上是否有成熟作物
        Crop[] cropsInPlot = GetComponentsInChildren<Crop>();
        foreach (Crop crop in cropsInPlot)
        {
            if (crop.CurrentState == Crop.CropState.Mature)
            {
                float distance = Vector3.Distance(player.position, crop.transform.position);
                if (distance <= detectRange)
                {
                    return crop;
                }
            }
        }
        return null;
    }

    // 种植作物（通用方法）
    public void TryPlantCrop()
    {
        // 防重复处理检查
        if (isProcessing)
        {
            Debug.Log($"【调试日志】{gameObject.name}：正在处理中，跳过");
            return;
        }

        isProcessing = true;

        try
        {
            Debug.Log($"【调试日志】{gameObject.name}：开始种植处理");

            // 先检查是否有成熟作物，优先收获
            Crop nearbyMatureCrop = CheckForNearbyMatureCrop();
            if (nearbyMatureCrop != null)
            {
                Debug.Log($"【调试日志】{gameObject.name}：发现成熟作物，开始收获...");
                bool harvestSuccess = nearbyMatureCrop.Harvest();
                if (harvestSuccess)
                {
                    // 收获成功后立即更新状态
                    isPlanted = false;
                    currentCrop = null;
                    UpdatePromptWindow(true); // 立即更新提示
                }
                return; // 收获后返回，不继续种植
            }

            // 检查是否已种植
            if (isPlanted)
            {
                Debug.LogWarning($"【调试日志】{gameObject.name}：耕地已种植，无法再次播种！");
                if (promptText != null)
                {
                    promptText.text = "耕地已种植！";
                    promptText.color = Color.red;
                }
                return;
            }

            // 基础校验
            if (inventoryUI == null)
            {
                Debug.LogError($"【调试日志】{gameObject.name}：inventoryUI未绑定！");
                return;
            }

            // 获取当前可用的种子
            detectedSeed = GetAvailableSelectedSeed();
            if (detectedSeed == null)
            {
                Debug.LogWarning($"【调试日志】{gameObject.name}：未选中可用种子或种子不足！");
                if (promptText != null)
                {
                    promptText.text = "请选中可用种子！";
                    promptText.color = Color.red;
                }
                return;
            }

            if (detectedSeed.cropPrefab == null)
            {
                Debug.LogError($"【调试日志】{gameObject.name}：{detectedSeed.itemName}未绑定作物预制体！");
                return;
            }

            // 校验背包引用
            if (playerInventory == null)
            {
                playerInventory = FindObjectOfType<SimpleInventory>();
                if (playerInventory == null)
                {
                    Debug.LogError($"【调试日志】{gameObject.name}：找不到SimpleInventory组件！");
                    return;
                }
            }

            // 检测种子数量
            int seedCount = playerInventory.GetItemCount(detectedSeed.itemID);
            if (seedCount <= 0)
            {
                Debug.LogWarning($"【调试日志】{gameObject.name}：{detectedSeed.itemName}数量不足（剩余{seedCount}）！");
                if (promptText != null)
                {
                    promptText.text = $"{detectedSeed.itemName}数量不足！";
                    promptText.color = Color.red;
                }
                return;
            }

            // 消耗种子
            bool removeSuccess = playerInventory.RemoveItem(detectedSeed.itemID, 1);
            if (!removeSuccess)
            {
                Debug.LogError($"【调试日志】{gameObject.name}：消耗{detectedSeed.itemName}失败！");
                if (promptText != null)
                {
                    promptText.text = "消耗种子失败！";
                    promptText.color = Color.red;
                }
                return;
            }

            // 生成作物
            Vector3 plantPos = new Vector3(
                transform.position.x,
                transform.position.y + plantYOffset,
                transform.position.z
            );
            GameObject spawnedCrop = Instantiate(detectedSeed.cropPrefab, plantPos, Quaternion.identity, transform);

            if (spawnedCrop == null)
            {
                Debug.LogError($"【调试日志】{gameObject.name}：生成作物失败！");
                // 回滚种子
                playerInventory.AddItem(detectedSeed.itemID, 1);
                if (promptText != null)
                {
                    promptText.text = "生成作物失败！";
                    promptText.color = Color.red;
                }
                return;
            }

            spawnedCrop.SetActive(true);
            spawnedCrop.layer = LayerMask.NameToLayer("Interactable");

            // 获取作物组件并记录
            Crop newCrop = spawnedCrop.GetComponent<Crop>();
            if (newCrop != null)
            {
                currentCrop = newCrop;
            }

            // 更新状态
            isPlanted = true;

            if (plantPromptWindow != null) plantPromptWindow.SetActive(false);

            if (inventoryUI != null)
            {
                inventoryUI.RefreshInventoryUI(); // 只刷新UI，不清空选中状态
            }

            // 提示播种成功
            int remainingCount = playerInventory.GetItemCount(detectedSeed.itemID);
            Debug.Log($"【调试日志】{gameObject.name}播种成功！消耗1个{detectedSeed.itemName}，剩余{remainingCount}个");

            if (promptText != null)
            {
                promptText.text = $"播种成功！剩余{remainingCount}个{detectedSeed.itemName}";
                promptText.color = Color.green;
            }
        }
        finally
        {
            // 确保最终重置处理标志
            isProcessing = false;
        }
    }

    // 重置耕地状态（由Crop.Harvest调用）
    public void ResetPlot()
    {
        // 防止在重置过程中重复种植
        if (isProcessing) return;

        isPlanted = false;
        isPromptShowing = false;
        detectedSeed = null;
        currentCrop = null; // 清除作物引用

        // 立即更新提示窗口
        if (plantPromptWindow != null) plantPromptWindow.SetActive(false);

        // 强制更新提示，避免状态延迟
        ForceUpdatePrompt();

        Debug.Log($"【调试日志】耕地{gameObject.name}已重置，可重新播种");
    }

    // Gizmos可视化
    void OnDrawGizmosSelected()
    {
        // 检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // 种植位置
        Gizmos.color = Color.green;
        Vector3 plantPos = transform.position + new Vector3(0f, plantYOffset, 0f);
        Gizmos.DrawSphere(plantPos, 0.2f);
    }

    // 新增：获取当前选中的种子ID（供调试用）
    public string GetCurrentSelectedSeedID()
    {
        if (inventoryUI == null) return "null";
        return string.IsNullOrEmpty(inventoryUI.selectedItemID) ? "空" : inventoryUI.selectedItemID;
    }

    // 新增：调试耕地状态
    [ContextMenu("调试耕地状态")]
    public void DebugPlotState()
    {
        Debug.Log($"=== 耕地 {gameObject.name} 状态调试 ===");
        Debug.Log($"已种植: {isPlanted}");
        Debug.Log($"检测到的种子: {(detectedSeed != null ? detectedSeed.itemName : "无")}");
        Debug.Log($"UI选中的种子: {GetCurrentSelectedSeedID()}");
        Debug.Log($"玩家距离: {(player != null ? Vector3.Distance(transform.position, player.position).ToString("F2") : "无玩家")}");
        // 检查成熟作物
        Crop matureCrop = CheckForNearbyMatureCrop();
        Debug.Log($"成熟作物: {(matureCrop != null ? matureCrop.CropName : "无")}");
        Debug.Log($"==============================");
    }
}
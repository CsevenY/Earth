using UnityEngine;
using UnityEngine.UI;

public class CraftingStation : MonoBehaviour
{
    [Header("制作台配置")]
    public ItemCraftDataSO targetCraftItem; // 制作台要产出的物品（如小麦酒）
    public Text promptText;
    public Transform triggerZone;
    
    [Header("文本样式")]
    public int textFontSize = 24;
    public Color textColor = Color.white;
    
    // 核心状态
    private bool isCrafting;
    private float craftTimer;
    private bool isPlayerNearby;
    private bool isCraftComplete;
    private SimpleInventory playerInventory;
    private PlayerController playerController;
    private InventoryUIManager inventoryUIManager;

    private void Start()
    {
        // 获取核心组件引用
        playerInventory = FindObjectOfType<SimpleInventory>();
        playerController = FindObjectOfType<PlayerController>();
        inventoryUIManager = FindObjectOfType<InventoryUIManager>();
        
        InitPromptText();
        BindTriggerEvent();
    }

    /// <summary>
    /// 初始化提示文本
    /// </summary>
    private void InitPromptText()
    {
        if (promptText == null)
        {
            Debug.LogError("【制作台】promptText未绑定！");
            return;
        }
        promptText.fontSize = textFontSize;
        promptText.color = textColor;
        promptText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 绑定触发器事件
    /// </summary>
    private void BindTriggerEvent()
    {
        if (triggerZone == null)
        {
            Debug.LogError("【制作台】triggerZone未绑定！");
            return;
        }

        TriggerDetector detector = triggerZone.GetComponent<TriggerDetector>();
        if (detector == null)
        {
            detector = triggerZone.gameObject.AddComponent<TriggerDetector>();
        }
        detector.onPlayerEnter += OnPlayerEnter;
        detector.onPlayerExit += OnPlayerExit;
    }

    private void Update()
    {
        // 制作计时逻辑
        if (isCrafting)
        {
            UpdateCraftTimer();
            return;
        }

        // 玩家交互逻辑
        if (isPlayerNearby && playerController != null && !playerController.isInShop)
        {
            UpdatePromptText();
            if (Input.GetKeyDown(KeyCode.E))
            {
                HandleInteraction();
            }
        }
    }

    /// <summary>
    /// 更新制作倒计时
    /// </summary>
    private void UpdateCraftTimer()
    {
        craftTimer += Time.deltaTime;
        float remainingTime = Mathf.Max(0, targetCraftItem.craftTime - craftTimer);
        promptText.text = $"制作{targetCraftItem.itemName}中...({remainingTime:F1}s)";

        // 制作完成
        if (craftTimer >= targetCraftItem.craftTime)
        {
            isCrafting = false;
            isCraftComplete = true;
            craftTimer = 0f;
            promptText.text = string.Format(targetCraftItem.harvestPrompt, targetCraftItem.itemName);
        }
    }

    /// <summary>
    /// 更新提示文本
    /// </summary>
    private void UpdatePromptText()
    {
        if (!targetCraftItem.isCraftable)
        {
            promptText.text = "该物品无法制作";
            return;
        }

        if (isCraftComplete)
        {
            promptText.text = string.Format(targetCraftItem.harvestPrompt, targetCraftItem.itemName);
            return;
        }

        // 检查输入材料是否足够
        bool hasEnough = playerInventory.GetItemCount(targetCraftItem.craftInputItem.itemID) >= targetCraftItem.craftInputCount;
        if (hasEnough)
        {
            // 替换提示文本占位符：{0}=成品名 {1}=输入数量 {2}=输入物品名
            promptText.text = string.Format(
                targetCraftItem.craftPrompt,
                targetCraftItem.itemName,
                targetCraftItem.craftInputCount,
                targetCraftItem.craftInputItem.itemName
            );
        }
        else
        {
            // 材料不足提示
            promptText.text = string.Format(
                targetCraftItem.lackMaterialPrompt,
                targetCraftItem.craftInputItem.itemName,
                targetCraftItem.itemName
            );
        }
    }

    /// <summary>
    /// 处理玩家E键交互
    /// </summary>
    private void HandleInteraction()
    {
        if (targetCraftItem == null || !targetCraftItem.isCraftable || playerInventory == null)
        {
            return;
        }

        // 收获逻辑
        if (isCraftComplete)
        {
            // 添加制作好的物品到背包
            playerInventory.AddItem(
                targetCraftItem.itemID,
                targetCraftItem.itemName,
                1,
                targetCraftItem.itemIcon
            );
            inventoryUIManager.RefreshInventoryUI();
            
            // 重置制作状态
            isCraftComplete = false;
            UpdatePromptText();
            return;
        }

        // 制作逻辑
        bool hasEnough = playerInventory.GetItemCount(targetCraftItem.craftInputItem.itemID) >= targetCraftItem.craftInputCount;
        if (hasEnough)
        {
            // 消耗输入材料
            playerInventory.RemoveItem(targetCraftItem.craftInputItem.itemID, targetCraftItem.craftInputCount);
            inventoryUIManager.RefreshInventoryUI();
            
            // 开始制作
            isCrafting = true;
            craftTimer = 0f;
            UpdateCraftTimer();
        }
    }

    /// <summary>
    /// 玩家进入触发器
    /// </summary>
    private void OnPlayerEnter()
    {
        isPlayerNearby = true;
        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            UpdatePromptText();
        }
    }

    /// <summary>
    /// 玩家离开触发器
    /// </summary>
    private void OnPlayerExit()
    {
        isPlayerNearby = false;
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 绘制调试范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (triggerZone != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(triggerZone.position, 2f);
        }
    }
}

/// <summary>
/// 触发器检测组件
/// </summary>
public class TriggerDetector : MonoBehaviour
{
    public System.Action onPlayerEnter;
    public System.Action onPlayerExit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onPlayerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onPlayerExit?.Invoke();
        }
    }
}
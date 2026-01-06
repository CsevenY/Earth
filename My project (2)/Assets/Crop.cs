using UnityEngine;

public class Crop : MonoBehaviour
{
    // 作物基础信息
    public string CropName = "小麦";
    public enum CropState { Seed, Growing, Mature }
    public CropState CurrentState = CropState.Seed;

    // 生长设置
    public float GrowthTime = 5f;
    private float growthProgress = 0f;

    // 各阶段模型（大小完全由预制体决定）
    public GameObject SeedModel;
    public GameObject GrowingModel;
    public GameObject MatureModel;

    // 收获配置
    public ItemDataSO harvestItem;
    public int harvestCount = 1;

    // 引用
    private SimpleInventory playerInventory;
    private InventoryUIManager inventoryUI;
    private Transform player;

    // 仅保留耕地贴合配置（不涉及缩放）
    [Header("耕地贴合配置")]
    public float cropYOffset = 0.1f;

    // 新增：防止重复收获标志
    private bool isBeingHarvested = false;

    void Start()
    {
        // 只调整位置，不修改任何缩放
        CalibrateToFarmPlot();

        // 只控制显示/隐藏，不修改缩放
        UpdateModelVisibility();

        // 获取核心引用
        playerInventory = FindObjectOfType<SimpleInventory>();
        inventoryUI = FindObjectOfType<InventoryUIManager>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 配置校验
        if (harvestItem == null) Debug.LogError("Crop：harvestItem未绑定！");
        if (player == null)
            Debug.LogError("Crop：未找到玩家（Player标签未设置）！");

        // 新增：禁用作物的碰撞体，防止顶起玩家
        DisableCropCollision();
    }

    void Update()
    {
        // 1. 生长逻辑（仅切换状态，不修改缩放）
        if (CurrentState != CropState.Mature)
        {
            growthProgress += Time.deltaTime;
            UpdateGrowthState();
            UpdateModelVisibility();
        }
    }

    // 更新生长状态（仅切换枚举值）
    private void UpdateGrowthState()
    {
        if (growthProgress >= GrowthTime)
        {
            CurrentState = CropState.Mature;
            Debug.Log($"{CropName}已成熟！");
        }
        else if (growthProgress >= GrowthTime * 0.5f)
        {
            CurrentState = CropState.Growing;
            Debug.Log($"{CropName}进入生长期...");
        }
    }

    // 仅控制模型显示/隐藏，不做任何缩放修改
    private void UpdateModelVisibility()
    {
        // 先隐藏所有模型
        SeedModel?.SetActive(false);
        GrowingModel?.SetActive(false);
        MatureModel?.SetActive(false);

        // 按状态显示对应模型（大小完全用预制体原始值）
        switch (CurrentState)
        {
            case CropState.Seed:
                if (SeedModel != null) SeedModel.SetActive(true);
                break;
            case CropState.Growing:
                if (GrowingModel != null) GrowingModel.SetActive(true);
                break;
            case CropState.Mature:
                if (MatureModel != null) MatureModel.SetActive(true);
                break;
        }
    }

    // 新增：禁用作物碰撞体
    private void DisableCropCollision()
    {
        // 获取所有碰撞体
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            // 将碰撞体设为触发器，这样不会产生物理推动
            collider.isTrigger = true;
        }
        
        // 获取所有刚体并设置为运动学（如果存在）
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
        
        Debug.Log($"作物 {CropName} 碰撞体已设为触发器，不会顶起玩家");
    }

    // 核心收获方法（仅添加物品，不修改物品模型缩放）
    public bool Harvest()
    {
        // 防止重复收获
        if (isBeingHarvested)
        {
            Debug.LogWarning($"Crop：{CropName}正在被收获，跳过重复调用");
            return false;
        }

        // 状态/配置校验
        if (CurrentState != CropState.Mature || harvestItem == null || playerInventory == null)
        {
            Debug.LogError("收获失败：状态/配置异常！");
            return false;
        }

        // 标记为正在收获
        isBeingHarvested = true;

        Debug.Log($"开始收获：{CropName}，数量：{harvestCount}");

        // 关键修改：先重置耕地状态，再添加物品
        FarmPlot farmPlot = transform.parent?.GetComponent<FarmPlot>();
        if (farmPlot != null)
        {
            farmPlot.ResetPlot(); // 立即重置耕地状态
            Debug.Log("耕地状态已重置，可重新播种");
        }

        // 添加收获物品
        playerInventory.AddItem(harvestItem.itemID, harvestCount);
        Debug.Log($"收获成功！获得{harvestCount}个{harvestItem.itemName}");

        // 刷新UI
        if (inventoryUI != null)
        {
            inventoryUI.RefreshInventoryUI();
        }

        // 销毁作物对象
        Destroy(gameObject);

        return true;
    }

    // 手动收获测试（右键菜单调用）
    [ContextMenu("手动收获（测试用）")]
    public void ManualHarvest()
    {
        CurrentState = CropState.Mature;
        Harvest();
    }

    // 仅调整模型位置贴合耕地，不修改任何缩放
    private void CalibrateToFarmPlot()
    {
        if (transform.parent != null)
        {
            Vector3 farmPlotPos = transform.parent.position;
            // 仅调整Y轴高度，让模型贴地
            transform.position = new Vector3(
                farmPlotPos.x, farmPlotPos.y + cropYOffset, farmPlotPos.z
            );
        }
        else
        {
            transform.position = new Vector3(transform.position.x, cropYOffset, transform.position.z);
        }

        // 仅重置模型位置/旋转，不修改缩放
        ResetModelTransform(SeedModel);
        ResetModelTransform(GrowingModel);
        ResetModelTransform(MatureModel);
    }

    // 仅重置模型位置和旋转，保留预制体原始缩放
    private void ResetModelTransform(GameObject model)
    {
        if (model != null)
        {
            // 仅让模型在作物对象内居中，不修改缩放
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            // 完全注释缩放重置，100%保留预制体缩放
            // model.transform.localScale = Vector3.one;
        }
    }

    // 绘制可收获范围（辅助调试）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 2f); // 固定2米范围
    }
}
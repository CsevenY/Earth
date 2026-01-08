using UnityEngine;

/// <summary>
/// 融合版：物品属性 + 制作配方配置（替代原ItemDataSO + CraftingStationSO）
/// </summary>
[CreateAssetMenu(fileName = "Item_WheatSeed", menuName = "游戏配置/物品配置")]
public class ItemCraftDataSO : ScriptableObject
{
    [Header("========= 基础物品属性 =========")]
    public string itemID; // 物品唯一ID（如wheat_seed、wheat_wine）
    public string itemName; // 物品显示名称（如小麦种子、小麦酒）
    public Sprite itemIcon; // 物品图标
    public int defaultCount = 1; // 默认数量

    [Header("========= 作物专属属性（非作物留空） =========")]
    public GameObject cropPrefab; // 对应的作物预制体
    public float growTime = 5f; // 作物生长时间

    [Header("========= 制作配方属性（非制作物品留空） =========")]
    public bool isCraftable = false; // 是否可制作
    public ItemCraftDataSO craftInputItem; // 制作所需的输入物品（如小麦种子）
    public int craftInputCount = 1; // 制作所需输入数量
    public float craftTime = 5f; // 制作耗时（秒）
    // 制作提示文本（{0}自动替换为输入/输出物品名称）
    public string craftPrompt = "按E制作{0}（消耗{1}个{2}）";
    public string lackMaterialPrompt = "{0}不足，无法制作{1}！";
    public string harvestPrompt = "按E收获{0}";
}
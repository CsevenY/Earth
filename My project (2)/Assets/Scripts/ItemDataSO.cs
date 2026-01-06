using UnityEngine;

// 物品配置文件（可在Project面板右键创建）
[CreateAssetMenu(fileName = "NewItem", menuName = "游戏配置/物品配置")]
public class ItemDataSO : ScriptableObject
{
    [Header("基础配置")]
    public string itemID; // 物品唯一ID（如wheat、wheat_seed）
    public string itemName; // 物品显示名称（如小麦、小麦种子）
    public Sprite itemIcon; // 物品图标（可选）
    public int defaultCount = 1; // 默认数量

    [Header("作物专属配置（非作物留空）")]
    public GameObject cropPrefab; // 对应的作物预制体（如小麦预制体）
    public float growTime = 5f; // 生长时间
}
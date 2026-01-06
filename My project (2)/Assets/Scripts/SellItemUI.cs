using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 出售物品UI - 与BuyItemUI逻辑对齐（极简版，保证点击）
/// </summary>
public class SellItemUI : MonoBehaviour
{
    [Header("UI引用")]
    public Image itemIcon; // 商品图标
    public Text itemNameText; // 商品名称
    public Text priceText; // 价格文本
    public Button sellButton; // 出售按钮

    // 与BuyItemUI参数格式完全一致（仅改文本和按钮名）
    public void SetItemInfo(string name, Sprite icon, int price, System.Action onSellClick)
    {
        // 1. 设置基础信息（和BuyItemUI逻辑一致）
        itemNameText.text = name;
        priceText.text = string.Format("售价：{0}金币", price);

        // 2. 设置图标（和BuyItemUI逻辑一致）
        if (icon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = true;
        }
        else
        {
            itemIcon.enabled = false;
        }

        // 3. 绑定按钮事件（完全复用BuyItemUI的稳定绑定方式）
        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(new UnityEngine.Events.UnityAction(onSellClick));

        // 关键：强制启用按钮（和BuyItemUI默认状态对齐）
        sellButton.interactable = true;
    }
}
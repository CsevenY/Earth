using UnityEngine;
using UnityEngine.UI;

public class BuyItemUI : MonoBehaviour
{
    [Header("UI引用")]
    public Image itemIcon; // 商品图标
    public Text itemNameText; // 商品名称（原生Text）
    public Text priceText; // 价格文本（原生Text）
    public Button buyButton; // 购买按钮

    // 4个参数，与ShopUIManager调用匹配
    public void SetItemInfo(string name, Sprite icon, int price, System.Action onBuyClick)
    {
        // 设置名称
        itemNameText.text = name;
        // 设置购买价格
        priceText.text = string.Format("买入：{0}金币", price);
        // 设置图标（无图标时隐藏）
        if (icon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = true;
        }
        else
        {
            itemIcon.enabled = false;
        }

        // 绑定购买按钮事件
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(new UnityEngine.Events.UnityAction(onBuyClick));
    }
}
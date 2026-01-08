using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class ShopUIManager : MonoBehaviour
{
    [Header("页面引用")]
    public GameObject mainPage;         // 主页面
    public GameObject buyPage;          // 购买页面
    public GameObject sellPage;         // 出售页面

    [Header("按钮引用")]
    public Button buyButton;            // 购买按钮
    public Button sellButton;           // 出售按钮
    public Button closeButton;          // 关闭按钮
    public Button buyPageBackButton;    // 购买返回按钮
    public Button sellPageBackButton;   // 出售返回按钮

    [Header("商品预制体")]
    public GameObject buyItemPrefab;    // 购买商品预制体
    public GameObject sellItemPrefab;   // 出售商品预制体

    [Header("商品容器")]
    public Transform buyItemContainer;  // 购买容器
    public Transform sellItemContainer; // 出售容器

    [Header("布局配置")]
    public float itemHeight = 100f;
    public float itemSpacing = 20f;
    public float startXPosition = 0f;
    public float startYPosition = 0f;

    // 核心引用
    private SimpleInventory playerInventory;
    private Merchant merchant;
    private GoldManager goldManager;
    // 新增：缓存背包UI引用，避免重复FindObjectOfType
    private InventoryUIManager inventoryUI;

    [Header("商店配置")]
    public List<ShopItemConfig> shopItems = new List<ShopItemConfig>()
    {
        new ShopItemConfig("wheat_seed", "小麦种子", 5, 2),
        new ShopItemConfig("apple_seed", "苹果种子", 8, 3),
        new ShopItemConfig("wheat", "小麦", 3, 1),
        new ShopItemConfig("apple", "苹果", 10, 5)
    };

    private void Start()
    {
        // 获取核心组件
        playerInventory = FindObjectOfType<SimpleInventory>();
        merchant = FindObjectOfType<Merchant>();
        goldManager = FindObjectOfType<GoldManager>();
        // 新增：缓存背包UI引用
        inventoryUI = FindObjectOfType<InventoryUIManager>();

        // 判空日志
        if (playerInventory == null)
        {
            Debug.LogError("【ShopUIManager】未找到SimpleInventory背包脚本！");
            return;
        }
        if (goldManager == null)
        {
            Debug.LogError("【ShopUIManager】未找到GoldManager金币脚本！");
            return;
        }
        if (inventoryUI == null)
        {
            Debug.LogWarning("【ShopUIManager】未找到InventoryUIManager背包UI脚本！");
        }

        // 绑定按钮事件
        BindButtonEvents();

        // 初始化页面
        ShowMainPage();
        InitBuyItems();
    }

    /// <summary>
    /// 绑定按钮事件
    /// </summary>
    private void BindButtonEvents()
    {
        buyButton?.onClick.AddListener(ShowBuyPage);
        sellButton?.onClick.AddListener(ShowSellPage);

        // 关闭按钮
        closeButton?.onClick.AddListener(() =>
        {
            merchant?.CloseShop();
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            var player = FindObjectOfType<PlayerController>();
            if (player != null) player.isInShop = false;
        });

        // 返回按钮
        buyPageBackButton?.onClick.AddListener(ShowMainPage);
        sellPageBackButton?.onClick.AddListener(ShowMainPage);
    }

    #region 页面切换
    public void ShowMainPage()
    {
        mainPage.SetActive(true);
        buyPage.SetActive(false);
        sellPage.SetActive(false);
    }

    public void ShowBuyPage()
    {
        mainPage.SetActive(false);
        buyPage.SetActive(true);
        sellPage.SetActive(false);
        InitBuyItems();
    }

    public void ShowSellPage()
    {
        mainPage.SetActive(false);
        buyPage.SetActive(false);
        sellPage.SetActive(true);
        RefreshSellItems();
    }
    #endregion

    #region 购买逻辑（新增背包刷新）
    private void InitBuyItems()
    {
        ClearContainer(buyItemContainer);

        float currentYOffset = startYPosition;
        foreach (var shopItem in shopItems)
        {
            GameObject itemObj = Instantiate(buyItemPrefab, buyItemContainer);
            RectTransform itemRect = itemObj.GetComponent<RectTransform>();
            itemRect.anchoredPosition = new Vector2(startXPosition, -currentYOffset);

            BuyItemUI itemUI = itemObj.GetComponent<BuyItemUI>();
            if (itemUI != null)
            {
                var itemConfig = playerInventory.GetItemConfig(shopItem.itemID);
                Sprite icon = itemConfig != null ? itemConfig.icon : null;

                // 购买按钮调用（原有逻辑）
                itemUI.SetItemInfo(
                    shopItem.itemName,
                    icon,
                    shopItem.buyPrice,
                    () => BuyItem(shopItem.itemID, shopItem.buyPrice)
                );
            }

            currentYOffset += itemHeight + itemSpacing;
        }
    }

    private void BuyItem(string itemID, int buyPrice)
    {
        if (!goldManager.SpendGold(buyPrice))
        {
            Debug.LogWarning($"【ShopUIManager】购买{itemID}失败：金币不足");
            return;
        }

        string error;
        if (playerInventory.BuyItemWithGold(itemID, 1, out error))
        {
            goldManager.AddGold(-buyPrice); // 确保金币扣减（兜底）
            Debug.Log($"【ShopUIManager】购买{itemID}成功，花费{buyPrice}金币");
            InitBuyItems(); // 刷新商店购买列表

            // ========== 完全复用卖东西的刷新逻辑 ==========
            if (inventoryUI != null)
            {
                inventoryUI.RefreshInventoryUI(); // 和卖东西一样的全量刷新
                Debug.Log($"【ShopUIManager】购买{itemID}后，触发背包全量刷新");
            }
            else
            {
                // 兜底逻辑也和卖东西一致
                InventoryUIManager tempUI = FindObjectOfType<InventoryUIManager>();
                if (tempUI != null)
                {
                    tempUI.RefreshInventoryUI();
                    Debug.Log($"【ShopUIManager】兜底触发背包全量刷新：{itemID}");
                }
                else
                {
                    Debug.LogError($"【ShopUIManager】未找到InventoryUIManager，无法刷新背包");
                }
            }
        }
        else
        {
            // 购买失败，返还金币
            goldManager.AddGold(buyPrice);
            Debug.LogWarning($"【ShopUIManager】购买{itemID}失败：{error}");
        }
    }
    #endregion

    #region 出售逻辑（新增背包刷新+修复参数不匹配）
    private void RefreshSellItems()
    {
        ClearContainer(sellItemContainer);
        float currentYOffset = startYPosition;

        foreach (var inventoryItem in playerInventory.GetAllItems())
        {
            ShopItemConfig shopItem = shopItems.Find(item => item.itemID == inventoryItem.itemID);
            if (shopItem == null)
            {
                Debug.LogWarning($"【ShopUIManager】{inventoryItem.itemID} 未配置商店售价，跳过");
                continue;
            }

            GameObject itemObj = Instantiate(sellItemPrefab, sellItemContainer);
            RectTransform itemRect = itemObj.GetComponent<RectTransform>();
            itemRect.anchoredPosition = new Vector2(startXPosition, -currentYOffset);

            SellItemUI itemUI = itemObj.GetComponent<SellItemUI>();
            if (itemUI != null)
            {
                // 修复：仅传递4个参数（和BuyItemUI调用格式完全一致）
                itemUI.SetItemInfo(
                    inventoryItem.itemName,
                    inventoryItem.icon,
                    shopItem.sellPrice,
                    () => SellItem(inventoryItem.itemID, shopItem.sellPrice)
                );
            }

            currentYOffset += itemHeight + itemSpacing;
        }
    }

    private void SellItem(string itemID, int sellPrice)
    {
        int goldEarned = 0;
        if (playerInventory.SellItemForGold(itemID, 1, out goldEarned))
        {
            goldManager.AddGold(sellPrice);
            Debug.Log($"【ShopUIManager】出售{itemID}成功，获得{sellPrice}金币");
            RefreshSellItems();

            // 新增：出售后实时刷新背包UI（重新构建列表，清理空物品）
            if (inventoryUI != null)
            {
                inventoryUI.RefreshInventoryUI();
                Debug.Log($"【ShopUIManager】已触发背包全量刷新：{itemID}");
            }
            else
            {
                // 兜底：重新查找并刷新
                InventoryUIManager tempUI = FindObjectOfType<InventoryUIManager>();
                tempUI?.RefreshInventoryUI();
            }
        }
        else
        {
            Debug.LogWarning($"【ShopUIManager】出售{itemID}失败：数量不足或不存在");
        }
    }
    #endregion

    /// <summary>
    /// 清空容器
    /// </summary>
    private void ClearContainer(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child.name.Contains("BuyItem") || child.name.Contains("SellItem"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// 商店商品配置类
    /// </summary>
    [System.Serializable]
    public class ShopItemConfig
    {
        public string itemID;
        public string itemName;
        public int buyPrice;
        public int sellPrice;

        public ShopItemConfig(string id, string name, int buy, int sell)
        {
            itemID = id;
            itemName = name;
            buyPrice = buy;
            sellPrice = sell;
        }
    }
}

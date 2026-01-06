using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUIController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject buyPanel;   // 购买界面
    public GameObject sellPanel;  // 售卖界面

    // 初始化：显示购买界面，隐藏售卖界面
    void Start()
    {
        ShowBuyPanel();
    }

    // 点击“买”按钮
    public void ShowBuyPanel()
    {
        buyPanel.SetActive(true);
        sellPanel.SetActive(false);
    }

    // 点击“卖”按钮
    public void ShowSellPanel()
    {
        buyPanel.SetActive(false);
        sellPanel.SetActive(true);
    }
}

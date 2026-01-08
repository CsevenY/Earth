using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 2f; 
    public float rotationSpeed = 15f;

    [Header("组件引用")]
    private CharacterController characterController;
    private Animator animator; 

    [Header("交互设置")]
    public float interactionRange = 2f;
    public LayerMask interactableLayer;
    public LayerMask npcLayer; // 商人NPC所在Layer

    [Header("动画参数配置")]
    public string isRunAnimParam = "isRun"; 
    public string interactAnimParam = "isInteract";
    public string waterAnimParam = "isWater";

    [Header("调试开关")]
    public bool forceRunAnim = false; 

    [Header("经济交易系统")]
    public Merchant merchant; // 商人脚本引用
    public bool isInShop = false; // 改为public，供Merchant修改！

    // 原有核心变量
    private Vector3 dir;
    private bool isInteracting = false;
    private bool isWatering = false;
    void Start()
    {
        // 原有逻辑：获取核心组件
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // 原有启动日志
        Debug.Log("===== 玩家控制器启动 =====");
        Debug.Log($"Animator组件是否存在：{animator != null}");
        if (animator != null)
        {
            Debug.Log($"动画控制器：{animator.runtimeAnimatorController.name}");
            Debug.Log($"是否绑定Avatar：{animator.avatar != null}");
        }
        Debug.Log("==========================");

        // 原有逻辑：忽略作物碰撞
        IgnoreAllCropCollision();

        // 新增：自动查找场景中的商人（未手动赋值时）
        if (merchant == null)
        {
            merchant = FindObjectOfType<Merchant>();
            if (merchant != null)
            {
                Debug.Log("[交易系统] 自动找到商人：" + merchant.gameObject.name);
            }
            else
            {
                Debug.LogWarning("[交易系统] 场景中未找到Merchant脚本，请给商人模型挂载");
            }
        }
    }

   void Update()
{
    // 新增：强制兜底 - 商店UI关闭时，强制重置所有状态（解决光标消失+人物不动）
    if (merchant != null && !merchant.shopUIRoot.activeSelf)
    {
        isInShop = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        // 可根据需求调整光标状态：
        // 模式1（显示光标，适合界面操作）：
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;
        // 模式2（隐藏光标，适合纯游戏操作）：
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    // 商店打开时，只处理商店关闭逻辑，屏蔽其他操作
    if (isInShop)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
        return;
    }
    
    // 原有核心逻辑（完全保留）
    HandleMovementInput();
    HandleInteraction();
    HandleWatering();
    UpdateAnimatorState(); 
}
    void HandleWatering()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isWatering && !isInteracting)
        {
            isWatering = true;
            Debug.Log("按下Q键，尝试浇水...（无消耗）");

            // 播放浇水动画（可选）
            if (animator != null && !string.IsNullOrEmpty(waterAnimParam))
            {
                try
                {
                    animator.SetTrigger(waterAnimParam);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"播放浇水动画失败: {e.Message}");
                }
            }

            TryWaterCrop();
            Invoke("ResetWateringState", 0.2f); // 防止短时间重复浇水
        }
    }

    // 新增：重置浇水状态
    void ResetWateringState()
    {
        isWatering = false;
    }

    // 新增：尝试给附近作物浇水（无消耗）
    void TryWaterCrop()
    {
        // 检测交互范围内的作物
        Collider[] cropHits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);
        Crop nearestCrop = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in cropHits)
        {
            Crop crop = hit.GetComponent<Crop>();
            if (crop != null)
            {
                float distance = Vector3.Distance(transform.position, crop.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCrop = crop;
                }
            }
        }

        if (nearestCrop != null)
        {
            bool waterSuccess = nearestCrop.WaterCrop();
            if (waterSuccess)
            {
                Debug.Log($"成功给{nearestCrop.CropName}浇水（无消耗）");
            }
            else
            {
                Debug.Log($"给{nearestCrop.CropName}浇水失败（已成熟）");
            }
        }
        else
        {
            Debug.Log("交互范围内没有可浇水的作物");
        }
    }

    // 原有移动逻辑（完全保留）
    void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        dir = new Vector3(horizontal, 0, vertical).normalized;

        Debug.Log($"原始输入 - 水平：{horizontal} | 垂直：{vertical} | 合成方向：{dir}");

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
            transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    // 扩展交互逻辑：先检测商人交互，再执行原有种植交互
    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            isInteracting = true;
            
            // 优先检测商人交互
            bool isMerchantInteract = TryInteractWithMerchant();
            if (isMerchantInteract)
            {
                Debug.Log("[交易系统] 打开商店界面");
            }
            else
            {
                // 原有种植交互动画逻辑
                if (animator != null && !string.IsNullOrEmpty(interactAnimParam))
                {
                    try
                    {
                        animator.SetTrigger(interactAnimParam);
                        Debug.Log("[交互调试] 触发交互动画！");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[交互调试] 交互动画触发失败：{e.Message}");
                    }
                }
                // 原有种植交互逻辑
                TryInteract();
            }
            
            Invoke("ResetInteraction", 0.2f);
        }

        // 原有空格通用动作逻辑（保留）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryAction();
        }
    }

    // 原有动画状态更新逻辑（完全保留）
    void UpdateAnimatorState()
    {
        if (animator == null) 
        {
            Debug.LogError("[动画调试] Animator组件不存在！请给玩家添加Animator组件！");
            return;
        }

        bool shouldRun = dir != Vector3.zero;
        if (forceRunAnim)
        {
            shouldRun = true;
            Debug.LogWarning("[动画调试] 强制触发跑步动画！");
        }

        bool currentIsRun = animator.GetBool(isRunAnimParam);
        Debug.Log($"[动画调试] 输入是否移动：{dir != Vector3.zero} | 要设置的isRun：{shouldRun} | 控制器当前isRun：{currentIsRun}");

        try
        {
            animator.SetBool(isRunAnimParam, shouldRun);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[动画调试] 设置参数失败！原因：{e.Message} | 检查参数名是否为：{isRunAnimParam}");
        }
    }

    // 商人交互检测逻辑
    bool TryInteractWithMerchant()
    {
        if (merchant == null) return false;

        // 检测交互范围内的商人NPC
        Collider[] npcHits = Physics.OverlapSphere(transform.position, interactionRange, npcLayer);
        foreach (Collider hit in npcHits)
        {
            Merchant hitMerchant = hit.GetComponent<Merchant>();
            if (hitMerchant != null)
            {
                merchant = hitMerchant; // 绑定当前靠近的商人
                OpenShop(); // 打开商店
                return true;
            }
        }
        return false;
    }

    // 打开商店
    void OpenShop()
    {
        if (merchant != null && merchant.shopUIRoot != null)
        {
            merchant.ToggleShopUI(); // 调用商人的Toggle方法
            isInShop = true;
            Time.timeScale = 0f; // 暂停游戏
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            Cursor.lockState = CursorLockMode.None; 
            Cursor.visible = true;
            Debug.Log("[交易系统] 商店已打开，游戏暂停");
        }
        else
        {
            Debug.LogError("[交易系统] 商店UI未绑定！请给Merchant脚本赋值shopUIRoot");
        }
    }

    // 关闭商店
    void CloseShop()
    {
        if (merchant != null && merchant.shopUIRoot != null)
        {
            merchant.CloseShop(); // 调用商人的Close方法
            isInShop = false;
            Time.timeScale = 1f; // 强制恢复游戏
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("[交易系统] 商店已关闭，游戏恢复");
        }
    }

    // 强化版：商店关闭回调（双重保险恢复状态）
    public void OnShopClosed()
    {
        isInShop = false;
        // 强制恢复时间缩放，避免遗漏
        if (Time.timeScale != 1f)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
        // 强制恢复光标状态
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("[玩家控制器] 商店关闭，所有输入/移动状态恢复");
    }

    // 原有种植交互逻辑（完全保留）
    void TryInteract()
    {
        Debug.Log("开始交互检测...");
        Collider[] farmHits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);
        FarmPlot nearestPlot = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in farmHits)
        {
            FarmPlot farmPlot = hit.GetComponent<FarmPlot>();
            if (farmPlot != null)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPlot = farmPlot;
                }
            }
        }

        if (nearestPlot != null)
        {
            Debug.Log($"与最近耕地交互：{nearestPlot.gameObject.name}，距离：{nearestDistance:F2}");
            nearestPlot.TryPlantCrop();
        }
        else
        {
            Debug.Log("附近没有可交互的耕地");
        }
    }

    // 原有通用动作逻辑（完全保留）
    void TryAction()
    {
        Debug.Log("执行通用动作（如跳跃/使用工具）");
    }

    // 重置交互状态
    void ResetInteraction()
    {
        isInteracting = false;
    }

    // 绘制调试范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        if (Application.isPlaying && dir != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + dir * 3f);
        }
    }

    // 忽略作物碰撞
    private void IgnoreAllCropCollision()
    {
        GameObject[] allCrops = GameObject.FindGameObjectsWithTag("Crop");
        foreach (GameObject crop in allCrops)
        {
            Collider cropCollider = crop.GetComponent<Collider>();
            if (cropCollider != null && GetComponent<Collider>() != null)
            {
                Physics.IgnoreCollision(cropCollider, GetComponent<Collider>(), true);
                Debug.Log($"已忽略作物碰撞：{crop.name}");
            }
        }
    }

    // 动态忽略作物碰撞
    void LateUpdate()
    {
        IgnoreAllCropCollision();
    }
}
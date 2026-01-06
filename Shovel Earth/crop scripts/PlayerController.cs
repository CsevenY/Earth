using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 2f; // 和成功版一致的移动速度
    public float rotationSpeed = 15f;

    [Header("组件引用")]
    private CharacterController characterController;
    private Animator animator; 

    [Header("交互设置")]
    public float interactionRange = 2f;
    public LayerMask interactableLayer;

    [Header("动画参数配置")]
    public string isRunAnimParam = "isRun"; // 必须和动画控制器参数名一致
    public string interactAnimParam = "isInteract"; 

    [Header("调试开关")] // 新增：调试用开关
    public bool forceRunAnim = false; // 强制触发跑步动画（测试用）

    [Header("浇水设置")]
    public KeyCode wateringKey = KeyCode.Q;
    public string wateringAnimParam = "isWatering"; // 浇水动画参数名
    public float wateringRange = 2f; // 浇水范围
    public GameObject wateringTool; // 浇水工具（如水壶）模型

    // 和成功版一致的核心变量
    private Vector3 dir;
    private bool isInteracting = false; 

    void Start()
    {
        // 成功版逻辑：只获取，不强制报错
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // 启动日志：检查核心组件
        Debug.Log("===== 玩家控制器启动 =====");
        Debug.Log($"Animator组件是否存在：{animator != null}");
        if (animator != null)
        {
            Debug.Log($"动画控制器：{animator.runtimeAnimatorController.name}");
            Debug.Log($"是否绑定Avatar：{animator.avatar != null}");
        }
        Debug.Log("==========================");

        // 新增：游戏启动时就忽略所有作物的碰撞（永久穿模）
        IgnoreAllCropCollision();
    }

    void Update()
    {
        // 复刻成功版的核心逻辑
        HandleMovementInput();
        HandleInteraction(); 
        UpdateAnimatorState(); 
    }

    // 完全复刻成功版的移动输入逻辑（核心改这里去惯性）
    void HandleMovementInput()
    {
        // 1. 改用GetAxisRaw：无输入缓动，瞬时响应（彻底去惯性）
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        // 2. 归一化：避免斜向移动速度翻倍，输入更精准
        dir = new Vector3(horizontal, 0, vertical).normalized;

        // 调试日志：打印原始输入值
        Debug.Log($"原始输入 - 水平：{horizontal} | 垂直：{vertical} | 合成方向：{dir}");

        if (dir != Vector3.zero)
        {
            // 成功版的旋转逻辑
            transform.rotation = Quaternion.LookRotation(dir);
            // 3. 改用世界坐标系移动：方向精准，无自身坐标系偏移惯性
            transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);
        }
        // 4. 松键后无任何残留移动：dir=Vector3.zero，彻底停稳
    }

    // 带完整调试的动画触发逻辑
    void UpdateAnimatorState()
    {
        if (animator == null) 
        {
            Debug.LogError("[动画调试] Animator组件不存在！请给玩家添加Animator组件！");
            return;
        }

        // 1. 确定是否要播放跑步动画
        bool shouldRun = dir != Vector3.zero;
        // 调试开关：强制触发跑步动画（忽略输入）
        if (forceRunAnim)
        {
            shouldRun = true;
            Debug.LogWarning("[动画调试] 强制触发跑步动画！");
        }

        // 2. 获取当前动画控制器的isRun值（验证是否传参成功）
        bool currentIsRun = animator.GetBool(isRunAnimParam);

        // 3. 打印完整调试日志（每帧更新）
        Debug.Log($"[动画调试] 输入是否移动：{dir != Vector3.zero} | 要设置的isRun：{shouldRun} | 控制器当前isRun：{currentIsRun}");

        // 4. 处理参数名错误的情况
        try
        {
            animator.SetBool(isRunAnimParam, shouldRun);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[动画调试] 设置参数失败！原因：{e.Message} | 检查参数名是否为：{isRunAnimParam}");
        }
    }

    // 保留所有交互功能（不影响动画）
    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            isInteracting = true;
            // 交互动画（可选）
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
            TryInteract();
            Invoke("ResetInteraction", 0.2f);
        }
        // 新增：浇水交互
        if (Input.GetKeyDown(wateringKey) && !isInteracting)
        {
            isInteracting = true;
            // 触发浇水动画

            if (animator != null && !string.IsNullOrEmpty(wateringAnimParam))
            {
                try
                {
                    animator.SetTrigger(wateringAnimParam);
                    Debug.Log("[浇水调试] 触发浇水动画！");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[浇水调试] 浇水动画触发失败：{e.Message}");
                }
            }

            // 显示浇水工具（如果有）
            if (wateringTool != null)
            {
                StartCoroutine(ShowWateringTool());
            }

            TryWater();
            Invoke("ResetInteraction", 3f); 
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryAction();
        }
    }
    private IEnumerator ShowWateringTool()
    {
        wateringTool.SetActive(true);
        yield return new WaitForSeconds(3f); 
        wateringTool.SetActive(false);
    }

    // 新增：浇水方法
    void TryWater()
    {
        Debug.Log("开始浇水检测...");

        // 检测范围内的耕地
        Collider[] farmHits = Physics.OverlapSphere(transform.position, wateringRange, interactableLayer);
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
            Debug.Log($"为最近耕地浇水：{nearestPlot.gameObject.name}，距离：{nearestDistance:F2}");
            // 调用耕地的浇水方法
            nearestPlot.TryWaterCrop();
        }
        else
        {
            Debug.Log("附近没有可浇水的耕地");
        }
    }
    void ResetInteraction()
    {
        isInteracting = false;
    }

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

    void TryAction()
    {
        Debug.Log("执行通用动作（如跳跃/使用工具）");
    }

    // 绘制交互范围（调试用）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wateringRange);
        if (Application.isPlaying && dir != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + dir * 3f);
        }
    }

    // 新增：永久忽略所有作物的碰撞（允许穿模，不被顶飞）
    private void IgnoreAllCropCollision()
    {
        // 找到场景中所有标签为Crop的对象
        GameObject[] allCrops = GameObject.FindGameObjectsWithTag("Crop");
        foreach (GameObject crop in allCrops)
        {
            Collider cropCollider = crop.GetComponent<Collider>();
            if (cropCollider != null && GetComponent<Collider>() != null)
            {
                // 永久忽略角色和作物的碰撞（允许穿模）
                Physics.IgnoreCollision(cropCollider, GetComponent<Collider>(), true);
                Debug.Log($"已忽略作物碰撞：{crop.name}");
            }
        }
    }

    // 可选：如果有动态生成的作物，在Update中检测并忽略碰撞
    void LateUpdate()
    {
        IgnoreAllCropCollision();
    }
}
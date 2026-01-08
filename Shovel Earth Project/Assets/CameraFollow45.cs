using UnityEngine;

public class CameraFollow45 : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;          // 跟随的目标（玩家）

    [Header("跟随设置")]
    public Vector3 offset = new Vector3(0, 15, -15);  // 45度视角偏移
    public float smoothSpeed = 5f;    // 跟随平滑度
    public bool followRotation = false; // 是否跟随旋转

    [Header("边界限制")]
    public bool useBounds = false;
    public Vector2 xBounds = new Vector2(-50, 50);
    public Vector2 zBounds = new Vector2(-50, 50);

    private Vector3 desiredPosition;

    void Start()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // 设置初始位置
        if (target != null)
        {
            transform.position = target.position + offset;
            if (followRotation)
            {
                transform.rotation = Quaternion.Euler(45, target.eulerAngles.y, 0);
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置
        desiredPosition = target.position + offset;

        // 应用边界限制
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, xBounds.x, xBounds.y);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, zBounds.x, zBounds.y);
        }

        // 平滑移动
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // 如果需要，跟随旋转
        if (followRotation)
        {
            Quaternion targetRotation = Quaternion.Euler(45, target.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
        }
    }

    // 手动设置跟随目标
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // 手动更新相机位置（立即跳转）
    public void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    // 绘制边界Gizmos（调试用）
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((xBounds.x + xBounds.y) / 2, 0, (zBounds.x + zBounds.y) / 2);
            Vector3 size = new Vector3(xBounds.y - xBounds.x, 0.1f, zBounds.y - zBounds.x);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
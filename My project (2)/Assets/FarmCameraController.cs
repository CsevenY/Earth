using UnityEngine;

public class FarmCameraController : MonoBehaviour
{
    [Header("相机控制")]
    public float moveSpeed = 15f;
    public float zoomSpeed = 30f;
    public float rotationSpeed = 80f;

    [Header("边界限制")]
    public Vector2 xLimit = new Vector2(-60, 60);
    public Vector2 zLimit = new Vector2(-60, 60);
    public Vector2 heightLimit = new Vector2(10, 50);

    [Header("初始设置")]
    public float startHeight = 25f;
    public float startRotationX = 45f;
    public float startRotationY = 45f;  // 添加Y轴旋转

    private Vector3 cameraPosition;
    private float currentHeight;
    private float currentRotationX;
    private float currentRotationY;

    private void Start()
    {
        // 设置初始位置和角度
        cameraPosition = Vector3.zero;
        currentHeight = startHeight;
        currentRotationX = startRotationX;
        currentRotationY = startRotationY;

        UpdateCameraTransform();
    }

    private void Update()
    {
        HandleCameraMovement();
        HandleCameraZoom();
        HandleCameraRotation();

        // 按R键重置相机
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCamera();
        }
    }

    private void HandleCameraMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        // 基于当前相机朝向计算移动方向
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveDirection += GetCameraForward();
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveDirection += GetCameraBack();
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveDirection += GetCameraLeft();
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveDirection += GetCameraRight();

        // 鼠标拖拽移动（按住鼠标中键）
        if (Input.GetMouseButton(2)) // 中键
        {
            moveDirection += new Vector3(-Input.GetAxis("Mouse X"), 0, -Input.GetAxis("Mouse Y")) * 3f;
        }

        // 应用移动
        if (moveDirection != Vector3.zero)
        {
            cameraPosition += moveDirection.normalized * moveSpeed * Time.deltaTime;
            cameraPosition.x = Mathf.Clamp(cameraPosition.x, xLimit.x, xLimit.y);
            cameraPosition.z = Mathf.Clamp(cameraPosition.z, zLimit.x, zLimit.y);

            UpdateCameraTransform();
        }
    }

    // 获取相机方向向量（忽略Y轴）
    private Vector3 GetCameraForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetCameraBack()
    {
        return -GetCameraForward();
    }

    private Vector3 GetCameraRight()
    {
        Vector3 right = transform.right;
        right.y = 0;
        return right.normalized;
    }

    private Vector3 GetCameraLeft()
    {
        return -GetCameraRight();
    }

    private void HandleCameraZoom()
    {
        float zoom = Input.GetAxis("Mouse ScrollWheel");

        if (zoom != 0)
        {
            currentHeight -= zoom * zoomSpeed;
            currentHeight = Mathf.Clamp(currentHeight, heightLimit.x, heightLimit.y);

            UpdateCameraTransform();
        }
    }

    private void HandleCameraRotation()
    {
        // 按住鼠标右键旋转视角
        if (Input.GetMouseButton(1))
        {
            currentRotationY += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            currentRotationX -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            currentRotationX = Mathf.Clamp(currentRotationX, 20, 80);

            UpdateCameraTransform();
        }
    }

    private void UpdateCameraTransform()
    {
        // 计算相机位置
        Vector3 targetPosition = cameraPosition;
        targetPosition.y = currentHeight;

        // 计算相机旋转
        Quaternion targetRotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);

        // 应用变换
        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    // 重置相机
    public void ResetCamera()
    {
        cameraPosition = Vector3.zero;
        currentHeight = startHeight;
        currentRotationX = startRotationX;
        currentRotationY = startRotationY;
        UpdateCameraTransform();
    }

    // 聚焦到指定位置
    public void FocusOnPosition(Vector3 position)
    {
        cameraPosition = new Vector3(position.x, 0, position.z);
        UpdateCameraTransform();
    }

    // 在场景中绘制Gizmos（调试用）
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + GetCameraForward() * 5f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + GetCameraRight() * 5f);
    }
}
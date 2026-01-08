
using UnityEngine;


public class UltraSimpleDayNight : MonoBehaviour
{
    [Header("=== 基础设置 ===")]
    [Tooltip("光照旋转速度，值越大时间过得越快")]
    public float cycleSpeed = 0.1f;

    [Header("=== 颜色设置 ===")]
    [Tooltip("白天的光照颜色（暖白色）")]
    public Color dayColor = new Color(1f, 0.95f, 0.9f, 1f);

    [Tooltip("夜晚的光照颜色（深蓝色）")]
    public Color nightColor = new Color(0.1f, 0.1f, 0.3f, 1f);

    [Tooltip("日出时的光照颜色（橙红色）")]
    public Color sunriseColor = new Color(1f, 0.5f, 0.2f, 1f);

    [Tooltip("日落时的光照颜色（红色）")]
    public Color sunsetColor = new Color(1f, 0.3f, 0.1f, 1f);

    [Header("=== 强度设置 ===")]
    [Tooltip("白天光照强度")]
    [Range(0f, 2f)]
    public float dayIntensity = 1f;

    [Tooltip("夜晚光照强度")]
    [Range(0f, 1f)]
    public float nightIntensity = 0.1f;

    [Tooltip("日出日落光照强度")]
    [Range(0f, 2f)]
    public float sunriseIntensity = 0.8f;

    [Header("=== 时间显示设置 ===")]
    [Tooltip("是否在屏幕上显示时间")]
    public bool showOnScreen = true;

    [Tooltip("时间显示的位置")]
    public TimeDisplayPosition displayPosition = TimeDisplayPosition.TopLeft;

    [Tooltip("字体大小")]
    public int fontSize = 24;

    [Tooltip("字体颜色")]
    public Color fontColor = Color.white;

    [Tooltip("是否显示时间段（清晨/白天/傍晚/夜晚）")]
    public bool showTimePeriod = true;

    [Header("=== 调试设置 ===")]
    [Tooltip("是否在控制台输出时间")]
    public bool logToConsole = false;

    [Tooltip("控制台输出间隔（秒）")]
    public float logInterval = 5f;

    private Light dirLight;
    private float rotationAngle = 0f;
    private GUIStyle timeStyle;
    private float lastLogTime = 0f;

    // 当前时间段
    private string currentPeriod = "白天";

    // 枚举：显示位置
    public enum TimeDisplayPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        CenterTop
    }

    void Start()
    {
        // 获取或添加Light组件
        dirLight = GetComponent<Light>();
        if (dirLight == null)
        {
            dirLight = gameObject.AddComponent<Light>();
            Debug.Log("已自动添加Light组件到物体上");
        }

        // 确保是方向光
        dirLight.type = LightType.Directional;

        // 初始设置
        dirLight.shadows = LightShadows.Soft;
        dirLight.shadowStrength = 1f;

        // 初始化GUI样式
        timeStyle = new GUIStyle();
        timeStyle.fontSize = fontSize;
        timeStyle.normal.textColor = fontColor;
        timeStyle.alignment = TextAnchor.UpperLeft;

        Debug.Log($"昼夜系统已启动，初始时间: {GetTimeFromAngle(rotationAngle)}");
    }

    void Update()
    {
        // 1. 更新光照角度（模拟太阳/月亮运动）
        rotationAngle += Time.deltaTime * cycleSpeed;
        if (rotationAngle > 360f) rotationAngle -= 360f;

        // 2. 更新光照方向
        transform.rotation = Quaternion.Euler(rotationAngle, -30f, 0f);

        // 3. 更新光照属性
        UpdateLightProperties(rotationAngle);

        // 4. 更新当前时间段
        UpdateTimePeriod();

        // 5. 控制台输出
        if (logToConsole && Time.time - lastLogTime > logInterval)
        {
            Debug.Log($"游戏时间: {GetTimeFromAngle(rotationAngle)} - {currentPeriod}");
            lastLogTime = Time.time;
        }
    }

    void UpdateLightProperties(float angle)
    {
        // 确保角度在0-360范围内
        angle = Mathf.Repeat(angle, 360f);

        // 根据不同时间段混合颜色和强度
        if (angle >= 0f && angle < 90f) // 日出到正午 (6:00-12:00)
        {
            float t = angle / 90f;
            dirLight.color = Color.Lerp(sunriseColor, dayColor, t);
            dirLight.intensity = Mathf.Lerp(sunriseIntensity, dayIntensity, t);
        }
        else if (angle >= 90f && angle < 180f) // 正午到日落 (12:00-18:00)
        {
            float t = (angle - 90f) / 90f;
            dirLight.color = Color.Lerp(dayColor, sunsetColor, t);
            dirLight.intensity = Mathf.Lerp(dayIntensity, sunriseIntensity, t);
        }
        else if (angle >= 180f && angle < 270f) // 日落到午夜 (18:00-0:00)
        {
            float t = (angle - 180f) / 90f;
            dirLight.color = Color.Lerp(sunsetColor, nightColor, t);
            dirLight.intensity = Mathf.Lerp(sunriseIntensity, nightIntensity, t);
        }
        else // 午夜到日出 (0:00-6:00)
        {
            float t = (angle - 270f) / 90f;
            dirLight.color = Color.Lerp(nightColor, sunriseColor, t);
            dirLight.intensity = Mathf.Lerp(nightIntensity, sunriseIntensity, t);
        }

        // 根据时间调整阴影
        if (angle > 180f && angle < 360f) // 夜晚 (18:00-6:00)
        {
            dirLight.shadows = LightShadows.None;
        }
        else // 白天 (6:00-18:00)
        {
            dirLight.shadows = LightShadows.Soft;
            dirLight.shadowStrength = 1f;
        }
    }

    void UpdateTimePeriod()
    {
        if (rotationAngle >= 0 && rotationAngle < 90)
            currentPeriod = "清晨";
        else if (rotationAngle >= 90 && rotationAngle < 180)
            currentPeriod = "白天";
        else if (rotationAngle >= 180 && rotationAngle < 270)
            currentPeriod = "傍晚";
        else
            currentPeriod = "夜晚";
    }

    void OnGUI()
    {
        // 显示时间
        if (showOnScreen)
        {
            // 根据设置的位置计算显示区域
            Rect displayRect = GetDisplayRect();

            // 设置对齐方式
            timeStyle.alignment = GetTextAlignment();

            // 显示时间
            string timeString = GetTimeFromAngle(rotationAngle);
            string displayText = $"时间: {timeString}";

            if (showTimePeriod)
            {
                displayText += $"\n{currentPeriod}";
            }

            GUI.Label(displayRect, displayText, timeStyle);
        }

        // 键盘快捷键控制（仅在Play模式下有效）
        if (Event.current != null && Event.current.type == EventType.KeyDown && Application.isPlaying)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.F1:
                    SetToDay();
                    break;
                case KeyCode.F2:
                    SetToNight();
                    break;
                case KeyCode.F3:
                    SetToSunrise();
                    break;
                case KeyCode.F4:
                    SetToSunset();
                    break;
                case KeyCode.UpArrow:
                    SetCycleSpeed(cycleSpeed + 0.05f);
                    break;
                case KeyCode.DownArrow:
                    SetCycleSpeed(Mathf.Max(0.01f, cycleSpeed - 0.05f));
                    break;
            }
        }
    }

    Rect GetDisplayRect()
    {
        int margin = 10;
        int width = 150;
        int height = showTimePeriod ? 60 : 30;

        switch (displayPosition)
        {
            case TimeDisplayPosition.TopLeft:
                return new Rect(margin, margin, width, height);

            case TimeDisplayPosition.TopRight:
                return new Rect(Screen.width - width - margin, margin, width, height);

            case TimeDisplayPosition.BottomLeft:
                return new Rect(margin, Screen.height - height - margin, width, height);

            case TimeDisplayPosition.BottomRight:
                return new Rect(Screen.width - width - margin, Screen.height - height - margin, width, height);

            case TimeDisplayPosition.CenterTop:
                return new Rect(Screen.width / 2 - width / 2, margin, width, height);

            default:
                return new Rect(margin, margin, width, height);
        }
    }

    TextAnchor GetTextAlignment()
    {
        switch (displayPosition)
        {
            case TimeDisplayPosition.TopLeft:
            case TimeDisplayPosition.BottomLeft:
                return TextAnchor.UpperLeft;

            case TimeDisplayPosition.TopRight:
            case TimeDisplayPosition.BottomRight:
                return TextAnchor.UpperRight;

            case TimeDisplayPosition.CenterTop:
                return TextAnchor.UpperCenter;

            default:
                return TextAnchor.UpperLeft;
        }
    }

    // 根据角度获取时间字符串
    public string GetTimeFromAngle(float angle)
    {
        // 角度转换为24小时制时间
        // 0° = 6:00（日出），90° = 12:00，180° = 18:00（日落），270° = 0:00
        float hour = (angle / 360f) * 24f + 6f;
        if (hour >= 24f) hour -= 24f;

        int h = Mathf.FloorToInt(hour);
        int m = Mathf.FloorToInt((hour - h) * 60f);

        return $"{h:D2}:{m:D2}";
    }

    #region 公共API - 控制昼夜系统

    /// <summary>
    /// 设置到指定时间
    /// </summary>
    /// <param name="hour">小时 (0-24)</param>
    public void SetTime(float hour)
    {
        // 将小时转换为角度
        // 6:00 = 0°, 12:00 = 90°, 18:00 = 180°, 0:00 = 270°
        hour = Mathf.Repeat(hour, 24f);
        rotationAngle = ((hour - 6f) / 24f) * 360f;
        if (rotationAngle < 0) rotationAngle += 360f;

        UpdateLightProperties(rotationAngle);
        UpdateTimePeriod();
        Debug.Log($"已设置时间到: {GetTimeFromAngle(rotationAngle)}");
    }

    /// <summary>
    /// 设置为白天（正午12:00）
    /// </summary>
    public void SetToDay()
    {
        rotationAngle = 90f; // 正午
        UpdateLightProperties(rotationAngle);
        UpdateTimePeriod();
        Debug.Log("已设置为白天（12:00）");
    }

    /// <summary>
    /// 设置为夜晚（午夜0:00）
    /// </summary>
    public void SetToNight()
    {
        rotationAngle = 270f; // 午夜
        UpdateLightProperties(rotationAngle);
        UpdateTimePeriod();
        Debug.Log("已设置为夜晚（0:00）");
    }

    /// <summary>
    /// 设置为日出（6:00）
    /// </summary>
    public void SetToSunrise()
    {
        rotationAngle = 0f; // 日出
        UpdateLightProperties(rotationAngle);
        UpdateTimePeriod();
        Debug.Log("已设置为日出（6:00）");
    }

    /// <summary>
    /// 设置为日落（18:00）
    /// </summary>
    public void SetToSunset()
    {
        rotationAngle = 180f; // 日落
        UpdateLightProperties(rotationAngle);
        UpdateTimePeriod();
        Debug.Log("已设置为日落（18:00）");
    }

    /// <summary>
    /// 设置时间流逝速度
    /// </summary>
    /// <param name="speed">速度值（默认0.1）</param>
    public void SetCycleSpeed(float speed)
    {
        cycleSpeed = Mathf.Max(0.001f, speed);
        Debug.Log($"时间流逝速度设置为: {cycleSpeed}");
    }

    /// <summary>
    /// 获取当前时间字符串
    /// </summary>
    /// <returns>格式化的时间字符串 (HH:mm)</returns>
    public string GetCurrentTimeString()
    {
        return GetTimeFromAngle(rotationAngle);
    }

    /// <summary>
    /// 获取当前小时数
    /// </summary>
    /// <returns>当前小时 (0-24)</returns>
    public float GetCurrentHour()
    {
        float hour = (rotationAngle / 360f) * 24f + 6f;
        if (hour >= 24f) hour -= 24f;
        return hour;
    }

    /// <summary>
    /// 获取当前时间段
    /// </summary>
    /// <returns>时间段字符串</returns>
    public string GetCurrentPeriod()
    {
        return currentPeriod;
    }

    /// <summary>
    /// 判断当前是否是白天
    /// </summary>
    /// <returns>如果是白天返回true</returns>
    public bool IsDaytime()
    {
        return rotationAngle >= 0 && rotationAngle < 180;
    }

    /// <summary>
    /// 判断当前是否是夜晚
    /// </summary>
    /// <returns>如果是夜晚返回true</returns>
    public bool IsNighttime()
    {
        return !IsDaytime();
    }

    #endregion

    #region 调试方法

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // 在Scene视图中显示当前时间
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;

        string timeText = $"时间: {GetTimeFromAngle(rotationAngle)}\n{currentPeriod}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, timeText, style);
    }

    #endregion
}
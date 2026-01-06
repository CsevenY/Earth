using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [Header("交互设置")]
    public string objectName = "木箱";
    public Color highlightColor = Color.yellow;
    
    private Material originalMaterial;
    private Renderer objectRenderer;
    
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
    }
    
    public void Interact()
    {
        Debug.Log($"你与 {objectName} 交互了！");
        
        // 简单的视觉反馈
        if (objectRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }
        
        // 可以在这里添加其他交互逻辑
        // 如：打开箱子、拾取物品等
    }
    
    System.Collections.IEnumerator FlashEffect()
    {
        if (objectRenderer != null)
        {
            // 存储原始颜色
            Color originalColor = objectRenderer.material.color;
            
            // 变黄
            objectRenderer.material.color = highlightColor;
            yield return new WaitForSeconds(0.3f);
            
            // 恢复原色
            objectRenderer.material.color = originalColor;
        }
    }
    
    // 鼠标悬停时高亮
    void OnMouseEnter()
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = Color.Lerp(originalMaterial.color, highlightColor, 0.3f);
        }
    }
    
    void OnMouseExit()
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalMaterial.color;
        }
    }
}
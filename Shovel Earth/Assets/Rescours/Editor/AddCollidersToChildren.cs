using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 必须放在Editor文件夹下！
public class AddCollidersTool : EditorWindow
{
    [MenuItem("Tools/批量添加碰撞体", false, 100)] // 优先级100，确保在Tools菜单最上方
    static void ShowAddColliderWindow()
    {
        AddCollidersTool window = GetWindow<AddCollidersTool>("批量加碰撞体");
        window.minSize = new Vector2(300, 150); // 固定窗口大小，适配2022.3
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("操作说明：先选中父物体，再点击按钮", EditorStyles.label);
        GUILayout.Space(10);

        // 按钮1：添加Box Collider（包含父物体+所有子物体）
        if (GUILayout.Button("添加Box Collider（所有子物体）", GUILayout.Height(40)))
        {
            AddColliderToAll<BoxCollider>();
        }

        GUILayout.Space(5);

        // 按钮2：添加Mesh Collider（默认凸面体）
        if (GUILayout.Button("添加Mesh Collider（凸面体）", GUILayout.Height(40)))
        {
            AddColliderToAll<MeshCollider>();
        }
    }

    /// <summary>
    /// 批量添加指定类型碰撞体
    /// </summary>
    /// <typeparam name="T">碰撞体类型（BoxCollider/MeshCollider）</typeparam>
    private void AddColliderToAll<T>() where T : Collider
    {
        // 检查是否选中物体
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("提示", "请先在层级面板选中父物体！", "确定");
            return;
        }

        GameObject parentObj = Selection.activeGameObject;
        // 获取所有子物体（包括父物体，true表示包含非激活物体）
        Transform[] allTrans = parentObj.GetComponentsInChildren<Transform>(true);
        int addedCount = 0;

        foreach (Transform trans in allTrans)
        {
            // 跳过已存在该碰撞体的物体
            if (trans.GetComponent<T>() != null) continue;

            // 添加碰撞体
            T collider = trans.gameObject.AddComponent<T>();
            // MeshCollider默认勾选凸面体（否则无法用于刚体）
            if (collider is MeshCollider meshCol)
            {
                meshCol.convex = true;
                meshCol.isTrigger = false; // 确保不是触发器（根据需求调整）
            }
            addedCount++;
        }

        // 提示完成
        EditorUtility.DisplayDialog(
            "操作完成",
            $"已为【{parentObj.name}】及其子物体添加 {typeof(T).Name} 共 {addedCount} 个！",
            "确定"
        );
    }
}

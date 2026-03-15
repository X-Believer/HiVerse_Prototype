using UnityEngine;
using UnityEditor;
using TMPro;

public class ReplaceTMPFont : MonoBehaviour
{
    [MenuItem("Tools/Replace All TMP Fonts")]
    static void ReplaceAllTMPFonts()
    {
        // 找到项目中的所有 TextMeshProUGUI 组件
        TMP_Text[] allTMPTexts = GameObject.FindObjectsOfType<TMP_Text>(true);

        // 加载你刚才生成的微软雅黑 Font Asset
        TMP_FontAsset newFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/msyh SDF.asset" // 修改成你实际路径
        );

        if (newFont == null)
        {
            Debug.LogError("找不到微软雅黑 Font Asset，请确认路径正确");
            return;
        }

        int count = 0;
        foreach (TMP_Text tmp in allTMPTexts)
        {
            Undo.RecordObject(tmp, "Replace TMP Font");
            tmp.font = newFont;
            count++;
        }

        Debug.Log($"替换完成，共替换 {count} 个 TextMeshPro 组件的字体");
    }
}
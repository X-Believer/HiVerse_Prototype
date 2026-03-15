using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameDataInitializer : MonoBehaviour
{
    [Header("Editor 设置")]
    [Tooltip("是否加载数据，如果取消勾选，将跳过初始化流程，直接使用 persistentDataPath 的数据")]
    [SerializeField]
    private bool loadData = true;

    [Tooltip("是否从服务器覆盖本地数据（PersistentData），勾选后会重新下载覆盖")]
    [SerializeField]
    private bool overwriteFromServer = true;

    void Start()
    {
        if (!loadData)
        {
            Debug.Log("跳过数据初始化，使用 persistentDataPath 已存在的数据");
            return;
        }

        StartCoroutine(InitializeGameData());
        Debug.Log(Application.persistentDataPath);
    }

    IEnumerator InitializeGameData()
    {
        Debug.Log("开始初始化游戏数据");

        if (!overwriteFromServer)
        {
            Debug.Log("保留本地数据，不从服务器覆盖");
            yield break; // 直接使用 persistentDataPath 里的数据
        }

        bool pingFinished = false;
        bool serverAvailable = false;

        // 尝试连接服务器
        ServerAPI.Instance.PostJson(
            ServerAPI.Instance.serverURL + "api/ping",
            new { },
            (success, response) =>
            {
                serverAvailable = success;
                pingFinished = true;
            });

        while (!pingFinished)
            yield return null;

        if (!serverAvailable)
        {
            Debug.LogWarning("服务器不可用，拷贝 StreamingAssets 测试数据");
            CopyDirectory(Application.streamingAssetsPath, Application.persistentDataPath);
            yield break;
        }

        Debug.Log("服务器连接成功");

        // 下载 CityEvents
        bool cityEventDone = false;
        ServerAPI.Instance.GetCityEvents("2026-01-01", (success, paths) =>
        {
            cityEventDone = true;
            if (success) Debug.Log("CityEvents 下载完成");
            else Debug.LogError("CityEvents 下载失败");
        });

        while (!cityEventDone)
            yield return null;

        // 下载最新 NPC 人格
        bool personalityDone = false;
        List<int> personalityIds = new List<int>();
        Dictionary<int, string> personalityPaths = new Dictionary<int, string>();
        ServerAPI.Instance.GetLatestPersonalities(10, (success, paths, ids) =>
        {
            personalityDone = true;
            if (success)
            {
                personalityIds = ids;
                personalityPaths = paths;
                Debug.Log("获取数字人格成功: " + ids.Count);
            }
            else
            {
                Debug.LogError("获取数字人格失败");
            }
        });

        while (!personalityDone)
            yield return null;

        // 下载 NPC Schedule
        bool scheduleDone = false;
        Dictionary<string, string> npcSchedulePaths = null;
        ServerAPI.Instance.GetAllNPCSchedules(personalityIds, (success, paths) =>
        {
            scheduleDone = true;
            if (success)
            {
                npcSchedulePaths = paths;
                Debug.Log("NPC Schedule 下载完成");
                foreach (var kvp in npcSchedulePaths)
                    Debug.Log($"NPC {kvp.Key} -> {kvp.Value}");
            }
            else
            {
                Debug.LogError("NPC Schedule 下载失败");
            }
        });

        while (!scheduleDone)
            yield return null;

        Debug.Log("游戏数据初始化完成");
    }

    void CopyDirectory(string source, string destination)
    {
        if (!Directory.Exists(destination))
            Directory.CreateDirectory(destination);

        foreach (string file in Directory.GetFiles(source))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destination, fileName);

            if (!File.Exists(destFile))
                File.Copy(file, destFile, true);
        }

        foreach (string dir in Directory.GetDirectories(source))
        {
            string dirName = Path.GetFileName(dir);
            string destDir = Path.Combine(destination, dirName);
            CopyDirectory(dir, destDir);
        }
    }
}
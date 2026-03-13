using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ServerAPI : MonoBehaviour
{
    public static ServerAPI Instance;
    public string serverURL = "http://127.0.0.1:5000/";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 发送POST JSON请求
    /// </summary>
    public void PostJson(string url, object data, Action<bool, string> callback)
    {
        string json = JsonUtility.ToJson(data);
        StartCoroutine(PostJsonCoroutine(url, json, callback));
    }

    private IEnumerator PostJsonCoroutine(string url, string json, Action<bool, string> callback)
    {
        byte[] postData = Encoding.UTF8.GetBytes(json);
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("HTTP请求失败: " + www.error);
            callback?.Invoke(false, www.error);
        }
        else
        {
            Debug.Log("HTTP请求成功: " + www.downloadHandler.text);
            callback?.Invoke(true, www.downloadHandler.text);
        }
    }

    /// <summary>
    /// 下载数字人格 JSON 文件到 persistentDataPath/Personalities
    /// </summary>
    private IEnumerator DownloadPersonality(string url, Action<bool, string> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("下载失败: " + www.error);
            callback?.Invoke(false, www.error);
            yield break;
        }

        string folder = DataPath.Personalities;
        Directory.CreateDirectory(folder);

        string filename = Path.GetFileName(url);
        string path = Path.Combine(folder, filename);

        File.WriteAllText(path, www.downloadHandler.text);
        Debug.Log("Personality saved: " + path);

        callback?.Invoke(true, path);
    }

    /// <summary>
    /// 生成数字人格并下载 JSON
    /// </summary>
    public void GenerateAndDownloadNPC(NPCRequestData requestData, Action<bool, string> callback)
    {
        string url = serverURL + "api/generatePersonality";

        // 先POST生成
        PostJson(url, requestData, (success, response) =>
        {
            if (!success)
            {
                callback?.Invoke(false, "生成失败: " + response);
                return;
            }

            // 解析返回 JSON 获取 download_url
            GenerateResponse res = JsonUtility.FromJson<GenerateResponse>(response);
            if (!res.success || string.IsNullOrEmpty(res.download_url))
            {
                callback?.Invoke(false, "生成返回异常");
                return;
            }

            string fullUrl = serverURL.TrimEnd('/') + res.download_url;

            // 下载数字人格
            StartCoroutine(DownloadPersonality(fullUrl, callback));
        });
    }
    
    /// 下载单个 NPC 周计划 JSON
    /// </summary>
    private IEnumerator DownloadWeeklySchedule(string url, string npcName, Action<bool, string> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"下载{npcName}周计划失败: " + www.error);
            callback?.Invoke(false, null);
            yield break;
        }

        string folder = Path.Combine(Application.persistentDataPath, "WeeklySchedules");
        Directory.CreateDirectory(folder);

        string filename = Path.GetFileName(url);
        string path = Path.Combine(folder, filename);

        File.WriteAllText(path, www.downloadHandler.text);
        Debug.Log($"周计划已保存: {path}");

        callback?.Invoke(true, path);
    }

    /// <summary>
    /// 一步生成并下载所有 NPC 的 7 天周计划
    /// </summary>
    public void GenerateAndDownloadWeeklySchedules(List<string> npcNames, string startDate, Action<bool, Dictionary<string, string>> callback)
    {
        StartCoroutine(GenerateAndDownloadCoroutine(npcNames, startDate, callback));
    }

    private IEnumerator GenerateAndDownloadCoroutine(List<string> npcNames, string startDate, Action<bool, Dictionary<string, string>> callback)
    {
        var weekSchedulesPaths = new Dictionary<string, string>();

        var requestData = new
        {
            npcNames = npcNames,
            startDate = startDate
        };

        bool requestCompleted = false;
        string responseText = null;
        bool requestSuccess = false;

        PostJson(serverURL + "api/generateWeeklySchedule", requestData, (success, response) =>
        {
            requestCompleted = true;
            requestSuccess = success;
            responseText = response;
        });

        // 等待请求完成
        while (!requestCompleted)
            yield return null;

        if (!requestSuccess)
        {
            callback?.Invoke(false, null);
            yield break;
        }

        // 解析 JSON 返回
        var res = JsonUtility.FromJson<WeeklyResponse>(responseText);
        if (!res.success)
        {
            callback?.Invoke(false, null);
            yield break;
        }

        foreach (var kvp in res.weekSchedules)
        {
            string npcName = kvp.Key;
            string downloadUrl = serverURL.TrimEnd('/') + kvp.Value;

            bool downloadDone = false;
            string savedPath = null;
            StartCoroutine(DownloadWeeklySchedule(downloadUrl, npcName, (suc, path) =>
            {
                savedPath = path;
                downloadDone = true;
            }));

            while (!downloadDone)
                yield return null;

            weekSchedulesPaths[npcName] = savedPath;
        }

        callback?.Invoke(true, weekSchedulesPaths);
    }

    /// <summary>
    /// 请求并下载7天城市事件JSON
    /// </summary>
    public void DownloadCityEvents(string startDate, Action<bool, Dictionary<string, string>> callback)
    {
        StartCoroutine(DownloadCityEventsCoroutine(startDate, callback));
    }
    
    private IEnumerator DownloadCityEventsCoroutine(string startDate, Action<bool, Dictionary<string, string>> callback)
    {
        var requestData = new
        {
            startDate = string.IsNullOrEmpty(startDate) ? "2026-01-01" : startDate
        };

        bool requestCompleted = false;
        bool requestSuccess = false;
        string responseText = null;

        PostJson(serverURL + "api/getCityEvents", requestData, (success, response) =>
        {
            requestCompleted = true;
            requestSuccess = success;
            responseText = response;
        });

        while (!requestCompleted)
            yield return null;

        if (!requestSuccess)
        {
            callback?.Invoke(false, null);
            yield break;
        }

        var res = JsonUtility.FromJson<CityEventsResponse>(responseText);
        if (!res.success)
        {
            callback?.Invoke(false, null);
            yield break;
        }

        string folder = DataPath.CityEvents;
        Directory.CreateDirectory(folder);

        var savedPaths = new Dictionary<string, string>();

        foreach (var kvp in res.cityEvents)
        {
            string day = kvp.Key;              // monday
            string downloadUrl = serverURL.TrimEnd('/') + kvp.Value;

            bool done = false;
            string savedPath = null;

            StartCoroutine(DownloadCityEventFile(downloadUrl, day, (suc, path) =>
            {
                savedPath = path;
                done = true;
            }));

            while (!done)
                yield return null;

            savedPaths[day] = savedPath;
        }

        callback?.Invoke(true, savedPaths);
    }
    
    private IEnumerator DownloadCityEventFile(string url, string dayName, Action<bool, string> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("下载CityEvent失败: " + www.error);
            callback?.Invoke(false, null);
            yield break;
        }

        string folder = DataPath.CityEvents;
        Directory.CreateDirectory(folder);

        string filename = dayName.ToLower() + ".json";
        string path = Path.Combine(folder, filename);

        File.WriteAllText(path, www.downloadHandler.text);

        Debug.Log("CityEvent saved: " + path);

        callback?.Invoke(true, path);
    }

    // ------------------- 数据类 -------------------
    [System.Serializable]
    public class NPCRequestData
    {
        public string name;
        public string job;
        public string gender;
        public string mbti;
        public string description;
        public string behaviorUsername;
        public string creatorUsername;
    }

    [System.Serializable]
    private class GenerateResponse
    {
        public bool success;
        public string download_url;
    }
    
    [Serializable]
    private class WeeklyResponse
    {
        public bool success;
        public Dictionary<string, string> weekSchedules;
    }
    [Serializable]
    private class CityEventsResponse
    {
        public bool success;
        public Dictionary<string, string> cityEvents;
    }
    
}
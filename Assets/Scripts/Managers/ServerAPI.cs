using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ServerAPI : MonoBehaviour
{
    public static ServerAPI Instance;
    public string serverURL = "http://127.0.0.1:5000/";

    // ------------------- 加载事件 -------------------
    public event Action OnRequestStarted;
    public event Action OnRequestEnded;

    private void RaiseRequestStarted() => OnRequestStarted?.Invoke();
    private void RaiseRequestEnded() => OnRequestEnded?.Invoke();

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

    // ------------------- 通用 POST JSON -------------------
    public void PostJson(string url, object data, Action<bool, string> callback)
    {
        string json = JsonConvert.SerializeObject(data);
        StartCoroutine(PostJsonCoroutine(url, json, callback));
    }

    private IEnumerator PostJsonCoroutine(string url, string json, Action<bool, string> callback)
    {
        RaiseRequestStarted(); // 请求开始

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

        RaiseRequestEnded(); // 请求结束
    }

    // ------------------- 下载文件工具 -------------------
    private IEnumerator DownloadFile(string url, string folder, string filename, Action<bool, string> callback)
    {
        RaiseRequestStarted(); // 下载开始

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("下载失败: " + www.error);
            callback?.Invoke(false, null);
        }
        else
        {
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, filename);
            File.WriteAllText(path, www.downloadHandler.text);
            callback?.Invoke(true, path);
        }

        RaiseRequestEnded(); // 下载结束
    }

    // ------------------- 数字人格 -------------------
    [Serializable]
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

    public class GenerateResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }
    }

    public class PersonalitiesResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("personalities")]
        public List<Dictionary<string, object>> Personalities { get; set; }
    }

    public void GenerateAndDownloadNPC(NPCRequestData requestData, Action<bool, string> callback)
    {
        StartCoroutine(GenerateAndDownloadNPCCoroutine(requestData, callback));
    }

    private IEnumerator GenerateAndDownloadNPCCoroutine(NPCRequestData requestData, Action<bool, string> callback)
    {
        RaiseRequestStarted();

        // 1. POST 请求生成
        bool requestDone = false;
        bool requestSuccess = false;
        string responseText = null;

        PostJson(serverURL + "api/generatePersonality", requestData, (s, r) =>
        {
            requestDone = true;
            requestSuccess = s;
            responseText = r;
        });

        while (!requestDone)
            yield return null;

        if (!requestSuccess)
        {
            RaiseRequestEnded();
            callback?.Invoke(false, "生成失败: " + responseText);
            yield break;
        }

        var res = JsonConvert.DeserializeObject<GenerateResponse>(responseText);
        if (!res.Success || string.IsNullOrEmpty(res.DownloadUrl))
        {
            RaiseRequestEnded();
            callback?.Invoke(false, "生成返回异常");
            yield break;
        }

        // 2. 下载 JSON 文件
        string fullUrl = serverURL.TrimEnd('/') + res.DownloadUrl;
        string folder = DataPath.Personalities;
        string filename = Path.GetFileName(fullUrl);

        bool downloadDone = false;
        string path = null;

        StartCoroutine(DownloadFile(fullUrl, folder, filename, (suc, p) =>
        {
            path = p;
            downloadDone = true;
        }));

        while (!downloadDone)
            yield return null;

        RaiseRequestEnded();
        callback(true, path);
    }

    public void GetLatestPersonalities(int count, Action<bool, Dictionary<int, string>, List<int>> callback)
    {
        StartCoroutine(GetLatestPersonalitiesCoroutine(count, callback));
    }

    private IEnumerator GetLatestPersonalitiesCoroutine(
        int count,
        Action<bool, Dictionary<int, string>, List<int>> callback)
    {
        RaiseRequestStarted();

        string url = serverURL + "api/getLatestPersonalities";

        var requestData = new
        {
            count = count
        };

        string json = JsonConvert.SerializeObject(requestData);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("下载Personalities失败: " + www.error);
            RaiseRequestEnded();
            callback(false, null, null);
            yield break;
        }

        var res = JsonConvert.DeserializeObject<PersonalitiesResponse>(www.downloadHandler.text);

        if (!res.Success)
        {
            Debug.LogError("服务器返回失败");
            RaiseRequestEnded();
            callback(false, null, null);
            yield break;
        }

        string folder = DataPath.Personalities;
        Directory.CreateDirectory(folder);

        var savedPaths = new Dictionary<int, string>();
        var personalityIds = new List<int>();

        foreach (var personality in res.Personalities)
        {
            int id = System.Convert.ToInt32(personality["personality_id"]);
            string name = personality["personality_name"].ToString();

            string filename = name + ".json";
            string path = Path.Combine(folder, filename);

            string jsonText = JsonConvert.SerializeObject(personality, Formatting.Indented);

            File.WriteAllText(path, jsonText);

            savedPaths[id] = path;
            personalityIds.Add(id);
        }

        Debug.Log("Personalities 下载完成");

        RaiseRequestEnded();
        callback(true, savedPaths, personalityIds);
    }

    // ------------------- NPC 周计划 -------------------
    public class WeeklyResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("weekSchedules")]
        public Dictionary<string, object> WeekSchedules { get; set; }
    }

    private IEnumerator DownloadWeeklySchedule(string url, string npcName, Action<bool, string> callback)
    {
        string folder = Path.Combine(Application.persistentDataPath, "WeeklySchedules");
        string filename = npcName + ".json";
        yield return DownloadFile(url, folder, filename, callback);
    }

    public void GenerateAndDownloadWeeklySchedules(List<string> npcNames, string startDate, Action<bool, Dictionary<string, string>> callback)
    {
        StartCoroutine(GenerateAndDownloadWeeklySchedulesCoroutine(npcNames, startDate, callback));
    }

    private IEnumerator GenerateAndDownloadWeeklySchedulesCoroutine(List<string> npcNames, string startDate, Action<bool, Dictionary<string, string>> callback)
    {
        RaiseRequestStarted(); // 批量操作开始

        var weekSchedulesPaths = new Dictionary<string, string>();
        var requestData = new { npcNames = npcNames, startDate = startDate };

        bool requestCompleted = false;
        bool requestSuccess = false;
        string responseText = null;

        PostJson(serverURL + "api/generateWeeklySchedule", requestData, (s, r) =>
        {
            requestCompleted = true;
            requestSuccess = s;
            responseText = r;
        });

        while (!requestCompleted)
            yield return null;

        if (!requestSuccess)
        {
            RaiseRequestEnded();
            callback?.Invoke(false, null);
            yield break;
        }

        var res = JsonConvert.DeserializeObject<WeeklyResponse>(responseText);
        if (!res.Success)
        {
            RaiseRequestEnded();
            callback?.Invoke(false, null);
            yield break;
        }

        foreach (var kvp in res.WeekSchedules)
        {
            string npcName = kvp.Key;
            string url = serverURL.TrimEnd('/') + kvp.Value.ToString();
            bool done = false;
            string path = null;

            StartCoroutine(DownloadWeeklySchedule(url, npcName, (suc, p) =>
            {
                path = p;
                done = true;
            }));

            while (!done)
                yield return null;

            weekSchedulesPaths[npcName] = path;
        }

        RaiseRequestEnded(); // 批量操作结束
        callback(true, weekSchedulesPaths);
    }

    public void GetAllNPCSchedules(List<int> personalityIds, Action<bool, Dictionary<string, string>> callback)
    {
        StartCoroutine(GetAllNPCSchedulesCoroutine(personalityIds, callback));
    }

    private IEnumerator GetAllNPCSchedulesCoroutine(List<int> ids, Action<bool, Dictionary<string, string>> callback)
    {
        RaiseRequestStarted(); // 批量操作开始

        string url = serverURL + "api/getNPCSchedules";
        var requestData = new { personalityIds = ids };
        string json = JsonConvert.SerializeObject(requestData);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("下载NPC周计划失败: " + www.error);
            RaiseRequestEnded();
            callback(false, null);
            yield break;
        }

        var res = JsonConvert.DeserializeObject<WeeklyResponse>(www.downloadHandler.text);
        string folder = DataPath.NPCSchedules;
        Directory.CreateDirectory(folder);
        var savedPaths = new Dictionary<string, string>();

        foreach (var kvp in res.WeekSchedules)
        {
            string filename = kvp.Key + ".json";
            string path = Path.Combine(folder, filename);
            string jsonText = JsonConvert.SerializeObject(kvp.Value, Formatting.Indented);
            File.WriteAllText(path, jsonText);
            savedPaths[kvp.Key] = path;
        }

        RaiseRequestEnded();
        callback(true, savedPaths);
    }

    // ------------------- 城市事件 -------------------
    public class CityEventsResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("cityEvents")]
        public Dictionary<string, object> CityEvents { get; set; }
    }
    [Serializable]
    public class CityEventConfig
    {
        public string eventName;
        public string eventDescription;
        public string buildingName;
        public int startTime;
        public int endTime;
        public int capacity;
    }

    [Serializable]
    public class CityEventConfigList
    {
        public List<CityEventConfig> events;
    }
    

    public void GetCityEvents(string startDate, System.Action<bool, Dictionary<string, string>> callback)
    {
        StartCoroutine(GetCityEventsCoroutine(startDate, callback));
    }

    private IEnumerator GetCityEventsCoroutine(string startDate, System.Action<bool, Dictionary<string, string>> callback)
    {
        RaiseRequestStarted();

        string url = serverURL + "api/getCityEvents";

        var requestData = new
        {
            startDate = startDate
        };

        string json = JsonConvert.SerializeObject(requestData);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("下载CityEvents失败: " + www.error);
            RaiseRequestEnded();
            callback(false, null);
            yield break;
        }

        var res = JsonConvert.DeserializeObject<CityEventsResponse>(www.downloadHandler.text);

        if (!res.Success)
        {
            Debug.LogError("服务器返回失败");
            RaiseRequestEnded();
            callback(false, null);
            yield break;
        }

        string folder = DataPath.CityEvents;
        Directory.CreateDirectory(folder);

        var savedPaths = new Dictionary<string, string>();

        foreach (var kvp in res.CityEvents)
        {
            string day = kvp.Key.ToLower();
            string filename = day + ".json";

            string path = Path.Combine(folder, filename);

            string jsonText = JsonConvert.SerializeObject(kvp.Value, Formatting.Indented);

            File.WriteAllText(path, jsonText);

            savedPaths[day] = path;
        }

        Debug.Log("CityEvents 下载完成");

        RaiseRequestEnded();
        callback(true, savedPaths);
    }
}
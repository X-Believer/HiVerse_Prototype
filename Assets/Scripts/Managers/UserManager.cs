using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class UserProfile
{
    public int userId;
    public string username;

    public int age;
    public Gender gender;    // 枚举
    public string job;
    public string mbti;
    public string description;
}

public class UserManager : MonoBehaviour
{
    public static UserManager Instance;

    private UserProfile currentUser;

    public event Action<UserProfile> OnUserProfileChanged;

    private string userFolderPath;
    private string serverUrl = "http://127.0.0.1:5000"; // 后端地址，可改

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        userFolderPath = DataPath.UserProfile;
    }

    void Start()
    {
        // StartCoroutine(LoginServerOrLocal("Shuyang Xing", "123456"));
        AutoLogin();
    }

    public UserProfile GetCurrentUser()
    {
        return currentUser;
    }

    // ======================================
    // 登录：优先服务器，失败则本地
    // ======================================
    public IEnumerator LoginServerOrLocal(string username, string password)
    {
        bool loginSuccess = false;

        // 尝试服务器登录
        string loginJson = JsonUtility.ToJson(new { username = username, password = password });
        using (UnityWebRequest www = UnityWebRequest.Put(serverUrl + "/user/login", loginJson))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    currentUser = JsonUtility.FromJson<UserProfile>(www.downloadHandler.text);
                    loginSuccess = true;
                    Debug.Log($"[Server Login] Success: {currentUser.username}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning("解析服务器返回用户信息失败: " + e);
                }
            }
            else
            {
                Debug.LogWarning("[Server Login] Failed: " + www.error);
            }
        }

        // 如果服务器失败，使用本地 JSON fallback
        if (!loginSuccess)
        {
            AutoLogin();
        }

        OnUserProfileChanged?.Invoke(currentUser);
    }

    // ======================================
    // 本地登录
    // ======================================
    public void AutoLogin()
    {
        string path = Path.Combine(userFolderPath, "000.json");

        if (!File.Exists(path))
        {
            Debug.LogError("User profile not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        currentUser = JsonUtility.FromJson<UserProfile>(json);

        Debug.Log($"[Local Login] User: {currentUser.username} Gender: {currentUser.gender}");
    }

    // ======================================
    // 更新用户资料：服务器优先，本地 fallback
    // ======================================
    public void UpdateUserProfile(
        int? age = null,
        Gender? gender = null,
        string job = null,
        string mbti = null,
        string description = null)
    {
        if (currentUser == null)
            return;

        if (age.HasValue)
            currentUser.age = age.Value;

        if (gender.HasValue)
            currentUser.gender = gender.Value;

        if (!string.IsNullOrEmpty(job))
            currentUser.job = job;

        if (!string.IsNullOrEmpty(mbti))
            currentUser.mbti = mbti;

        if (!string.IsNullOrEmpty(description))
            currentUser.description = description;

        // 先更新服务器
        StartCoroutine(UpdateServerOrLocal());
    }

    // ======================================
    // 更新服务器或本地
    // ======================================
    private IEnumerator UpdateServerOrLocal()
    {
        if (currentUser == null)
            yield break;

        string json = JsonUtility.ToJson(new
        {
            userId = currentUser.userId,
            age = currentUser.age,
            gender = (int)currentUser.gender,
            job = currentUser.job,
            mbti = currentUser.mbti,
            description = currentUser.description
        });

        bool updateSuccess = false;

        using (UnityWebRequest www = UnityWebRequest.Put(serverUrl + "/user/update", json))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                updateSuccess = true;
                Debug.Log("[Server Update] Success");
            }
            else
            {
                Debug.LogWarning("[Server Update] Failed: " + www.error);
            }
        }

        // 服务器失败，写本地 JSON
        if (!updateSuccess)
        {
            SaveUserProfileLocal();
        }

        OnUserProfileChanged?.Invoke(currentUser);
    }

    // ======================================
    // 保存本地 JSON
    // ======================================
    private void SaveUserProfileLocal()
    {
        if (currentUser == null)
            return;

        string fileName = currentUser.userId.ToString("000") + ".json";
        string path = Path.Combine(userFolderPath, fileName);

        string json = JsonUtility.ToJson(currentUser, true);
        File.WriteAllText(path, json);

        Debug.Log("[Local Save] User profile saved: " + path);
    }
}
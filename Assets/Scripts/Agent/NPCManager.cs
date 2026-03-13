using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public enum NPCSpeed
{
    Slow = 1,
    Normal = 2,
    Fast = 3
}

[Serializable]
public enum Gender
{
    Male = 1,
    Female = 2
}

[Serializable]
public class NPCPersonalityData
{
    public string name;
    public int age;
    public int gender;
    public string innate;
    public string learned;
    public string currently;
    public string lifestyle;
    public string living_area;
}

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    [Header("NPC Prefab")]
    public GameObject npcPrefab;

    public Vector3 NPCStartPosition;
    public NPCSpeed NPCSpeed = NPCSpeed.Normal;

    [Header("NPC Container")]
    public Transform agentsRoot;

    [Header("Raycast")]
    public LayerMask npcLayer;

    private List<NPCController> npcs = new List<NPCController>();

    // 鼠标悬停
    private NPCController hoverNPC;

    // 当前选中
    private NPCController currentNPC;

    // ======================
    // 事件
    // ======================
    public static event Action<NPCController> OnNPCSpawned;
    public static event Action<NPCController> OnNPCRemoved;
    public static event Action<NPCController> OnCurrentNPCChanged;
    public static event Action<List<NPCController>> OnNPCListChanged;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        LoadNPCsFromPersonalities();
    }

    void OnEnable()
    {
        WorldClock.OnDayChanged += ResetNPCPosition;
    }

    void OnDisable()
    {
        WorldClock.OnDayChanged -= ResetNPCPosition;
    }

    void Update()
    {
        DetectMouseNPC();
    }

    // ======================
    // 鼠标检测
    // ======================
    void DetectMouseNPC()
    {
        // 鼠标在UI上时不检测
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        NPCController npc = null;

        if (Physics.Raycast(ray, out hit, 100f, npcLayer))
        {
            npc = hit.collider.GetComponentInParent<NPCController>();
        }

        // Hover变化
        if (hoverNPC != npc)
        {
            if (hoverNPC != null)
                hoverNPC.Highlight(false);

            hoverNPC = npc;

            if (hoverNPC != null)
                hoverNPC.Highlight(true);
        }

        // 点击
        if (hoverNPC != null && Input.GetMouseButtonDown(0))
        {
            SetCurrentNPC(hoverNPC);
            hoverNPC.OnClick();
        }
    }

    // ======================
    // NPC生成
    // ======================
    void LoadNPCsFromPersonalities()
    {
        string folder = DataPath.Personalities;

        if (!Directory.Exists(folder))
        {
            Debug.LogWarning("Personalities folder not found");
            return;
        }

        string[] files = Directory.GetFiles(folder, "*.json");

        int index = 0;

        foreach (string file in files)
        {
            string json = File.ReadAllText(file);

            NPCPersonalityData data =
                JsonUtility.FromJson<NPCPersonalityData>(json);

            Vector3 pos = NPCStartPosition + new Vector3(
                (index % 5) * 2,
                0,
                (index / 5) * 2
            );

            SpawnNPCWithData(pos, data);

            index++;
        }
    }

    NPCController SpawnNPCWithData(Vector3 position, NPCPersonalityData data)
    {
        GameObject obj = Instantiate(
            npcPrefab,
            position,
            Quaternion.identity,
            agentsRoot
        );

        NPCController npc = obj.GetComponent<NPCController>();

        if (npc != null)
        {
            npc.InitFromPersonality(data);

            NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
            if (agent != null)
                agent.speed = GetNPCSpeed();

            RegisterNPC(npc);
        }

        return npc;
    }

    // ======================
    // 注册 / 注销 NPC
    // ======================
    public void RegisterNPC(NPCController npc)
    {
        if (npc != null && !npcs.Contains(npc))
        {
            npcs.Add(npc);

            OnNPCSpawned?.Invoke(npc);
            OnNPCListChanged?.Invoke(npcs);
        }
    }

    public void UnregisterNPC(NPCController npc)
    {
        if (npc != null && npcs.Contains(npc))
        {
            npcs.Remove(npc);

            OnNPCRemoved?.Invoke(npc);
            OnNPCListChanged?.Invoke(npcs);
        }
    }

    // ======================
    // 删除 NPC
    // ======================
    public void RemoveNPC(NPCController npc)
    {
        if (npc == null) return;

        if (npcs.Contains(npc))
            npcs.Remove(npc);

        OnNPCRemoved?.Invoke(npc);
        OnNPCListChanged?.Invoke(npcs);

        Destroy(npc.gameObject);
    }

    // ======================
    // 获取 NPC 列表
    // ======================
    public List<NPCController> GetAllNPCs()
    {
        return npcs;
    }

    // ======================
    // 重置 NPC 位置
    // ======================
    public void ResetNPCPosition()
    {
        for (int i = 0; i < npcs.Count; i++)
        {
            int row = i / 5;
            int col = i % 5;

            Vector3 pos = NPCStartPosition + new Vector3(col * 2, 0, row * 2);

            NavMeshAgent agent = npcs[i].GetComponent<NavMeshAgent>();

            if (agent != null)
            {
                agent.Warp(pos);
                agent.ResetPath();
                agent.isStopped = true;
            }
            else
            {
                npcs[i].transform.position = pos;
            }
        }
    }

    float GetNPCSpeed()
    {
        switch (NPCSpeed)
        {
            case NPCSpeed.Slow: return 3;
            case NPCSpeed.Normal: return 4;
            case NPCSpeed.Fast: return 6;
        }

        return 3f;
    }

    public void SetNPCSpeed(NPCSpeed speed)
    {
        NPCSpeed = speed;

        foreach (var npc in npcs)
        {
            if (npc != null)
            {
                NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();

                if (agent != null)
                    agent.speed = GetNPCSpeed();
            }
        }
    }

    public NPCController GetCurrentNPC()
    {
        return currentNPC;
    }

    public void SetCurrentNPC(NPCController npc)
    {
        if (currentNPC == npc) return;

        currentNPC = npc;

        OnCurrentNPCChanged?.Invoke(currentNPC);
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum NPCSpeed
{
    Slow = 1,
    Normal = 2,
    Fast = 3
}

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    [Header("NPC Prefab")]
    public GameObject npcPrefab;

    public Vector3 NPCStartPosition;
    public NPCSpeed NPCSpeed = NPCSpeed.Normal;

    private List<NPCController> npcs = new List<NPCController>();
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            NPCController npc = hit.collider.GetComponentInParent<NPCController>();

            if (npc != null)
            {
                if (currentNPC != npc)
                {
                    ClearHighlight();

                    currentNPC = npc;
                    currentNPC.Highlight(true);

                    OnCurrentNPCChanged?.Invoke(currentNPC);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    currentNPC.OnClick();
                }

                return;
            }
        }

        ClearHighlight();
    }

    void ClearHighlight()
    {
        if (currentNPC != null)
        {
            currentNPC.Highlight(false);
            currentNPC = null;

            OnCurrentNPCChanged?.Invoke(null);
        }
    }

    // ======================
    // NPC生成
    // ======================
    public NPCController SpawnNPC(Vector3 position)
    {
        GameObject obj = Instantiate(npcPrefab, position, Quaternion.identity);
        NPCController npc = obj.GetComponent<NPCController>();

        if (npc != null)
        {
            RegisterNPC(npc);
        }
        else
        {
            Debug.LogWarning("Spawned NPC has no NPCController!");
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
            npcs[i].transform.position = pos;

            // 重置NavMeshAgent状态
            NavMeshAgent agent = npcs[i].GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.Warp(pos); // 瞬间移动到目标，不会被NavMesh阻挡
                agent.ResetPath(); // 清除原来的路径
                agent.isStopped = true;
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
                {
                    agent.speed = GetNPCSpeed();
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    [Header("NPC Prefab")]
    public GameObject npcPrefab;

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
}
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    [Header("NPC Prefab")]
    public GameObject npcPrefab;

    private List<NPCController> npcs = new List<NPCController>();

    private NPCController currentNPC;

    void Awake()
    {
        Instance = this;
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
    // 删除NPC
    // ======================

    public void RemoveNPC(NPCController npc)
    {
        if (npc == null) return;

        if (npcs.Contains(npc))
        {
            npcs.Remove(npc);
        }

        Destroy(npc.gameObject);
    }

    // ======================
    // 注册NPC
    // ======================

    public void RegisterNPC(NPCController npc)
    {
        if (npc != null && !npcs.Contains(npc))
        {
            npcs.Add(npc);
        }
    }

    // ======================
    // 注销NPC
    // ======================

    public void UnregisterNPC(NPCController npc)
    {
        if (npc != null && npcs.Contains(npc))
        {
            npcs.Remove(npc);
        }
    }

    // ======================
    // 获取NPC列表
    // ======================

    public List<NPCController> GetAllNPCs()
    {
        return npcs;
    }
}
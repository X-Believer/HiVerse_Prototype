using UnityEngine;
using UnityEngine.AI;
using HiVerse.AI;

public class NPCController : MonoBehaviour
{
    [SerializeField]
    private string npcName;
    public string Name => npcName;
    
    private string job;
    
    [Header("Schedule")]
    public NPCSchedule schedule;

    private NavMeshAgent agent;
    private Outline outline;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        outline = GetComponentInChildren<Outline>();

        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    void Update()
    {
        UpdateSchedule();
    }

    // ======================
    // 日程系统
    // ======================

    void UpdateSchedule()
    {
        if (schedule == null)
            return;

        foreach (var item in schedule.dailyPlan)
        {
            
        }
    }

    // ======================
    // 鼠标高亮
    // ======================

    public void Highlight(bool enable)
    {
        if (outline != null)
        {
            outline.enabled = enable;
        }
    }

    // ======================
    // 交互
    // ======================

    public void OnClick()
    {
        Debug.Log("Interact with NPC: " + name);

        UIManager.Instance.OpenWorldPanelTab(0);
    }
}
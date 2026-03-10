using UnityEngine;
using UnityEngine.AI;
using HiVerse.AI;

public class NPCController : MonoBehaviour
{
    [SerializeField] private string npcName;
    [SerializeField] private Sprite icon;
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
    
    void OnEnable()
    {
        CameraManager.OnCameraModeChanged += OnCameraModeChanged;
    }

    void OnDisable()
    {
        CameraManager.OnCameraModeChanged -= OnCameraModeChanged;
    }
    
    void Start()
    {
        NPCManager.Instance.RegisterNPC(this);
        UIManager.Instance.CreateMarker(transform, npcName, icon, new Vector3(0, 3.0f, 0));
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
    // 交互
    // ======================
    public void Highlight(bool enable)
    {
        if (outline != null)
        {
            outline.enabled = enable;
        }
    }

    public void OnClick()
    {
        Debug.Log("Interact with NPC: " + name);

        UIManager.Instance.OpenWorldPanelTab(0);
    }
    
    // ======================
    // 监听摄像机模式
    // ======================
    void OnCameraModeChanged(CameraMode mode)
    {
        
    }
}
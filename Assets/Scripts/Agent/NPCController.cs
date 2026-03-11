using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using Opsive.BehaviorDesigner.Runtime;
using Opsive.BehaviorDesigner;
using Opsive.GraphDesigner.Runtime.Variables;

public class NPCController : MonoBehaviour
{
    public string npcName;
    public Sprite icon;
    private string job;
    
    [Header("Schedule")]
    public NPCSchedule schedule;
    private ScheduleItem currentTask;
    private string currentActionDescription;
    
    private NavMeshAgent agent;
    private Animator animator;
    private BehaviorTree behaviorTree;
    private bool isMovingToTarget = false;
    private string currentTargetBuilding;
    private BuildingEntrance currentTargetEntrance;

    private Outline outline;
    
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        outline = GetComponentInChildren<Outline>();
        behaviorTree = GetComponent<BehaviorTree>();
        animator = GetComponent<Animator>();

        if (outline != null)
        {
            outline.enabled = false;
        }
    }
    
    void OnEnable()
    {
        CameraManager.OnCameraModeChanged += OnCameraModeChanged;
        WorldClock.OnDayChanged += ReloadDailySchedule;
        WorldClock.OnMinuteChanged += OnMinuteChanged;
        BuildingManager.OnBuildingEventStarted += OnEventStarted;
    }

    void OnDisable()
    {
        CameraManager.OnCameraModeChanged -= OnCameraModeChanged;
        WorldClock.OnDayChanged -= ReloadDailySchedule;
        WorldClock.OnMinuteChanged -= OnMinuteChanged;
        BuildingManager.OnBuildingEventStarted -= OnEventStarted;
    }
    
    void Start()
    {
        NPCManager.Instance.RegisterNPC(this);
        UIManager.Instance.CreateMarker(transform, npcName, icon, new Vector3(0, 3.0f, 0));
        behaviorTree.SetVariableValue("Agent", gameObject);
        ReloadDailySchedule();
    }

    void Update()
    {
        UpdateSchedule();
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
        animator.SetFloat("MotionSpeed",1);
    }

    // ======================
    // 日程系统
    // ======================
    void ReloadDailySchedule()
    {
        LoadScheduleFromJson();
    }
    
    void OnMinuteChanged()
    {
        CheckSchedule();
    }
    
    void CheckSchedule()
    {
        if (schedule == null)
            return;

        int hour = WorldClock.Instance.CurrentHour;
        int minute = WorldClock.Instance.CurrentMinute;

        foreach (var item in schedule.dailyPlan)
        {
            if (item.hour == hour && item.minute == minute)
            {
                StartSchedule(item);
                break;
            }
        }
    }
    
    public void StartSchedule(ScheduleItem item)
    {
        currentTask = item;
        currentActionDescription = item.action;
        currentTargetBuilding = item.target;

        if (item.target != null)
        {
            // 重置 NavMeshAgent
            agent.isStopped = false;
            agent.ResetPath();

            // 更新行为树黑板变量
            if (behaviorTree.GetVariable("TargetBuildingName") is SharedVariable<string> targetVar) targetVar.Value = item.target;

            if (behaviorTree.GetVariable("CurrentTask") is SharedVariable<string> taskVar) taskVar.Value = item.action;

            // 启用行为树
            behaviorTree.enabled = true;
        }

        Debug.Log($"{npcName} 开始前往: {item.action}");
    }
    public string GetCurrentActionDescription()
    {
        return currentActionDescription;
    }

    // 行为树结束任务回调
    public void OnTaskComplete()
    {
        if (currentTask != null)
        {
            // 更新建筑内部列表
            var entrances = GameObject.FindObjectsOfType<BuildingEntrance>();
            foreach (var e in entrances)
            {
                if (e.building.buildingName == currentTask.target)
                {
                    e.building.EnterBuilding(this);
                    break;
                }
            }

            currentActionDescription = $"At {currentTask.target}";
            currentTask = null;
        }

        // 清理 Agent 状态，保证下一次移动
        agent.isStopped = true;
        agent.ResetPath();

        // 等待下一次日程触发
        behaviorTree.enabled = false;
    }
    
    
    void LoadScheduleFromJson()
    {
        string path = Path.Combine(
            Application.streamingAssetsPath,
            "NPCSchedules",
            npcName + ".json"
        );

        if (!File.Exists(path))
            return;

        string json = File.ReadAllText(path);

        NPCWeekScheduleData data =
            JsonUtility.FromJson<NPCWeekScheduleData>(json);

        List<ScheduleItemData> todayPlan = GetTodayPlan(data);

        schedule = ScriptableObject.CreateInstance<NPCSchedule>();

        foreach (var item in todayPlan)
        {
            ScheduleItem scheduleItem = new ScheduleItem();

            scheduleItem.hour = item.hour;
            scheduleItem.minute = item.minute;
            scheduleItem.target = item.target;
            scheduleItem.action = item.action;
            scheduleItem.duration = item.duration;

            schedule.dailyPlan.Add(scheduleItem);
        }
    }
    
    List<ScheduleItemData> GetTodayPlan(NPCWeekScheduleData data)
    {
        switch (WorldClock.Instance.CurrentDayOfWeek)
        {
            case DayOfWeek.Monday:
                return data.weekPlan.Monday;

            case DayOfWeek.Tuesday:
                return data.weekPlan.Tuesday;

            case DayOfWeek.Wednesday:
                return data.weekPlan.Wednesday;

            case DayOfWeek.Thursday:
                return data.weekPlan.Thursday;

            case DayOfWeek.Friday:
                return data.weekPlan.Friday;

            case DayOfWeek.Saturday:
                return data.weekPlan.Saturday;

            case DayOfWeek.Sunday:
                return data.weekPlan.Sunday;
        }

        return null;
    }
    
    void OnEventStarted(BuildingEvent e)
    {
        // NPC 决定是否参加
    }
    
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
    
    public void OnFootstep()
    {
        
    }
    
    // ======================
    // 监听摄像机模式
    // ======================
    void OnCameraModeChanged(CameraMode mode)
    {
        
    }
}
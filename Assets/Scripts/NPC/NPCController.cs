using UnityEngine;
using UnityEngine.AI;
using HiVerse.AI;

public class NPCController : MonoBehaviour
{
    public NPCSchedule schedule;      // 指向 ScriptableObject
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // 这里可以加时间匹配逻辑
        foreach (var item in schedule.dailyPlan)
        {
            // 示例：当前时间匹配，执行移动
            if(item.hour == WorldClock.CurrentHour && item.minute == WorldClock.CurrentMinute)
            {
                agent.SetDestination(item.target.position);
            }
        }
    }
}
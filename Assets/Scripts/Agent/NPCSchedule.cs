using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NPCSchedule", menuName = "HiVerse/NPC Schedule")]
public class NPCSchedule : ScriptableObject
{
    public List<ScheduleItem> dailyPlan = new List<ScheduleItem>();
}
    

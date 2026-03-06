using UnityEngine;
using System.Collections.Generic;

namespace HiVerse.AI
{
    [CreateAssetMenu(fileName = "NPCSchedule", menuName = "HiVerse/NPC Schedule")]
    public class NPCSchedule : ScriptableObject
    {
        public List<ScheduleItem> dailyPlan = new List<ScheduleItem>();
    }
}
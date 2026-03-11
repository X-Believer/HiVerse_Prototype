using System;
using System.Collections.Generic;

[Serializable]
public class NPCWeekScheduleData
{
    public WeekPlan weekPlan;
}

[Serializable]
public class WeekPlan
{
    public List<ScheduleItemData> Monday;
    public List<ScheduleItemData> Tuesday;
    public List<ScheduleItemData> Wednesday;
    public List<ScheduleItemData> Thursday;
    public List<ScheduleItemData> Friday;
    public List<ScheduleItemData> Saturday;
    public List<ScheduleItemData> Sunday;
}

[Serializable]
public class ScheduleItemData
{
    public int hour;
    public int minute;
    public string action;
    public string target;
    public float duration;
}
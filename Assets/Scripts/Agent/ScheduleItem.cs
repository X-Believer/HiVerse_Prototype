using UnityEngine;
using System;

[Serializable]
public class ScheduleItem
{
    public int hour;
    public int minute;
    public string action;
    public string target;
    public float duration = 60f;
}

using UnityEngine;
using System;

namespace HiVerse.AI
{
    [Serializable]
    public class ScheduleItem
    {
        public int hour;
        public int minute;
        public string action;
        public Transform target;
        public float duration = 60f;
    }
}
using System;
using UnityEngine;

public enum TimePeriod
{
    Night,      // 0-5
    Morning,    // 6-11
    Afternoon,  // 12-17
    Evening     // 18-23
}

public enum TimeSpeed
{
    Slow = 1,
    Normal = 2,
    Fast = 3
}

public class WorldClock : MonoBehaviour
{
    public static WorldClock Instance;

    [Header("时间设置")]
    [Tooltip("每天开始时间的小时")]
    [Range(0, 23)]
    public int startHour = 8;

    [Tooltip("每天开始时间的分钟")]
    [Range(0, 59)]
    public int startMinute = 0;

    [Header("时间速度")]
    public TimeSpeed timeSpeed = TimeSpeed.Normal;

    public int CurrentHour { get; private set; }
    public int CurrentMinute { get; private set; }

    public int CurrentDay { get; private set; }
    public DayOfWeek CurrentDayOfWeek { get; private set; }

    public TimePeriod CurrentPeriod { get; private set; }

    private float totalGameMinutes;

    const int MinutesPerDay = 1440;

    public static event Action OnMinuteChanged;
    public static event Action OnHourChanged;
    public static event Action OnDayChanged;
    public static event Action<TimePeriod> OnPeriodChanged;

    void Awake()
    {
        Instance = this;

        CurrentDay = 1;
        CurrentDayOfWeek = DayOfWeek.Monday;

        // 设置游戏总分钟数为第一天开始时间
        totalGameMinutes = startHour * 60 + startMinute;
        AdvanceTime(0);
    }

    void Update()
    {
        AdvanceTime(Time.deltaTime);
    }

    float GetTimeScale()
    {
        switch (timeSpeed)
        {
            case TimeSpeed.Slow:
                return 1440f / 1800f;
            case TimeSpeed.Normal:
                return 1440f / 600f;
            case TimeSpeed.Fast:
                return 1440f / 120f;
        }
        return 1f;
    }

    void AdvanceTime(float delta)
    {
        totalGameMinutes += delta * GetTimeScale();

        int minutes = (int)totalGameMinutes;

        int newHour = (minutes / 60) % 24;
        int newMinute = minutes % 60;
        int dayCount = minutes / MinutesPerDay;

        if (newMinute != CurrentMinute)
        {
            CurrentMinute = newMinute;
            OnMinuteChanged?.Invoke();
        }

        if (newHour != CurrentHour)
        {
            CurrentHour = newHour;
            OnHourChanged?.Invoke();
            UpdatePeriod();
        }

        if (dayCount + 1 != CurrentDay)
        {
            CurrentDay = dayCount + 1;

            CurrentDayOfWeek =
                (DayOfWeek)(((int)DayOfWeek.Monday + dayCount) % 7);

            OnDayChanged?.Invoke();
        }
    }

    void UpdatePeriod()
    {
        TimePeriod newPeriod = GetPeriodByHour(CurrentHour);

        if (newPeriod != CurrentPeriod)
        {
            CurrentPeriod = newPeriod;
            OnPeriodChanged?.Invoke(newPeriod);
        }
    }

    TimePeriod GetPeriodByHour(int hour)
    {
        if (hour < 6)
            return TimePeriod.Night;
        if (hour < 12)
            return TimePeriod.Morning;
        if (hour < 18)
            return TimePeriod.Afternoon;
        return TimePeriod.Evening;
    }

    public void SetTimeSpeed(TimeSpeed speed)
    {
        timeSpeed = speed;
    }

    /// <summary>
    /// 跳到指定星期，并重置时间到每天开始时间
    /// </summary>
    public void JumpToDay(DayOfWeek targetDay)
    {
        int current = (int)CurrentDayOfWeek;
        int target = (int)targetDay;

        int diff = target - current;
        if (diff < 0) diff += 7;

        JumpDays(diff);
    }

    /// <summary>
    /// 跳过若干天，每天从设置的开始时间开始
    /// </summary>
    public void JumpDays(int days)
    {
        // 计算新的总分钟数
        int currentDayCount = (int)(totalGameMinutes / MinutesPerDay);
        currentDayCount += days;
        totalGameMinutes = currentDayCount * MinutesPerDay + startHour * 60 + startMinute;

        AdvanceTime(0);
    }

    /// <summary>
    /// 跳到下一天
    /// </summary>
    public void SkipToNextDay()
    {
        JumpDays(1);
    }

    /// <summary>
    /// 设置每天开始时间
    /// </summary>
    public void SetStartTime(int hour, int minute)
    {
        startHour = Mathf.Clamp(hour, 0, 23);
        startMinute = Mathf.Clamp(minute, 0, 59);

        // 立即应用当前天的开始时间
        int currentDayCount = (int)(totalGameMinutes / MinutesPerDay);
        totalGameMinutes = currentDayCount * MinutesPerDay + startHour * 60 + startMinute;
        AdvanceTime(0);
    }

    /// <summary>
    /// 获取当前时间字符串
    /// </summary>
    public string GetTimeString()
    {
        return $"{CurrentHour:00}:{CurrentMinute:00}";
    }
}
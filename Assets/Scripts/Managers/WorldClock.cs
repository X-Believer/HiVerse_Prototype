using System;
using UnityEngine;

public enum TimePeriod
{
    Night,
    Morning,
    Afternoon,
    Evening
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
    [Range(0, 23)]
    public int startHour = 8;

    [Range(0, 59)]
    public int startMinute = 0;

    [Header("时间速度")]
    public TimeSpeed timeSpeed = TimeSpeed.Normal;

    public int CurrentHour { get; private set; }
    public int CurrentMinute { get; private set; }

    public int CurrentDay { get; private set; }
    public DayOfWeek CurrentDayOfWeek { get; private set; }

    public TimePeriod CurrentPeriod { get; private set; }

    public int CurrentTotalMinutes { get; private set; }

    const int MinutesPerDay = 1440;

    float totalGameMinutes;

    public static event Action OnMinuteChanged;
    public static event Action OnHourChanged;
    public static event Action OnDayChanged;
    public static event Action<TimePeriod> OnPeriodChanged;

    void Awake()
    {
        Instance = this;

        CurrentDay = 1;
        CurrentDayOfWeek = DayOfWeek.Monday;

        totalGameMinutes = startHour * 60 + startMinute;
    }
    
    void Start()
    {
        UpdateTimeState((int)totalGameMinutes, true);
    }

    void Update()
    {
        AdvanceTime(Time.deltaTime);
    }

    float GetTimeScale()
    {
        switch (timeSpeed)
        {
            case TimeSpeed.Slow: return 1440f / 1800f;
            case TimeSpeed.Normal: return 1440f / 600f;
            case TimeSpeed.Fast: return 1440f / 120f;
        }

        return 1f;
    }

    void AdvanceTime(float delta)
    {
        totalGameMinutes += delta * GetTimeScale();

        int newTotalMinutes = (int)totalGameMinutes;

        while (CurrentTotalMinutes < newTotalMinutes)
        {
            CurrentTotalMinutes++;
            UpdateTimeState(CurrentTotalMinutes, false);
        }
    }

    void UpdateTimeState(int minutes, bool forceUpdate)
    {
        int hour = (minutes / 60) % 24;
        int minute = minutes % 60;
        int dayIndex = minutes / MinutesPerDay;

        bool minuteChanged = forceUpdate || minute != CurrentMinute;
        bool hourChanged = forceUpdate || hour != CurrentHour;
        bool dayChanged = forceUpdate || dayIndex + 1 != CurrentDay;

        CurrentMinute = minute;
        CurrentHour = hour;

        if (dayChanged)
        {
            CurrentDay = dayIndex + 1;

            CurrentDayOfWeek =
                (DayOfWeek)(((int)DayOfWeek.Monday + dayIndex) % 7);
        }

        if (minuteChanged)
            OnMinuteChanged?.Invoke();

        if (hourChanged)
        {
            OnHourChanged?.Invoke();
            UpdatePeriod();
        }

        if (dayChanged)
            OnDayChanged?.Invoke();
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
        if (hour < 6) return TimePeriod.Night;
        if (hour < 12) return TimePeriod.Morning;
        if (hour < 18) return TimePeriod.Afternoon;
        return TimePeriod.Evening;
    }

    public void SetTimeSpeed(TimeSpeed speed)
    {
        timeSpeed = speed;
    }

    public void JumpDays(int days)
    {
        int currentDayIndex = CurrentTotalMinutes / MinutesPerDay;

        int newDayIndex = currentDayIndex + days;

        totalGameMinutes = newDayIndex * MinutesPerDay + startHour * 60 + startMinute;

        CurrentTotalMinutes = (int)totalGameMinutes;

        UpdateTimeState(CurrentTotalMinutes, true);
    }

    public void SkipToNextDay()
    {
        JumpDays(1);
    }

    public void JumpToDay(DayOfWeek targetDay)
    {
        int current = (int)CurrentDayOfWeek;
        int target = (int)targetDay;

        int diff = target - current;

        if (diff < 0)
            diff += 7;

        JumpDays(diff);
    }

    public void SetStartTime(int hour, int minute)
    {
        startHour = Mathf.Clamp(hour, 0, 23);
        startMinute = Mathf.Clamp(minute, 0, 59);

        int currentDayIndex = CurrentTotalMinutes / MinutesPerDay;

        totalGameMinutes = currentDayIndex * MinutesPerDay + startHour * 60 + startMinute;

        CurrentTotalMinutes = (int)totalGameMinutes;

        UpdateTimeState(CurrentTotalMinutes, true);
    }

    public string GetTimeString()
    {
        return $"{CurrentHour:00}:{CurrentMinute:00}";
    }
}
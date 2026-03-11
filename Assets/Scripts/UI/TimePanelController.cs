using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TimePanelController : MonoBehaviour
{
    public TMP_Text timeText;
    public TMP_Dropdown dayDropdown;
    public Slider speedSlider;

    void Start()
    {
        InitDropdown();
        InitSlider();

        UpdateTimeUI();

        WorldClock.OnMinuteChanged += UpdateTimeUI;
        WorldClock.OnDayChanged += UpdateDayUI;
    }

    void OnDestroy()
    {
        WorldClock.OnMinuteChanged -= UpdateTimeUI;
        WorldClock.OnDayChanged -= UpdateDayUI;
    }

    void InitDropdown()
    {
        dayDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();

        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            options.Add(day.ToString());
        }

        dayDropdown.AddOptions(options);

        dayDropdown.onValueChanged.AddListener(OnDayDropdownChanged);

        UpdateDayUI();
    }

    void InitSlider()
    {
        speedSlider.minValue = 1;
        speedSlider.maxValue = 3;
        speedSlider.wholeNumbers = true;

        speedSlider.value = (int)WorldClock.Instance.timeSpeed;

        speedSlider.onValueChanged.AddListener(OnSpeedChanged);
    }

    void UpdateTimeUI()
    {
        var clock = WorldClock.Instance;

        timeText.text =
            $"{clock.CurrentHour:00}:{clock.CurrentMinute:00}";
    }

    void UpdateDayUI()
    {
        var clock = WorldClock.Instance;

        dayDropdown.SetValueWithoutNotify((int)clock.CurrentDayOfWeek);
    }

    void OnDayDropdownChanged(int index)
    {
        DayOfWeek target = (DayOfWeek)index;

        WorldClock.Instance.JumpToDay(target);
    }

    void OnSpeedChanged(float value)
    {
        WorldClock.Instance.SetTimeSpeed((TimeSpeed)value);
        NPCManager.Instance.SetNPCSpeed((NPCSpeed)value);
    }
}
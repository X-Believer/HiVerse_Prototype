using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;
    
    public List<Building> allBuildings = new List<Building>();

    private List<BuildingEvent> todayEvents = new List<BuildingEvent>();

    public static event Action<BuildingEvent> OnBuildingEventStarted;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        WorldClock.OnDayChanged += OnNewDay;
        WorldClock.OnMinuteChanged += CheckEvents;
    }

    void OnDisable()
    {
        WorldClock.OnDayChanged -= OnNewDay;
        WorldClock.OnMinuteChanged -= CheckEvents;
    }

    void OnNewDay()
    {
        LoadTodayEvents();
    }

    void LoadTodayEvents()
    {
        DayOfWeek day = WorldClock.Instance.CurrentDayOfWeek;

        string fileName = GetTodayEventFileName();

        string path = Path.Combine(
            Application.streamingAssetsPath,
            "CityEvents",
            fileName
        );

        if (!File.Exists(path))
        {
            Debug.LogWarning("CityEvent file not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);

        CityEventConfigList data =
            JsonUtility.FromJson<CityEventConfigList>(json);

        todayEvents.Clear();

        foreach (var cfg in data.events)
        {
            Building building = FindBuilding(cfg.buildingName);

            if (building == null)
                continue;

            BuildingEvent e = new BuildingEvent();

            e.eventName = cfg.eventName;
            e.eventDescription = cfg.eventDescription;
            e.eventBuilding = building;
            e.startTime = cfg.startTime;
            e.endTime = cfg.endTime;
            e.capacity = cfg.capacity;

            todayEvents.Add(e);
        }

        SendCityNews();
    }

    Building FindBuilding(string name)
    {
        return allBuildings.Find(b => b.buildingName == name);
    }

    void SendCityNews()
    {
        string message = "今日城市活动：\n";

        foreach (var e in todayEvents)
        {
            string time =
                $"{e.startTime / 60:00}:{e.startTime % 60:00}";

            message +=
                $"{time} - {e.eventBuilding.buildingName} : {e.eventName}\n";
        }

        NewsSource.Instance.AddCityNews(message);
    }

    void CheckEvents()
    {
        int currentTime = WorldClock.Instance.CurrentTotalMinutes % 1440;

        foreach (var e in todayEvents)
        {
            if (!e.started && currentTime >= e.startTime)
            {
                StartEvent(e);
            }
        }
    }

    void StartEvent(BuildingEvent e)
    {
        e.started = true;

        e.eventBuilding.currentEvent = e;

        string time =
            $"{WorldClock.Instance.CurrentHour:00}:{WorldClock.Instance.CurrentMinute:00}";

        string msg =
            $"{e.eventBuilding.buildingName} 正在举办 {e.eventName}，时间：{time}";

        NewsSource.Instance.AddCityNews(msg);

        OnBuildingEventStarted?.Invoke(e);
    }
    
    string GetTodayEventFileName()
    {
        string fileName =
            WorldClock.Instance.CurrentDayOfWeek
                .ToString()
                .ToLower() + ".json";

        return Path.Combine(
            Application.streamingAssetsPath,
            "CityEvents",
            fileName
        );
    }
}
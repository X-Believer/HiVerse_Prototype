using System;
using System.Collections.Generic;

[Serializable]
public class CityEventConfig
{
    public string eventName;
    public string eventDescription;
    public string buildingName;

    public int startTime;
    public int endTime;

    public int capacity;
}

[Serializable]
public class CityEventConfigList
{
    public List<CityEventConfig> events;
}
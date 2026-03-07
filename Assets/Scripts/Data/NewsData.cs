using System;

public enum NewsType
{
    News,       // 城市新闻
    Citizen     // 居民行为
}

[Serializable]
public class NewsData
{
    public NewsType type;
    public string message;
    public DateTime time;

    public NewsData(NewsType type, string message)
    {
        this.type = type;
        this.message = message;
        this.time = DateTime.Now;
    }
}
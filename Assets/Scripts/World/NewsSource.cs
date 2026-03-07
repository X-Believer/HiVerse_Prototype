using System;
using System.Collections.Generic;
using UnityEngine;

public class NewsSource : MonoBehaviour
{
    public static NewsSource Instance;

    public Action<NewsData> OnNewsAdded;

    private List<NewsData> newsList = new List<NewsData>();

    void Awake()
    {
        Instance = this;
    }

    public void AddCityNews(string message)
    {
        NewsData data = new NewsData(NewsType.News, message);
        AddNews(data);
    }

    public void AddCitizenAction(string name, string action)
    {
        string msg = $"[{name}] 正在 {action}";
        NewsData data = new NewsData(NewsType.Citizen, msg);
        AddNews(data);
    }

    void AddNews(NewsData data)
    {
        newsList.Add(data);
        OnNewsAdded?.Invoke(data);
    }

    public List<NewsData> GetAllNews()
    {
        return newsList;
    }
}
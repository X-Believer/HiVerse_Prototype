using UnityEngine;
using UnityEngine.UI;

public class NewsWindow : MonoBehaviour
{
    public Transform content;
    public GameObject newsItemPrefab;
    private ScrollRect _scrollRect;

    void Start()
    {
        _scrollRect = GetComponent<ScrollRect>();
        NewsSource.Instance.OnNewsAdded += OnNewsAdded;

        foreach (var news in NewsSource.Instance.GetAllNews())
        {
            CreateItem(news);
        }
    }

    void OnNewsAdded(NewsData data)
    {
        CreateItem(data);
        ScrollToBottom();
    }

    void CreateItem(NewsData data)
    {
        GameObject obj = Instantiate(newsItemPrefab, content);
        obj.GetComponent<NewsItem>().Setup(data);
    }

    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 0f;
    }
}
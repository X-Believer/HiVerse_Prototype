using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewsItem : MonoBehaviour
{
    public TextMeshProUGUI text;
    private static Image _background;

    public Color newsColor;
    public Color citizenColor;
    
    void Awake()
    {
        _background = GetComponent<Image>();
    }

    public void Setup(NewsData data)
    {
        text.text = data.message;

        if (data.type == NewsType.News)
            _background.color = newsColor;
        else
            _background.color = citizenColor;
    }
}
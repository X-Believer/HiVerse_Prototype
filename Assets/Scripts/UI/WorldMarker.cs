using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WorldMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;

    private Transform target;
    private string displayName;
    private Vector3 offset;

    public void Init(Transform target, string name, Sprite sprite, Vector3 offset)
    {
        this.target = target;
        this.displayName = name;
        this.offset = offset;

        icon.sprite = sprite;
    }

    void Start()
    {
        UIManager.Instance.RegisterMarker(this);
    }

    void Update()
    {
        if (target == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
        transform.position = screenPos;
    }
    
    void OnDestroy()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UnregisterMarker(this);
    }
    
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.ShowTooltip(displayName, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideTooltip();
    }
}
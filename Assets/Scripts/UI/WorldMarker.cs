using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WorldMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    private Transform target;
    private string displayName;
    private Vector3 offset;
    
    RectTransform rectTransform;
    Camera mainCamera;
    RectTransform canvasRect;

    public void Init(Transform target, string name, Sprite sprite, Vector3 offset)
    {
        this.target = target;
        this.displayName = name;
        this.offset = offset;
        icon.sprite = sprite;
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
        canvasRect = UIManager.Instance.worldCanvas;

        UIManager.Instance.RegisterMarker(this);
    }

    void Update()
    {
        if (target == null) return;

        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position + offset);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            mainCamera,
            out Vector2 localPoint
        );

        rectTransform.anchoredPosition = localPoint;
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
        UIManager.Instance.ShowTooltip(displayName, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideTooltip();
    }
}
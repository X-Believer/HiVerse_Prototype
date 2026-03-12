using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum MarkerType
{
    NPC,
    Building,
    Quest,
    Event,
    Room
}

public class WorldMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    private Button button;

    private Transform target;
    private string displayName;
    private Vector3 offset;

    RectTransform rectTransform;
    Camera mainCamera;
    RectTransform canvasRect;

    public Vector2 projectedPosition;
    public Vector2 finalPosition;

    public RectTransform Rect => rectTransform;
    public MarkerType markerType;
    private IMarkerTarget markerTarget;

    public void Init(IMarkerTarget target)
    {
        markerTarget = target;

        this.target = target.GetMarkerTransform();
        this.displayName = target.GetMarkerName();
        this.offset = target.GetMarkerOffset();
        this.markerType = target.GetMarkerType();

        icon.sprite = target.GetMarkerIcon();

        ApplyMarkerStyle();
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
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

        projectedPosition = localPoint;
    }

    public void ApplyFinalPosition()
    {
        rectTransform.anchoredPosition = finalPosition;
    }
    
    public void OnMarkerClicked()
    {
        switch (markerType)
        {
            case MarkerType.NPC:
                HandleNPCClick();
                break;

            case MarkerType.Building:
                HandleBuildingClick();
                break;
        }
        UIManager.Instance.HideTooltip();
    }
    void HandleNPCClick()
    {
        NPCController npc = markerTarget as NPCController;

        if (npc == null)
            return;

        NPCManager.Instance.SetCurrentNPC(npc);

        UIManager.Instance.OpenWorldPanelTab(0); 
    }
    
    void HandleBuildingClick()
    {
        Building building = markerTarget as Building;

        if (building == null)
            return;
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
    
    void ApplyMarkerStyle()
    {
        switch (markerType)
        {
            case MarkerType.NPC:
                icon.color = Color.cyan;
                break;

            case MarkerType.Building:
                icon.color = Color.yellow;
                break;

            case MarkerType.Quest:
                icon.color = Color.blue;
                break;

            case MarkerType.Event:
                icon.color = Color.red;
                break;

            case MarkerType.Room:
                icon.color = Color.green;
                break;

            default:
                icon.color = Color.white;
                break;
        }
    }
}
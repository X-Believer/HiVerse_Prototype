using RainbowArt.CleanFlatUI;
using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("GeneralUI")]
    public GameObject hud;
    public GameObject settingsPanel;
    public GameObject worldPanel;
    public GameObject backgroundMask;
    
    [Header("World UI")]
    public RectTransform worldCanvas;
    public WorldMarker markerPrefab;
    public Tooltip topdownTip;
    
    private List<WorldMarker> markers = new List<WorldMarker>();

    [Header("Player")]
    public GameObject playerCharacter;
    
    private ThirdPersonController _playerController;
    private TabView _settingsTabView;
    private TabView _worldTabView;

    /// UI栈
    private Stack<GameObject> uiStack = new Stack<GameObject>();

    void Awake()
    {
        Instance = this;

        _settingsTabView = settingsPanel.GetComponent<TabView>();
        _worldTabView = worldPanel.GetComponent<TabView>();
        _playerController = playerCharacter.GetComponent<ThirdPersonController>();

        backgroundMask.SetActive(false);
    }
    
    void OnEnable()
    {
        CameraManager.OnCameraModeChanged += OnCameraModeChanged;
    }

    void OnDisable()
    {
        CameraManager.OnCameraModeChanged -= OnCameraModeChanged;
    }

    
    void Update()
    {
        if (CameraManager.Instance.cameraMode == CameraMode.TopDown)
        {
            UpdateTopdownTipPosition();
        }
    }

    //================================================
    // 打开UI
    //================================================

    public void OpenUI(GameObject panel)
    {
        if (!panel.activeSelf)
            panel.SetActive(true);

        uiStack.Push(panel);

        OnMenuOpened();
    }

    //================================================
    // 关闭UI
    //================================================

    public void CloseUI(GameObject panel)
    {
        if (!panel.activeSelf)
            return;

        panel.SetActive(false);

        if (uiStack.Count > 0 && uiStack.Peek() == panel)
        {
            uiStack.Pop();
        }

        OnMenuClosed();
    }

    //================================================
    // 关闭最上层UI（用于点击空白区域）
    //================================================

    public void CloseTopUI()
    {
        if (uiStack.Count == 0)
            return;

        GameObject top = uiStack.Pop();
        top.SetActive(false);

        OnMenuClosed();
    }

    //================================================
    // 打开Settings指定页
    //================================================

    public void OpenSettingsTab(int index)
    {
        settingsPanel.SetActive(true);
        _settingsTabView.CurrentIndex = index;

        OpenUI(settingsPanel);
    }

    public void OpenSettings()
    {
        OpenUI(settingsPanel);
    }

    public void CloseSettings()
    {
        CloseUI(settingsPanel);
    }
    
    //================================================
    // 打开WorldPanel指定页
    //================================================

    public void OpenWorldPanelTab(int index)
    {
        worldPanel.SetActive(true);
        _worldTabView.CurrentIndex = index;

        OpenUI(worldPanel);
    }

    public void OpenWorldPanel()
    {
        OpenUI(worldPanel);
    }

    public void CloseWorldPanel()
    {
        CloseUI(worldPanel);
    }

    //================================================
    // UI状态控制
    //================================================

    void OnMenuOpened()
    {
        backgroundMask.SetActive(true);

        // 禁用角色控制
        _playerController.enabled = false;

        // 禁用HUD
        SetHUDInteractable(false);

        // 显示鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnMenuClosed()
    {
        if (uiStack.Count == 0)
        {
            backgroundMask.SetActive(false);

            // 恢复角色控制
            _playerController.enabled = true;

            SetHUDInteractable(true);

            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;
        }
    }

    void SetHUDInteractable(bool value)
    {
        CanvasGroup group = hud.GetComponent<CanvasGroup>();

        if (group != null)
        {
            group.interactable = value;
            group.blocksRaycasts = value;
        }
    }
    
    
    //================================================
    // Topdown Marker 控制
    //================================================
    public void RegisterMarker(WorldMarker marker)
    {
        if (!markers.Contains(marker))
        {
            markers.Add(marker);
        }
    }

    public void UnregisterMarker(WorldMarker marker)
    {
        if (markers.Contains(marker))
        {
            markers.Remove(marker);
        }
    }

    private void OnCameraModeChanged(CameraMode mode)
    {
        bool active = mode == CameraMode.TopDown;

        foreach (var marker in markers)
        {
            marker.SetVisible(active);
        }
    }
    
    void UpdateTopdownTipPosition()
    {
        if (topdownTip != null && topdownTip.gameObject.activeSelf)
        {
            topdownTip.SetTooltipPosition(Input.mousePosition, 0, 0);
        }
    }
    
    public WorldMarker CreateMarker(Transform target, string name, Sprite icon, Vector3 offset)
    {
        if (markerPrefab == null || worldCanvas == null)
        {
            Debug.LogWarning("WorldUI not configured.");
            return null;
        }

        WorldMarker marker = Instantiate(markerPrefab, worldCanvas);

        marker.Init(target, name, icon, offset);

        markers.Add(marker);

        return marker;
    }
    
    public void RemoveMarker(WorldMarker marker)
    {
        if (marker == null)
            return;

        if (markers.Contains(marker))
            markers.Remove(marker);

        Destroy(marker.gameObject);
    }
    
    public void ShowTooltip(string text, Vector2 screenPos)
    {
        if (topdownTip == null)
            return;

        topdownTip.DescriptionValue = text;

        RectTransform canvasRect = worldCanvas;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            Camera.main,
            out Vector2 localPoint
        );

        topdownTip.SetTooltipPosition(localPoint, 0, 0);

        topdownTip.ShowTooltip();
    }

    public void HideTooltip()
    {
        if (topdownTip == null)
            return;

        topdownTip.HideTooltip();
    }

    //================================================
    // 背景点击
    //================================================

    public void OnBackgroundClicked()
    {
        CloseTopUI();
    }

    //================================================
    // 退出游戏
    //================================================

    public void OnExitButtonClicked()
    {
        NotificationManager.Instance.ShowConfirm(
            "Exit Game",
            "Are you sure you want to quit?",
            QuitGame,
            null
        );
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
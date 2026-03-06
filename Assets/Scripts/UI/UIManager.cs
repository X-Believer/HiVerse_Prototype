using RainbowArt.CleanFlatUI;
using StarterAssets;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public const int TAB_USER = 0;
    public const int TAB_SETTINGS = 1;
    public const int TAB_AGENT = 2;
    
    public static UIManager Instance;

    public GameObject hud;
    public GameObject settingsPanel;
    public GameObject playerCharacter;
    private ThirdPersonController _playerController;
    private TabView _settingsTabView;

    bool menuOpen = false;

    void Awake()
    {
        Instance = this;
        _settingsTabView = settingsPanel.GetComponent<TabView>();
        _playerController = playerCharacter.GetComponent<ThirdPersonController>();
    }
    
    public void OpenSettingsTab(int index)
    {
        settingsPanel.SetActive(true);
        _settingsTabView.CurrentIndex = index;

        OnMenuOpened();
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        OnMenuOpened();
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        OnMenuClosed();
    }

    public void OnMenuOpened()
    {
        menuOpen = true;

        // 禁用角色控制
        _playerController.enabled = false;

        // 禁用HUD交互
        SetHUDInteractable(false);

        // 显示鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnMenuClosed()
    {
        menuOpen = false;

        _playerController.enabled = true;

        SetHUDInteractable(true);
    }

    void SetHUDInteractable(bool value)
    {
        CanvasGroup group = hud.GetComponent<CanvasGroup>();
        group.interactable = value;
        group.blocksRaycasts = value;
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
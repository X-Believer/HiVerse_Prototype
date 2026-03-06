using UnityEngine;
using RainbowArt.CleanFlatUI;

public class QuitConfirmController : MonoBehaviour
{
    public NotificationContentFitterWithButton notification;

    void Start()
    {
        notification.HideNotification();
        notification.OnFirst.AddListener(OnConfirmQuit);
        notification.OnCancel.AddListener(OnCancelQuit);
    }

    public void ShowQuitConfirm()
    {
        notification.TitleValue = "Exit Game";
        notification.DescriptionValue = "Are you sure you want to quit HiVerse?";
        notification.ShowTime = 999f; // 不自动关闭

        notification.ShowNotification();
    }

    void OnConfirmQuit()
    {
        QuitGame();
    }

    void OnCancelQuit()
    {
        notification.HideNotification();
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
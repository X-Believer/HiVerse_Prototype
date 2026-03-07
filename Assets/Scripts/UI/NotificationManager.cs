using RainbowArt.CleanFlatUI;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("Notifications")]
    public GameObject toastObject;
    public GameObject messageObject;
    public GameObject confirmObject;
    
    private NotificationContentFitterWithButton _toast;
    private NotificationContentFitterWithButton _message;
    private NotificationContentFitterWithButton _confirm;

    void Awake()
    {
        Instance = this;
        // _toast = toastObject.GetComponent<NotificationContentFitterWithButton>();
        // _message = messageObject.GetComponent<NotificationContentFitterWithButton>();
        _confirm = confirmObject.GetComponent<NotificationContentFitterWithButton>();
    }

    public void ShowToast(string msg)
    {
        _toast.DescriptionValue = msg;
        _toast.ShowNotification();
        UIManager.Instance.OpenUI(toastObject);
    }

    public void ShowMessage(string title, string msg)
    {
        _message.TitleValue = title;
        _message.DescriptionValue = msg;
        _message.ShowNotification();
        UIManager.Instance.OpenUI(messageObject);
    }

    public void ShowConfirm(string title, string msg, System.Action onConfirm, System.Action onCancel)
    {
        _confirm.TitleValue = title;
        _confirm.DescriptionValue = msg;

        // 清理旧事件
        _confirm.OnFirst.RemoveAllListeners();
        _confirm.OnCancel.RemoveAllListeners();

        // 重新绑定
        _confirm.OnFirst.AddListener(() =>
        {
            onCancel?.Invoke();
            HideConfirm();
        });

        _confirm.OnSecond.AddListener(() =>
        {
            onConfirm?.Invoke();
        });

        _confirm.ShowTime = 999f;
        _confirm.ShowNotification();
        UIManager.Instance.OpenUI(confirmObject);
    }

    public void HideConfirm()
    {
        UIManager.Instance.CloseUI(confirmObject);
        // _confirm.HideNotification();
    }
}
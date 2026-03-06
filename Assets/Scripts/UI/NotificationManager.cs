using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("Parent")]
    public Transform notificationRoot;

    [Header("Prefabs")]
    public GameObject toastPrefab;
    public GameObject messagePrefab;
    public GameObject confirmPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void ShowToast(string message)
    {
        GameObject obj = Instantiate(toastPrefab, notificationRoot);
        // obj.GetComponent<ToastNotification>().Init(message);
    }

    public void ShowMessage(string title, string message)
    {
        GameObject obj = Instantiate(messagePrefab, notificationRoot);
        // obj.GetComponent<MessageNotification>().Init(title, message);
    }

    public void ShowConfirm(string title, string message,
        System.Action onConfirm,
        System.Action onCancel = null)
    {
        GameObject obj = Instantiate(confirmPrefab, notificationRoot);

        // obj.GetComponent<ConfirmNotification>().Init(title, message, onConfirm, onCancel);
    }
}
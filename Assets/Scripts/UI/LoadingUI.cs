using UnityEngine;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;

    private void OnEnable()
    {
        ServerAPI.Instance.OnRequestStarted += ShowLoading;
        ServerAPI.Instance.OnRequestEnded += HideLoading;
    }

    private void OnDisable()
    {
        ServerAPI.Instance.OnRequestStarted -= ShowLoading;
        ServerAPI.Instance.OnRequestEnded -= HideLoading;
    }

    private void ShowLoading() => loadingPanel.SetActive(true);
    private void HideLoading() => loadingPanel.SetActive(false);
}
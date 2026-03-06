using UnityEngine;
using UnityEngine.EventSystems;

public class ClickOutsideClose : MonoBehaviour, IPointerClickHandler
{
    public GameObject panel;

    public void OnPointerClick(PointerEventData eventData)
    {
        panel.SetActive(false);
        UIManager.Instance.OnMenuClosed();
    }
}
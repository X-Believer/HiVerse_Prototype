using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundMask : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        UIManager.Instance.CloseTopUI();
    }
}
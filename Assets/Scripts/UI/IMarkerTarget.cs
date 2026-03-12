using UnityEngine;

public interface IMarkerTarget
{
    Transform GetMarkerTransform();
    string GetMarkerName();
    Sprite GetMarkerIcon();
    Vector3 GetMarkerOffset();
    MarkerType GetMarkerType();
}
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour, IMarkerTarget
{
    public string buildingName;
    public Sprite buildingIcon;

    public List<NPCController> npcsInside = new List<NPCController>();

    public BuildingEvent currentEvent;
    void Start()
    {
        UIManager.Instance.CreateMarker(this);
    }
    

    public void EnterBuilding(NPCController npc)
    {
        if (npcsInside.Contains(npc))
            return;

        npcsInside.Add(npc);
    }

    public void LeaveBuilding(NPCController npc)
    {
        npcsInside.Remove(npc);
    }
    
    
    // ======================
    // Marker Interface
    // ======================
    public Transform GetMarkerTransform()
    {
        return transform;
    }

    public string GetMarkerName()
    {
        return buildingName;
    }

    public Sprite GetMarkerIcon()
    {
        return buildingIcon;
    }

    public Vector3 GetMarkerOffset()
    {
        return Vector3.up * 4f;
    }
    
    public MarkerType GetMarkerType()
    {
        return MarkerType.Building;
    }
}
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public string buildingName;

    public List<NPCController> npcsInside = new List<NPCController>();

    public BuildingEvent currentEvent;

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
}
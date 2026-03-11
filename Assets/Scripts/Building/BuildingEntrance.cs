using UnityEngine;

public class BuildingEntrance : MonoBehaviour
{
    private Building _building;
    
    public Building building
    {
        get { return _building; }
    }
    
    void Awake()
    {
        _building = GetComponentInParent<Building>();
    }

    private void OnTriggerEnter(Collider other)
    {
        NPCController npc = other.GetComponent<NPCController>();

        if (npc == null)
            return;

        _building.EnterBuilding(npc);
    }

    private void OnTriggerExit(Collider other)
    {
        NPCController npc = other.GetComponent<NPCController>();

        if (npc == null)
            return;

        _building.LeaveBuilding(npc);
    }
}
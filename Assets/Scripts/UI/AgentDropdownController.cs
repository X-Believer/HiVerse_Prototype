using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AgentDropdownController : MonoBehaviour
{
    private TMP_Dropdown _agentDropdown;
    private List<Transform> _agentList = new List<Transform>();

    private void Awake()
    {
        _agentDropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        NPCManager.OnNPCListChanged += RefreshAgentList;
    }

    private void OnDisable()
    {
        NPCManager.OnNPCListChanged -= RefreshAgentList;
    }

    private void Start()
    {
        _agentDropdown.onValueChanged.AddListener(OnAgentSelected);
    }

    public void RefreshAgentList(List<NPCController> npcList)
    {
        _agentDropdown.ClearOptions();
        _agentList.Clear();

        List<string> names = new List<string> { "You" };

        foreach (var npc in npcList)
        {
            _agentList.Add(npc.transform);
            names.Add(npc.name);
        }

        _agentDropdown.AddOptions(names);
    }

    private void OnAgentSelected(int oldIndex)
    {
        int index = _agentDropdown.value;
        if (index < 0 || index > _agentList.Count) return;

        if (index == 0)
        {
            CameraManager.Instance.SwitchToPlayerCamera();
            return;
        }

        Transform selectedAgent = _agentList[index - 1];
        CameraManager.Instance.SwitchToNPCCamera(selectedAgent);
    }
}
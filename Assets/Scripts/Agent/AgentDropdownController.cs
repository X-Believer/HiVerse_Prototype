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
    private void Start()
    {
        RefreshAgentList();
        
        // Dropdown 事件监听
        _agentDropdown.onValueChanged.AddListener(OnAgentSelected);
    }

    /// <summary>
    /// 刷新所有 Agent 名称到 Dropdown
    /// </summary>
    public void RefreshAgentList()
    {
        _agentDropdown.ClearOptions();
        _agentList.Clear();

        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
        List<string> agentNames = new List<string>();
        
        agentNames.Add("You");

        foreach (GameObject Agent in agents)
        {
            _agentList.Add(Agent.transform);
            agentNames.Add(Agent.name);
        }

        _agentDropdown.AddOptions(agentNames);
    }

    /// <summary>
    /// Dropdown 选中 Agent
    /// </summary>
    /// <param name="oldIndex"></param>
    private void OnAgentSelected(int oldIndex)
    {
        int index = _agentDropdown.value;
        if (index < 0 || index >= _agentList.Count + 1) return;
        if (index == 0)
        {
            CameraManager.Instance.SwitchToPlayerCamera();
            return;
        }

        Transform selectedAgent = _agentList[index - 1];
        CameraManager.Instance.SwitchToNPCCamera(selectedAgent);
    }
}
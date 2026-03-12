using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AgentDropdownController : MonoBehaviour
{
    private TMP_Dropdown _agentDropdown;
    private List<NPCController> _agentList = new List<NPCController>();

    private void Awake()
    {
        _agentDropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        NPCManager.OnNPCListChanged += RefreshAgentList;
        NPCManager.OnCurrentNPCChanged += OnCurrentNPCChanged;
    }

    private void OnDisable()
    {
        NPCManager.OnNPCListChanged -= RefreshAgentList;
        NPCManager.OnCurrentNPCChanged -= OnCurrentNPCChanged;
    }

    private void Start()
    {
        _agentDropdown.onValueChanged.AddListener(OnAgentSelected);
    }

    // 刷新Dropdown列表
    public void RefreshAgentList(List<NPCController> npcList)
    {
        _agentDropdown.ClearOptions();
        _agentList.Clear();

        List<string> names = new List<string> { "You" };

        foreach (var npc in npcList)
        {
            _agentList.Add(npc);
            names.Add(npc.npcName);
        }

        _agentDropdown.AddOptions(names);

        // 刷新后同步选中项
        SyncDropdownWithCurrentNPC();
    }

    // 玩家或NPC被选中时
    private void OnAgentSelected(int oldIndex)
    {
        int index = _agentDropdown.value;

        if (index == 0)
        {
            CameraManager.Instance.SwitchToPlayerCamera();
            NPCManager.Instance.SetCurrentNPC(null);
            return;
        }

        if (index - 1 < 0 || index - 1 >= _agentList.Count)
            return;

        NPCController npc = _agentList[index - 1];
        CameraManager.Instance.SwitchToNPCCamera(npc.transform);
        NPCManager.Instance.SetCurrentNPC(npc);
    }

    // 当前NPC变化事件回调
    private void OnCurrentNPCChanged(NPCController npc)
    {
        SyncDropdownWithCurrentNPC();
    }

    // 同步Dropdown显示
    private void SyncDropdownWithCurrentNPC()
    {
        NPCController currentNPC = NPCManager.Instance.GetCurrentNPC();

        if (currentNPC == null)
        {
            _agentDropdown.value = 0; // 玩家
        }
        else
        {
            int index = _agentList.IndexOf(currentNPC);
            if (index >= 0)
                _agentDropdown.value = index + 1; // +1 因为0是玩家
        }
    }
}
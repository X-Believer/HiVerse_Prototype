using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WorldPanelController : MonoBehaviour
{
    public TextMeshProUGUI npcInfoText;
    public TextMeshProUGUI scheduleText;
    public Image npcAvatarImage;
    private Sprite npcAvatar;

    void OnEnable()
    {
        NPCManager.OnCurrentNPCChanged += OnNPCChanged;
        UpdateNPCInfo();
    }
    
    void OnDisable()
    {
        NPCManager.OnCurrentNPCChanged -= OnNPCChanged;
        UpdateNPCInfo();
    }
    
    void OnNPCChanged(NPCController npc)
    {
        UpdateNPCInfo();
    }

    void UpdateNPCInfo()
    {
        NPCController npc = NPCManager.Instance.GetCurrentNPC();

        if (npc != null)
        {
            npcInfoText.text = npc.GetNPCInfoText();
            scheduleText.text = npc.GetTodayScheduleText();
            npcAvatar = npc.icon;
            npcAvatarImage.sprite = npcAvatar;
        }
        else
        {
            npcInfoText.text = "No NPC selected";
            scheduleText.text = "";
        }
            
    }
}
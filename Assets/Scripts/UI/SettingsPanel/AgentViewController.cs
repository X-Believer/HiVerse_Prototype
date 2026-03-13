using System;
using System.Collections.Generic;
using RainbowArt.CleanFlatUI;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AgentViewController : MonoBehaviour
{
    [Header("NPC 输入字段")]
    public TMP_InputField nameField;
    public TMP_InputField jobField;
    public TMP_InputField mbtiField;
    public TMP_InputField descriptionField;
    public TMP_InputField behaviorDataField;
    public SwitchSimple genderSwitch; // 关=Male，开=Female

    [Header("按钮")]
    public Button useMyProfileButton;
    public Button generateButton;

    private void Start()
    {
        useMyProfileButton.onClick.AddListener(OnUseMyProfileClicked);
        generateButton.onClick.AddListener(OnGenerateClicked);

        // 初始化检查必填字段
        generateButton.interactable = false;

        // 监听输入变化
        nameField.onValueChanged.AddListener(OnInputFieldChanged);
        behaviorDataField.onValueChanged.AddListener(OnInputFieldChanged);
    }

    private void OnInputFieldChanged(string _)
    {
        // Name 和 BehaviorData 必填
        generateButton.interactable = !string.IsNullOrEmpty(nameField.text) && !string.IsNullOrEmpty(behaviorDataField.text);
    }

    private void OnUseMyProfileClicked()
    {
        if (UserManager.Instance == null) return;

        var user = UserManager.Instance.GetCurrentUser();
        if (user == null) return;

        nameField.text = user.username;
        jobField.text = user.job;
        mbtiField.text = user.mbti;
        descriptionField.text = user.description;
        behaviorDataField.text = user.username;
        genderSwitch.IsOn = (user.gender == Gender.Female);
    }

    private void OnGenerateClicked()
    {
        var requestData = new ServerAPI.NPCRequestData()
        {
            name = nameField.text,
            job = jobField.text,
            gender = genderSwitch.IsOn ? "Female" : "Male",
            mbti = mbtiField.text,
            description = descriptionField.text,
            behaviorUsername = behaviorDataField.text,
            creatorUsername = UserManager.Instance.GetCurrentUser().username
        };

        ServerAPI.Instance.GenerateAndDownloadNPC(requestData, (success, pathOrError) =>
        {
            if (success)
            {
                Debug.Log("NPC JSON 下载成功: " + pathOrError);
                // 获取 NPC 名称列表，这里只有一个新生成的 NPC
                List<string> npcNames = new List<string> { requestData.name };
                
                // 默认开始时间
                string startDate = "2026-01-01";

                // 调用生成并下载周计划
                ServerAPI.Instance.GenerateAndDownloadWeeklySchedules(npcNames, startDate, (weekSuccess, weekPaths) =>
                {
                    if (weekSuccess)
                    {
                        foreach (var kvp in weekPaths)
                            Debug.Log($"周计划生成成功: NPC {kvp.Key} 文件路径 {kvp.Value}");
                    }
                    else
                    {
                        Debug.LogError("周计划生成失败");
                    }
                });
            }
                
            else
                Debug.LogError("NPC生成失败: " + pathOrError);
        });
    }
}
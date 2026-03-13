using RainbowArt.CleanFlatUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileViewController : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_InputField usernameField;   // 用户名，不可修改
    public TMP_InputField jobField;
    public TMP_InputField mbtiField;
    public TMP_InputField descriptionField;
    public SwitchSimple genderSwitch;  // 关代表男，开代表女
    public Button modifyButton;
    public Image buttonImage;
    public TMP_Text buttonText;
    
    public Color saveButtonColor = Color.green; // Save按钮颜色

    private Color defaultButtonColor;
    private bool isEditing = false;

    void OnEnable()
    {
        RefreshUI();
        if (UserManager.Instance != null)
            UserManager.Instance.OnUserProfileChanged += OnUserProfileChanged;
    }

    void OnDisable()
    {
        if (UserManager.Instance != null)
            UserManager.Instance.OnUserProfileChanged -= OnUserProfileChanged;
    }

    private void OnUserProfileChanged(UserProfile profile)
    {
        RefreshUI();
    }

    void Start()
    {
        defaultButtonColor = buttonImage.color;
        modifyButton.onClick.AddListener(OnModifyButtonClicked);

        SetEditing(false); // 默认不可编辑
    }

    private void RefreshUI()
    {
        if (UserManager.Instance == null) return;
        var user = UserManager.Instance.GetCurrentUser();
        if (user == null) return;

        usernameField.text = user.username;
        usernameField.interactable = false;

        jobField.text = user.job;
        mbtiField.text = user.mbti;
        descriptionField.text = user.description;

        // 修正 Gender 逻辑：Switch 开 = Female，关 = Male
        genderSwitch.IsOn = (user.gender == Gender.Female);

        SetEditing(isEditing);
    }

    private void OnModifyButtonClicked()
    {
        if (!isEditing)
        {
            isEditing = true;
            buttonText.text = "Save";
            buttonImage.color = saveButtonColor;
            SetEditing(true);
        }
        else
        {
            SaveUserChanges();
            isEditing = false;
            buttonText.text = "Modify";
            buttonImage.color = defaultButtonColor;
            SetEditing(false);
        }
    }

    private void SetEditing(bool editable)
    {
        jobField.interactable = editable;
        mbtiField.interactable = editable;
        descriptionField.interactable = editable;
        genderSwitch.enabled = editable;
    }

    private void SaveUserChanges()
    {
        if (UserManager.Instance == null) return;

        // 修正 Gender 枚举保存逻辑
        Gender genderValue = genderSwitch.IsOn ? Gender.Female : Gender.Male;

        UserManager.Instance.UpdateUserProfile(
            job: jobField.text,
            mbti: mbtiField.text,
            description: descriptionField.text,
            gender: genderValue
        );

        Debug.Log("用户信息已保存！");
    }
}
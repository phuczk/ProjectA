using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class MainMenuUI : MonoBehaviour, IBackHandler
{
    [Header("Input Settings")]
    [SerializeField] private InputActionAsset _inputActions;
    private InputAction _backAction;

    [Header("Localized Texts (Optional)")]
    // Main Menu Buttons
    [SerializeField] private TextMeshProUGUI _playText;
    [SerializeField] private TextMeshProUGUI _settingText;
    [SerializeField] private TextMeshProUGUI _achievementText;
    [SerializeField] private TextMeshProUGUI _exitText;

    //Setting buttons
    [SerializeField] private TextMeshProUGUI _languageText;
    [SerializeField] private TextMeshProUGUI _graphicsText;
    [SerializeField] private TextMeshProUGUI _keyboardText;
    [SerializeField] private TextMeshProUGUI _audioText;

    [Header("Main Panels")]
    public GameObject StartUI;
    public GameObject SettingUI;
    public GameObject AchievementUI;
    public GameObject SlotPlayUI;

    [Header("Start UI Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingButton;
    [SerializeField] private Button _achievementButton;
    [SerializeField] private Button _exitButton;

    [Header("Setting UI - Options List")]
    public GameObject OptionButtonsRoot;
    public Button LanguageButton;
    public Button GraphicsButton;
    public Button KeyboardButton;
    public Button AudioButton;
    public Button BackSettingButton;

    [Header("Setting UI - Individual Contents")]
    public GameObject LanguageContent;
    public GameObject GraphicsContent;
    public GameObject KeyboardContent;
    public GameObject AudioContent;

    [Header("Achievement UI")]
    public Button BackAchievementButton;

    private void Awake()
    {
        var uiMap = _inputActions.FindActionMap("UI", true);
        _backAction = uiMap.FindAction("Cancel", true);

        _playButton.onClick.AddListener(() => OpenPanel(SlotPlayUI));
        _settingButton.onClick.AddListener(() => OpenPanel(SettingUI));
        _achievementButton.onClick.AddListener(() => OpenPanel(AchievementUI));
        _exitButton.onClick.AddListener(OnExitButtonClicked);

        LanguageButton.onClick.AddListener(() => SwitchSettingTab(LanguageContent));
        GraphicsButton.onClick.AddListener(() => SwitchSettingTab(GraphicsContent));
        KeyboardButton.onClick.AddListener(() => SwitchSettingTab(KeyboardContent));
        AudioButton.onClick.AddListener(() => SwitchSettingTab(AudioContent));
        
        BackSettingButton.onClick.AddListener(() => OnBack());
        BackAchievementButton.onClick.AddListener(() => OnBack());

        ShowStartUI();
        ApplyLocalization();
    }

    private void OnEnable() { _backAction.Enable(); _backAction.performed += OnBackPerformed; }
    private void OnDisable() { _backAction.Disable(); _backAction.performed -= OnBackPerformed; }
    private void OnBackPerformed(InputAction.CallbackContext context) => OnBack();

    private void ShowStartUI()
    {
        InternalCloseAll();
        StartUI.SetActive(true);
    }

    private void OpenPanel(GameObject targetPanel)
    {
        InternalCloseAll();
        targetPanel.SetActive(true);

        if (targetPanel == SettingUI)
        {
            OptionButtonsRoot.SetActive(true);
            HideAllSettingContents();
            EventSystem.current.SetSelectedGameObject(LanguageButton.gameObject);
        }
        else if (targetPanel == AchievementUI)
        {
            EventSystem.current.SetSelectedGameObject(BackAchievementButton.gameObject);
        }
    }

    private void SwitchSettingTab(GameObject targetContent)
    {
        OptionButtonsRoot.SetActive(false);
        
        HideAllSettingContents();
        targetContent.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null); 
    }

    private void HideAllSettingContents()
    {
        LanguageContent.SetActive(false);
        GraphicsContent.SetActive(false);
        KeyboardContent.SetActive(false);
        AudioContent.SetActive(false);
    }

    public bool OnBack()
    {
        if (SettingUI.activeSelf && !OptionButtonsRoot.activeSelf)
        {
            HideAllSettingContents();
            OptionButtonsRoot.SetActive(true);
            EventSystem.current.SetSelectedGameObject(LanguageButton.gameObject);
            return true;
        }

        if (!StartUI.activeSelf)
        {
            ShowStartUI();
            return true;
        }

        return false;
    }

    private void InternalCloseAll()
    {
        StartUI.SetActive(false);
        SettingUI.SetActive(false);
        AchievementUI.SetActive(false);
        SlotPlayUI.SetActive(false);
    }

    private void OnExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyLocalization();
    }
#endif
    private void ApplyLocalization()
    {
        if (_playText) _playText.text = Localization.Get("menu.start");
        if (_settingText) _settingText.text = Localization.Get("menu.options.title");
        if (_exitText) _exitText.text = Localization.Get("menu.exit");
        if (_achievementText) _achievementText.text = Localization.Get("menu.achievement.title");
        if (_languageText) _languageText.text = Localization.Get("menu.options.language");
        if (_graphicsText) _graphicsText.text = Localization.Get("menu.options.graphic");
        if (_keyboardText) _keyboardText.text = Localization.Get("menu.options.keyboard");
        if (_audioText) _audioText.text = Localization.Get("menu.options.audio");
    }
}
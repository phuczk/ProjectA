using UnityEngine;
using GlobalEnums;

public class GameBootstrap : Singleton<GameBootstrap>
{
    [Header("UI JSON")]
    [SerializeField] private TextAsset viUIJson;
    [SerializeField] private TextAsset enUIJson;

    [Header("Dialogue JSON")]
    [SerializeField] private TextAsset viMainDialogueJson;
    [SerializeField] private TextAsset enMainDialogueJson;
    [SerializeField] private TextAsset viSubDialogueJson;
    [SerializeField] private TextAsset enSubDialogueJson;

    private const string LANG_KEY = "SelectedLanguage";
    public Language currentLanguage = Language.Vietnamese;

    public static System.Action OnLanguageChanged;

    protected override void Awake()
    {
        base.Awake();
        int savedLang = PlayerPrefs.GetInt(LANG_KEY, (int)Language.Vietnamese);
        currentLanguage = (Language)savedLang;
        
        ReloadAllLocalization();
    }

    public void ReloadAllLocalization()
    {
        string uiJson = (currentLanguage == Language.Vietnamese) ? viUIJson.text : enUIJson.text;
        Localization.LoadFromJson(uiJson, true);
    }

    public void LoadMainDialogue()
    {
        string diagJson = (currentLanguage == Language.Vietnamese) ? viMainDialogueJson.text : enMainDialogueJson.text;
        Localization.LoadFromJson(diagJson, false);
    }

    public void LoadSubDialogue()
    {
        string subJson = (currentLanguage == Language.Vietnamese) ? viSubDialogueJson.text : enSubDialogueJson.text;
        Localization.LoadFromJson(subJson, false); 
    }

    public void ChangeLanguage(Language language)
    {
        currentLanguage = language;
        PlayerPrefs.SetInt(LANG_KEY, (int)language);
        
        ReloadAllLocalization();

        OnLanguageChanged?.Invoke();

        var context = FindFirstObjectByType<SceneLocalizationContext>();
        context?.LoadContext();
    }
}

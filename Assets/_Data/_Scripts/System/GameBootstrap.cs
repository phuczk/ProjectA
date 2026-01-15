using UnityEngine;
using GlobalEnums;

public class GameBootstrap : Singleton<GameBootstrap>
{
    [SerializeField] TextAsset viJson;
    [SerializeField] TextAsset enJson;
    
    private const string LANG_KEY = "SelectedLanguage";
    public Language currentLanguage = Language.Vietnamese;

    protected override void Awake()
    {
        base.Awake();
        LoadLanguageSettings();
    }

    private void LoadLanguageSettings()
    {
        // 1. Kiểm tra xem đã từng lưu ngôn ngữ chưa, nếu chưa mặc định là Vietnamese (0)
        int savedLang = PlayerPrefs.GetInt(LANG_KEY, (int)Language.Vietnamese);
        currentLanguage = (Language)savedLang;

        // 2. Load dữ liệu Json tương ứng
        string jsonContent = (currentLanguage == Language.Vietnamese) ? viJson.text : enJson.text;
        Localization.LoadFromJson(jsonContent);
        
        Debug.Log($"[GameBootstrap] Loaded Language: {currentLanguage}");
    }

    public void ChangeLanguage(Language language)
    {
        // 1. Cập nhật biến hiện tại
        currentLanguage = language;

        // 2. Load lại dữ liệu Localization
        string jsonContent = (language == Language.Vietnamese) ? viJson.text : enJson.text;
        Localization.LoadFromJson(jsonContent);

        // 3. LƯU VÀO PLAYERPREFS
        PlayerPrefs.SetInt(LANG_KEY, (int)language);
        PlayerPrefs.Save(); // Đảm bảo dữ liệu được ghi xuống đĩa ngay lập tức

        Debug.Log($"[GameBootstrap] Changed & Saved Language: {language}");
    }

    public string GetLocalizedString(string key)
    {
        return Localization.Get(key);
    }
}
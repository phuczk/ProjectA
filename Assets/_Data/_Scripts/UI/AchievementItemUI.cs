using GlobalEnums;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementItemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public GameObject lockOverlay;

    private AchievementData _data;

    public void Setup(AchievementData data, bool isUnlocked)
    {
        _data = data;
        iconImage.sprite = data.icon;
        lockOverlay.SetActive(!isUnlocked);
        
        UpdateText();
        
        // Đăng ký đổi ngôn ngữ thời gian thực
        GameBootstrap.OnLanguageChanged += UpdateText;
    }

    private void UpdateText()
    {
        if (_data == null) return;
        titleText.text = Localization.Get(_data.titleKey);
        descText.text = Localization.Get(_data.descKey);
    }

    private void OnDestroy() => GameBootstrap.OnLanguageChanged -= UpdateText;
}
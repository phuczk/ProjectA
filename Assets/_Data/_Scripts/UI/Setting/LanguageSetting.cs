using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GlobalEnums;

public class LanguageSetting : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _languageText;
    [SerializeField] private Button _nextLanguageButton;
    [SerializeField] private Button _prevLanguageButton;

    private void Awake()
    {
        _nextLanguageButton.onClick.AddListener(() =>
        {
            var values = System.Enum.GetValues(typeof(Language));
            int currentIndex = System.Array.IndexOf(values, GameBootstrap.Instance.currentLanguage);
            int nextIndex = (currentIndex + 1) % values.Length;
            Language nextLang = (Language)values.GetValue(nextIndex);
                
            GameBootstrap.Instance.ChangeLanguage(nextLang);
            UpdateLanguageText();
        });
        
        _prevLanguageButton.onClick.AddListener(() =>
        {
            var values = System.Enum.GetValues(typeof(Language));
            int currentIndex = System.Array.IndexOf(values, GameBootstrap.Instance.currentLanguage);
            int prevIndex = (currentIndex - 1 + values.Length) % values.Length;
            Language prevLang = (Language)values.GetValue(prevIndex);
                
            GameBootstrap.Instance.ChangeLanguage(prevLang);
            UpdateLanguageText();
        });
    }

    private void Start()
    {
        UpdateLanguageText();
    }

    private void UpdateLanguageText()
    {
        _languageText.text = GameBootstrap.Instance.currentLanguage.ToString();
    }
}
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class LanguageSetting : MonoBehaviour
// {
//     [SerializeField] private TextMeshProUGUI _languageText;
//     [SerializeField] private Button _languageButton;

//     private void Awake()
//     {
//         _languageButton.onClick.AddListener(() =>
//         {
//             LanguageManager.Instance.ChangeLanguage();
//             _languageText.text = LanguageManager.Instance.GetCurrentLanguage();
//         });
//     }

//     private void Start()
//     {
//         _languageText.text = LanguageManager.Instance.GetCurrentLanguage();
//     }
// }
using TMPro;
using UnityEngine;

public class LocalizeText : MonoBehaviour
{
    public string key; // Nhập key vào đây (vd: menu.start)
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        GameBootstrap.OnLanguageChanged += Refresh;
    }

    private void Start() => Refresh();

    private void OnDestroy() => GameBootstrap.OnLanguageChanged -= Refresh;

    public void Refresh()
    {
        if (_text != null) _text.text = Localization.Get(key);
    }
}
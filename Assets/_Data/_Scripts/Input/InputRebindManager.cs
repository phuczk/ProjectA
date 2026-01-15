using UnityEngine;
using UnityEngine.InputSystem;

public class InputRebindManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private const string REBINDS_KEY = "rebinds";

    private static InputRebindManager _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadRebinds();
    }

    public void SaveRebinds()
    {
        if (inputActions == null) return;

        var json = inputActions.SaveBindingOverridesAsJson();
        if (!string.IsNullOrEmpty(json))
        {
            PlayerPrefs.SetString(REBINDS_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("[Rebind] Đã lưu input overrides");
        }
    }

    private void LoadRebinds()
    {
        if (inputActions == null) return;

        var json = PlayerPrefs.GetString(REBINDS_KEY);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                inputActions.LoadBindingOverridesFromJson(json);
                Debug.Log("[Rebind] Đã load input overrides từ PlayerPrefs");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Rebind] Lỗi khi load overrides: " + e.Message);
                PlayerPrefs.DeleteKey(REBINDS_KEY);
            }
        }
        else
        {
            Debug.Log("[Rebind] Không tìm thấy overrides lưu, dùng mặc định");
        }
    }

    public void ResetToDefault()
    {
        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(REBINDS_KEY);
        PlayerPrefs.Save();
        Debug.Log("[Rebind] Đã reset về input mặc định");
    }
}
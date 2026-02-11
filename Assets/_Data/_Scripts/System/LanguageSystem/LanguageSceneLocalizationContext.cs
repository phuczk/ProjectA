using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SceneLocalizationContext : MonoBehaviour
{
    [Header("JSON Resources")]
    public TextAsset viJson;
    public TextAsset enJson;

    private HashSet<string> _loadedKeys = new HashSet<string>();

    void Start()
    {
        LoadContext();
    }

    public void LoadContext()
    {
        NPC[] allNPCs = FindObjectsByType<NPC>(FindObjectsSortMode.None);
        
        _loadedKeys = allNPCs.Select(npc => npc.GetNPCKey())
                             .Where(key => !string.IsNullOrEmpty(key))
                             .ToHashSet();
        
        if (_loadedKeys.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        string content = (GameBootstrap.Instance.currentLanguage == GlobalEnums.Language.Vietnamese) 
            ? viJson.text 
            : enJson.text;

        if (!string.IsNullOrEmpty(content))
        {
            foreach (var key in _loadedKeys)
            {
                Localization.LoadScope(key, content);
                Debug.Log($"[SceneContext] Auto-detected and Loaded: {key}");
            }
        }
    }

    void OnDestroy()
    {
        foreach (var key in _loadedKeys)
        {
            Localization.UnloadScope(key);
            Debug.Log($"[SceneContext] Auto-unloaded: {key}");
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UISelectionManager : MonoBehaviour
{
    public static UISelectionManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool _isDynamic = true;

    public GameObject[] UIObjects;

    public GameObject LastSelected { get; set; }
    public int CurrentIndex { get; set; } = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        StopAllCoroutines();

        if (_isDynamic)
        {
            StartCoroutine(WaitAndInitialize());
        }
        else
        {
            CleanMissingReferences();
            if (UIObjects != null && UIObjects.Length > 0)
            {
                StartCoroutine(SetSelectedAfterOneFrame());
            }
        }
    }

    private void CleanMissingReferences()
    {
        if (UIObjects == null) return;
        UIObjects = UIObjects.Where(obj => obj != null && obj.activeInHierarchy).ToArray();
    }

    private IEnumerator WaitAndInitialize()
    {
        yield return new WaitForEndOfFrame();
        InitializeFromChildren();

        if (UIObjects != null && UIObjects.Length > 0 && UIObjects[0] != null && UIObjects[0].activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(UIObjects[0]);
            LastSelected = UIObjects[0];
            CurrentIndex = 0;
        }
    }

    public void InitializeFromChildren()
    {
        var allHandlers = GetComponentsInChildren<UISelectionHandler>(true).ToList();
        if (allHandlers.Count > 0)
        {
            UIObjects = allHandlers.Where(h => h.gameObject.activeInHierarchy).Select(h => h.gameObject).ToArray();
            
            foreach (var handler in allHandlers)
            {
                handler.ResetScaleState();
            }
            
            // 🔥 FIXED: Cần gọi SetSelectedAfterOneFrame để thực sự select object
            if (UIObjects.Length > 0)
            {
                StartCoroutine(SetSelectedAfterOneFrame());
            }
        }
    }

    private IEnumerator SetSelectedAfterOneFrame()
    {
        yield return null;
        
        CleanMissingReferences();
        
        if (UIObjects.Length > 0 && UIObjects[0] != null && UIObjects[0].activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(UIObjects[0]);
            LastSelected = UIObjects[0];
        }
    }

    private void HandleNextUI(int addition)
    {
        int nextIndex = Mathf.Clamp(CurrentIndex + addition, 0, UIObjects.Length - 1);

        if (UIObjects[nextIndex] != null)
        {
            EventSystem.current.SetSelectedGameObject(UIObjects[nextIndex]);
        }
    }

    public void UpdateCurrentIndex(GameObject selected)
    {
        if (UIObjects == null) return;
        for (int i = 0; i < UIObjects.Length; i++)
        {
            if (UIObjects[i] == selected) { CurrentIndex = i; break; }
        }
    }
}

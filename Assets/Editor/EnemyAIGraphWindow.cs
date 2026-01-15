#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyAIGraphWindow : EditorWindow
{
    private EnemyAIGraphView _graphView;
    private EnemyUniversalMachine _selectedMachine;

    [MenuItem("Tools/Enemy AI Graph Editor")]
    public static void Open() => GetWindow<EnemyAIGraphWindow>("AI Graph");

    private void OnEnable()
    {
        _graphView = new EnemyAIGraphView();
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void OnSelectionChange()
{
    var machine = Selection.activeGameObject?.GetComponent<EnemyUniversalMachine>();
    
    // Nếu chọn trúng vật thể không có Machine, không làm gì cả hoặc xóa Graph
    if (machine != null)
    {
        _selectedMachine = machine;
        _graphView.Load(_selectedMachine);
    }
}
}
#endif

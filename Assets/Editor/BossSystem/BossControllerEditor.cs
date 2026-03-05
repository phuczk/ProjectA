#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// The BossAIGraphWindow is in a different namespace/file
// We need to reference it properly

[CustomEditor(typeof(BossController))]
public class BossControllerEditor : Editor
{
    private BossController _controller;
    private bool _showRuntimeInfo = true;
    private bool _showAIData = true;
    
    private void OnEnable()
    {
        _controller = (BossController)target;
    }
    
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Boss AI Controller", EditorStyles.boldLabel);
        
        // Runtime Info
        _showRuntimeInfo = EditorGUILayout.Foldout(_showRuntimeInfo, "Runtime Info");
        if (_showRuntimeInfo)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"Current Node: {_controller.GetCurrentNodeName()}");
            EditorGUILayout.LabelField($"Is Running: {_controller.IsRunning}");
            EditorGUILayout.LabelField($"Total Nodes: {_controller.StateNodes.Count}");
            
            EditorGUILayout.Space();
            
            // AI Control Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start AI"))
            {
                _controller.StartAI();
            }
            if (GUILayout.Button("Stop AI"))
            {
                _controller.StopAI();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        // AI Data
        _showAIData = EditorGUILayout.Foldout(_showAIData, "State Nodes");
        if (_showAIData)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField($"State Nodes Count: {_controller.StateNodes.Count}");
            EditorGUILayout.LabelField($"Current State: {_controller.GetCurrentNodeName()}");
            EditorGUILayout.LabelField($"Previous State: {_controller.PreviousState?.GetType().Name ?? "None"}");
            
            // Show nodes list
            if (_controller.StateNodes.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Nodes:", EditorStyles.boldLabel);
                for (int i = 0; i < _controller.StateNodes.Count; i++)
                {
                    var node = _controller.StateNodes[i];
                    EditorGUILayout.LabelField($"  {i}: {node?.GetType().Name} ({node?.Guid.Substring(0, 8)}...)");
                }
            }
            
            EditorGUI.indentLevel--;
        }
        
        // Quick test buttons
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Tests", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Test Start"))
        {
            _controller.TestStartAI();
        }
        if (GUILayout.Button("Test Stop"))
        {
            _controller.TestStopAI();
        }
        EditorGUILayout.EndHorizontal();
        
        // Open graph editor button
        if (GUILayout.Button("Open Graph Editor"))
        {
            BossAIGraphWindow.ShowWindow();
        }
    }
}
#endif

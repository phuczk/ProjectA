#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(BossController), true)]
public class BossNodeEditor : Editor
{
    private BossController _bossController;
    private bool _showNodeDetails = true;
    
    private void OnEnable()
    {
        _bossController = (BossController)target;
    }
    
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Node Connections", EditorStyles.boldLabel);
        
        _showNodeDetails = EditorGUILayout.Foldout(_showNodeDetails, "Node Details");
        if (_showNodeDetails)
        {
            EditorGUI.indentLevel++;
            DrawNodeConnections();
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawNodeConnections()
    {
        if (_bossController.StateNodes == null || _bossController.StateNodes.Count == 0)
        {
            EditorGUILayout.HelpBox("No state nodes found.", MessageType.Info);
            return;
        }
        
        for (int i = 0; i < _bossController.StateNodes.Count; i++)
        {
            var node = _bossController.StateNodes[i];
            if (node == null) continue;
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField($"Node {i}: {node.GetType().Name}", EditorStyles.boldLabel);
            
            // Show GUID
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("GUID", node.Guid);
            EditorGUI.EndDisabledGroup();
            
            // Next Node GUID with dropdown
            DrawNextNodeDropdown(node, i);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
    
    private void DrawNextNodeDropdown(BossNode node, int nodeIndex)
    {
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField("Next Node", GUILayout.Width(80));
        
        // Get current index
        int currentIndex = -1;
        if (!string.IsNullOrEmpty(node.NextNodeGuid))
        {
            for (int i = 0; i < _bossController.StateNodes.Count; i++)
            {
                if (_bossController.StateNodes[i].Guid == node.NextNodeGuid)
                {
                    currentIndex = i;
                    break;
                }
            }
        }
        
        // Create options
        string[] options = new string[_bossController.StateNodes.Count + 1];
        options[0] = "None";
        for (int i = 0; i < _bossController.StateNodes.Count; i++)
        {
            var targetNode = _bossController.StateNodes[i];
            options[i + 1] = $"{i}: {targetNode.GetType().Name}";
        }
        
        // Dropdown
        int newIndex = EditorGUILayout.Popup(currentIndex + 1, options);
        
        // Update NextNodeGuid
        if (newIndex == 0)
        {
            node.NextNodeGuid = "";
        }
        else if (newIndex - 1 != currentIndex)
        {
            node.NextNodeGuid = _bossController.StateNodes[newIndex - 1].Guid;
            EditorUtility.SetDirty(_bossController);
        }
        
        EditorGUILayout.EndHorizontal();
    }
}
#endif

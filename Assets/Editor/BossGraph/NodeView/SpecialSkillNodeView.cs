#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class SpecialSkillNodeView : SkillNodeView
{
    private SpecialSkillNode _specialNode;
    
    public SpecialSkillNodeView(SpecialSkillNode node, BossController machine) : base(node, machine)
    {
        _specialNode = node;
        title = "Special Skill";
        
        // Recreate inspector with special-specific properties
        extensionContainer.Clear();
        CreateSpecialInspector();
        
        RefreshExpandedState();
    }
    
    protected void CreateSpecialInspector()
    {
        IMGUIContainer inspector = new IMGUIContainer(() =>
        {
            EditorGUILayout.LabelField("Special Skill", EditorStyles.boldLabel);
            
            // Base Settings
            EditorGUILayout.LabelField("Base Settings", EditorStyles.boldLabel);
            _specialNode.Weight = EditorGUILayout.FloatField("Weight", _specialNode.Weight);
            _specialNode.NextNodeGuid = EditorGUILayout.TextField("Next Node GUID", _specialNode.NextNodeGuid);
            
            // Special Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Special Settings", EditorStyles.boldLabel);
            _specialNode.SpecialType = (SpecialSkillType)EditorGUILayout.EnumPopup("Special Type", _specialNode.SpecialType);
            _specialNode.Duration = EditorGUILayout.FloatField("Duration", _specialNode.Duration);
            _specialNode.Cooldown = EditorGUILayout.FloatField("Cooldown", _specialNode.Cooldown);
            _specialNode.EffectRadius = EditorGUILayout.FloatField("Effect Radius", _specialNode.EffectRadius);
            _specialNode.EffectPrefab = (GameObject)EditorGUILayout.ObjectField("Effect Prefab", _specialNode.EffectPrefab, typeof(GameObject), false);
            _specialNode.SpecialSound = (AudioClip)EditorGUILayout.ObjectField("Special Sound", _specialNode.SpecialSound, typeof(AudioClip), false);
            
            // Animation Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            _specialNode.AnimationName = EditorGUILayout.TextField("Animation Name", _specialNode.AnimationName);
            _specialNode.UseCustomAnimation = EditorGUILayout.Toggle("Use Custom Animation", _specialNode.UseCustomAnimation);
            _specialNode.AnimationType = (BossAnimationType)EditorGUILayout.EnumPopup("Animation Type", _specialNode.AnimationType);
            _specialNode.AttackVariant = EditorGUILayout.IntField("Attack Variant", _specialNode.AttackVariant);
            _specialNode.DelayAnimation = EditorGUILayout.FloatField("Delay Animation", _specialNode.DelayAnimation);
            
            // Entry Conditions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Entry Conditions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_specialNode.EntryConditions.Count}", EditorStyles.helpBox);
            
            // Transitions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Next Node: {(string.IsNullOrEmpty(_specialNode.NextNodeGuid) ? "None" : _specialNode.NextNodeGuid)}", EditorStyles.helpBox);
            
            if (GUI.changed)
                if (_machine != null)
                    EditorUtility.SetDirty(_machine);
        });
        
        extensionContainer.Add(inspector);
    }
}
#endif

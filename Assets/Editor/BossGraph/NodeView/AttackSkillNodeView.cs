#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class AttackSkillNodeView : SkillNodeView
{
    private AttackSkillNode _attackNode;
    
    public AttackSkillNodeView(AttackSkillNode node, BossController machine) : base(node, machine)
    {
        _attackNode = node;
        title = "Attack Skill";
        
        // Recreate inspector with attack-specific properties
        extensionContainer.Clear();
        CreateAttackInspector();
        
        RefreshExpandedState();
    }
    
    protected void CreateAttackInspector()
    {
        IMGUIContainer inspector = new IMGUIContainer(() =>
        {
            EditorGUILayout.LabelField("Attack Skill", EditorStyles.boldLabel);
            
            // Base Settings
            EditorGUILayout.LabelField("Base Settings", EditorStyles.boldLabel);
            _attackNode.Weight = EditorGUILayout.FloatField("Weight", _attackNode.Weight);
            _attackNode.NextNodeGuid = EditorGUILayout.TextField("Next Node GUID", _attackNode.NextNodeGuid);
            
            // Attack Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Attack Settings", EditorStyles.boldLabel);
            _attackNode.Damage = EditorGUILayout.IntField("Damage", _attackNode.Damage);
            _attackNode.AttackRange = EditorGUILayout.FloatField("Attack Range", _attackNode.AttackRange);
            _attackNode.AttackCooldown = EditorGUILayout.FloatField("Attack Cooldown", _attackNode.AttackCooldown);
            _attackNode.TargetLayer = EditorGUILayout.LayerField("Target Layer", _attackNode.TargetLayer);
            _attackNode.AttackOffset = EditorGUILayout.Vector3Field("Attack Offset", _attackNode.AttackOffset);
            _attackNode.UseAttackAnimation = EditorGUILayout.Toggle("Use Attack Animation", _attackNode.UseAttackAnimation);
            
            // Animation Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            _attackNode.AnimationName = EditorGUILayout.TextField("Animation Name", _attackNode.AnimationName);
            _attackNode.UseCustomAnimation = EditorGUILayout.Toggle("Use Custom Animation", _attackNode.UseCustomAnimation);
            _attackNode.AnimationType = (BossAnimationType)EditorGUILayout.EnumPopup("Animation Type", _attackNode.AnimationType);
            _attackNode.AttackVariant = EditorGUILayout.IntField("Attack Variant", _attackNode.AttackVariant);
            _attackNode.DelayAnimation = EditorGUILayout.FloatField("Delay Animation", _attackNode.DelayAnimation);
            
            // Entry Conditions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Entry Conditions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_attackNode.EntryConditions.Count}", EditorStyles.helpBox);
            
            // Transitions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_attackNode.Transitions.Count}", EditorStyles.helpBox);
            
            if (GUI.changed)
                if (_machine != null)
                    EditorUtility.SetDirty(_machine);
        });
        
        extensionContainer.Add(inspector);
    }
    
    public AttackSkillNode GetNode()
    {
        return _attackNode;
    }
}
#endif

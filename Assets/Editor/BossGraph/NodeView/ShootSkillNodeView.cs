#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class ShootSkillNodeView : SkillNodeView
{
    private ShootSkillNode _shootNode;
    
    public ShootSkillNodeView(ShootSkillNode node, BossController machine) : base(node, machine)
    {
        _shootNode = node;
        title = "Shoot Skill";
        
        // Recreate inspector with shoot-specific properties
        extensionContainer.Clear();
        CreateShootInspector();
        
        RefreshExpandedState();
    }
    
    protected void CreateShootInspector()
    {
        IMGUIContainer inspector = new IMGUIContainer(() =>
        {
            EditorGUILayout.LabelField("Shoot Skill", EditorStyles.boldLabel);
            
            // Base Settings
            EditorGUILayout.LabelField("Base Settings", EditorStyles.boldLabel);
            _shootNode.Weight = EditorGUILayout.FloatField("Weight", _shootNode.Weight);
            _shootNode.NextNodeGuid = EditorGUILayout.TextField("Next Node GUID", _shootNode.NextNodeGuid);
            
            // Shoot Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shoot Settings", EditorStyles.boldLabel);
            _shootNode.BulletPrefab = (GameObject)EditorGUILayout.ObjectField("Bullet Prefab", _shootNode.BulletPrefab, typeof(GameObject), false);
            _shootNode.BulletSpeed = EditorGUILayout.FloatField("Bullet Speed", _shootNode.BulletSpeed);
            _shootNode.BulletDamage = EditorGUILayout.FloatField("Bullet Damage", _shootNode.BulletDamage);
            _shootNode.FireRate = EditorGUILayout.FloatField("Fire Rate", _shootNode.FireRate);
            _shootNode.BurstCount = EditorGUILayout.IntField("Burst Count", _shootNode.BurstCount);
            _shootNode.BurstDelay = EditorGUILayout.FloatField("Burst Delay", _shootNode.BurstDelay);
            _shootNode.ShootOffset = EditorGUILayout.Vector3Field("Shoot Offset", _shootNode.ShootOffset);
            _shootNode.AimAtPlayer = EditorGUILayout.Toggle("Aim At Player", _shootNode.AimAtPlayer);
            
            // Animation Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            _shootNode.AnimationName = EditorGUILayout.TextField("Animation Name", _shootNode.AnimationName);
            _shootNode.UseCustomAnimation = EditorGUILayout.Toggle("Use Custom Animation", _shootNode.UseCustomAnimation);
            _shootNode.AnimationType = (BossAnimationType)EditorGUILayout.EnumPopup("Animation Type", _shootNode.AnimationType);
            _shootNode.AttackVariant = EditorGUILayout.IntField("Attack Variant", _shootNode.AttackVariant);
            _shootNode.DelayAnimation = EditorGUILayout.FloatField("Delay Animation", _shootNode.DelayAnimation);
            
            // Entry Conditions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Entry Conditions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_shootNode.EntryConditions.Count}", EditorStyles.helpBox);
            
            // Transitions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Next Node: {(string.IsNullOrEmpty(_shootNode.NextNodeGuid) ? "None" : _shootNode.NextNodeGuid)}", EditorStyles.helpBox);
            
            if (GUI.changed)
                if (_machine != null)
                    EditorUtility.SetDirty(_machine);
        });
        
        extensionContainer.Add(inspector);
    }
}
#endif

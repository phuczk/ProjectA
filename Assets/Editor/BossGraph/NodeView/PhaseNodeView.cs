#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class PhaseNodeView : Node
{
    public Port Input;
    public Port Output;

    private PhaseNode _node;
    private BossController _machine;

    public PhaseNodeView(PhaseNode node, BossController machine)
    {
        _node = node;
        _machine = machine;

        title = "Phase";

        viewDataKey = node.Guid;

        style.left = node.GraphPosition.x;
        style.top = node.GraphPosition.y;

        Input = InstantiatePort(
            Orientation.Horizontal,
            Direction.Input,
            Port.Capacity.Multi,
            typeof(bool));

        Input.portName = "In";

        inputContainer.Add(Input);

        Output = InstantiatePort(
            Orientation.Horizontal,
            Direction.Output,
            Port.Capacity.Single,
            typeof(bool));

        Output.portName = "Next";

        outputContainer.Add(Output);

        IMGUIContainer inspector = new IMGUIContainer(() =>
        {
            EditorGUILayout.LabelField("Phase Settings", EditorStyles.boldLabel);

            _node.PhaseName = EditorGUILayout.TextField(
                "Phase Name",
                _node.PhaseName);

            _node.AnimatorState = EditorGUILayout.TextField(
                "Animator State",
                _node.AnimatorState);

            _node.PhaseAudio = (AudioClip)EditorGUILayout.ObjectField(
                "Audio Clip",
                _node.PhaseAudio,
                typeof(AudioClip),
                false);

            if (GUI.changed)
                if (_machine != null)
                    EditorUtility.SetDirty(_machine);
        });

        extensionContainer.Add(inspector);

        RefreshExpandedState();
        RefreshPorts();
    }
    
    public PhaseNode GetNode()
    {
        return _node;
    }
}
#endif
using UnityEngine;

public class NPC : Interactable, ITalkable
{
    [SerializeField] private string NPCKey;
    [SerializeField] private DialougeText dialougeText;

    public string GetNPCKey() => NPCKey;

    protected override void OnInteract(Transform player)
    {
        if (player == null) return;
        if (dialougeText == null) return;

        Talk(dialougeText);
    }

    public void Talk(DialougeText dialougeText)
    {
        if (DialougeController.Instance == null)
        {
            Debug.LogWarning("DialougeController Instance is NULL");
            return;
        }

        DialougeController.Instance.DisplayNextParagraph(dialougeText, NPCKey);
    }
}

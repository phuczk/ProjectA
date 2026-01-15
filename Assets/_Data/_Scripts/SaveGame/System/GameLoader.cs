using UnityEngine;

public class GameLoader : MonoBehaviour
{
    void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveApplier.Apply(SaveManager.Instance.CurrentData);
        }
    }
}

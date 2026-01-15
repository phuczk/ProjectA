using System.Linq;
using UnityEngine;

public static class SaveApplier
{
    public static void Apply(SaveData data)
    {
        if (data == null) return;

        var saveables = Object
            .FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ISaveable>();

        foreach (var s in saveables)
            s.LoadData(data);
    }
}

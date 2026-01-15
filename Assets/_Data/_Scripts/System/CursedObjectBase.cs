using UnityEngine;
using System.Collections.Generic;

// Tạo một class helper để dễ truy cập database
public static class CursedDatabase
{
    private static CursedList _instance;

    // Cách 1: Assign thủ công trong Inspector (khuyến nghị)
    // [SerializeField] private CursedList cursedListAsset; // kéo thả asset vào đây trong một MonoBehaviour nào đó

    // // Cách 2: Tự động tìm (nếu chỉ có 1 asset duy nhất trong project)
    // private static void LoadInstance()
    // {
    //     if (_instance != null) return;

    //     var assets = Resources.LoadAll<CursedList>("");
    //     if (assets.Length > 0)
    //     {
    //         _instance = assets[0];
    //         Debug.Log($"Loaded CursedList: {_instance.name}");
    //     }
    //     else
    //     {
    //         Debug.LogError("Không tìm thấy CursedList asset nào trong Resources!");
    //     }
    // }

    // public static CursedObjectData GetById(string id)
    // {
    //     if (_instance == null)
    //     {
    //         LoadInstance();
    //         if (_instance == null) return null;
    //     }

    //     // Tìm theo id (có thể tối ưu bằng Dictionary nếu danh sách lớn)
    //     return _instance.CursedObjects.Find(co => co.id == id);
    // }

    // // Optional: Nếu danh sách lớn, nên build dictionary khi load
    // private static Dictionary<string, CursedObjectData> _lookup;

    // public static CursedObjectData GetByIdFast(string id)
    // {
    //     if (_instance == null) LoadInstance();
    //     if (_instance == null) return null;

    //     if (_lookup == null || _lookup.Count == 0)
    //     {
    //         _lookup = new Dictionary<string, CursedObjectData>();
    //         foreach (var co in _instance.CursedObjects)
    //         {
    //             if (!_lookup.ContainsKey(co.id))
    //                 _lookup[co.id] = co;
    //             else
    //                 Debug.LogWarning($"Duplicate cursed id: {co.id}");
    //         }
    //     }

    //     _lookup.TryGetValue(id, out var data);
    //     return data;
    // }
}
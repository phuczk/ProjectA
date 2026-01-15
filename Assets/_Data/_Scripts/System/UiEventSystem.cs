using System;
using System.Collections.Generic;

public static class UIEventSystem
{
    // Sự kiện HP: truyền vào (currentHP, maxHP)
    public static Action<float, float> OnHealthChanged;
    // Sự kiện Mana: truyền vào (currentMana, maxMana)
    public static Action<float, float> OnManaChanged;
    // Sự kiện Exp: truyền vào (currentExp, maxExp)
    public static Action<bool> OnCutSceneStart;
    public static Action<bool> OnCutSceneEnd;

    // Sự kiện Inventory: truyền vào danh sách vật phẩm mới
    public static Action<List<string>> OnInventoryUpdated;

    // Sự kiện Trạng thái Menu: truyền vào (Tên menu, Trạng thái đóng/mở)
    public static Action<bool> OnToggleUIElement;

    // Sự kiện Map: truyền vào tọa độ Player
    public static Action<UnityEngine.Vector2> OnPlayerMoved;
}
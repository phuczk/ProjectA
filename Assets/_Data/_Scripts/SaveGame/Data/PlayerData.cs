using UnityEngine;
using System.Collections.Generic;
using GlobalEnums;

[System.Serializable]
public class PlayerData
{
    public int maxHealth;
    public int maxMana;
    public Vector3 position;
    public int currentMoney;
    public int currentMaskFragment;
    public int currentManaFragment;

    public bool hasHunterNote = false;
    public List<string> currentCursedObjects = new();
    public List<AbilityType> unlockedAbilities = new();
    public List<GunType> unlockedGuns = new();
    public List<string> unlockedCursedObjects = new();
}

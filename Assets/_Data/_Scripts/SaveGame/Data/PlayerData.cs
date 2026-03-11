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
    public int currentNotch = 4;
    public int currentDamage = 0;
    public int currentSkillDamage = 0;
    public GunType currentGun = GunType.Normal;

    public bool hasHunterNote = false;

    [SerializeField]
    public List<string> currentCursedObjects = new();

    public List<AbilityType> unlockedAbilities = new();
    public List<GunType> unlockedGuns = new();
}

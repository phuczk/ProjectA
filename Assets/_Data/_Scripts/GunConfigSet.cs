using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/GunConfigSet")]
public class GunConfigSet : ScriptableObject
{
    public GunConfig Normal;
    public GunConfig Shotgun;
    public GunConfig Rapid;
    public GunConfig Sniper;
}

[System.Serializable]
public struct GunConfig
{
    public float bulletSpeed;
    public float bulletLifetime;
    public float bulletRange;
    public float cooldown;
    public int bulletCount;
    public float spreadAngle;
    public float damage;
    public Sprite bulletSprite;
    public Sprite gunSprite;
    public float shakeCameraAmount;
    public AudioClip shootSound;
    public GameObject bulletPrefab;
    public float manaGain;
}

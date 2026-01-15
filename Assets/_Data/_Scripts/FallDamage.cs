using UnityEngine;

public class FallDamage : MonoBehaviour
{
    private PlayerHealth playerHealth;
    // private SafeGroundSave safeGroundSave;

    private SafeGroundCheckpointSaver safeGroundCheckpointSaver;

    private void Start() {
        safeGroundCheckpointSaver = GameObject.FindGameObjectWithTag("Player").GetComponent<SafeGroundCheckpointSaver>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<PlayerHealth>();
            playerHealth.TakeDamage(1);
            safeGroundCheckpointSaver.WrapToSafeGround();
        }
    }
}
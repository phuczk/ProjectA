using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FallDamage : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private SafeGroundCheckpointSaver safeGroundCheckpointSaver;

    private void Start()
    {
        safeGroundCheckpointSaver = GameObject
            .FindGameObjectWithTag("Player")
            .GetComponent<SafeGroundCheckpointSaver>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<PlayerHealth>();
            playerHealth.TakeDamage(1);

            if (playerHealth.CurrentHealth <= 0)
            {
                return;
            }

            SceneTransitionManager.Instance.TransitionToScene(
                "Game",
                TransitionType.Death,
                FadeDirection.Left,
                false
            );

            StartCoroutine(SwapCheckpointDelay());
        }
    }

    private IEnumerator SwapCheckpointDelay()
    {
        yield return new WaitForSeconds(0.3f);
        safeGroundCheckpointSaver.WrapToSafeGround();
    }
}

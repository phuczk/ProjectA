using UnityEngine;
using System.Collections;

public class SafeGroundSave : MonoBehaviour
{
    [SerializeField] private float saveFrequency = 3f;
    [SerializeField] private GroundCheck groundCheck;

    public Vector2 SafeGroundPosition { get; private set; } = new Vector2(0f, 0f);
    private Coroutine SaveGroundCoroutine;

    private void Start() {
        SaveGroundCoroutine = StartCoroutine(SaveGroundLocation());
        SafeGroundPosition = transform.position;
    }

    private IEnumerator SaveGroundLocation() {
        float elapsedTime = 0f;
        while (elapsedTime < saveFrequency) {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (groundCheck.IsGrounded()) {
            SafeGroundPosition = transform.position;
        }
        SaveGroundCoroutine = StartCoroutine(SaveGroundLocation());
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(1);
        }
    }
    
    public void WrapToSafeGround()
    {
        transform.position = SafeGroundPosition;
    }
}

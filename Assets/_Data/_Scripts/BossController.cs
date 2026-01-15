using UnityEngine;

public class BossController : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 100f;

    void OnBecameVisible()
{
    enabled = true;
}

void OnBecameInvisible()
{
    enabled = false;
}

}

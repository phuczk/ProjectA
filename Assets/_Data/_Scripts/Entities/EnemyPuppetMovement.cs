using UnityEngine;

public class EnemyPuppetMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 12f;
    public float turnSpeed = 12f;

    [Header("Bobbing")]
    public float bobSpeed = 9f;
    public float bobHeight = 0.25f;

    [Header("Sway")]
    public float swaySpeed = 3f;
    public float bodyJiggleAmp = 3f;

    [Header("Visual")]
    public Transform body;

    private Vector2 moveDir = Vector2.right;

    private Vector3 bodyBaseLocalPos;
    private float bobPhaseOffset;
    private float swayPhaseOffset;

    void Awake()
    {
        if (body == null)
        {
            Debug.LogError("EnemyPuppetMovement: body is NULL");
            enabled = false;
            return;
        }
        bodyBaseLocalPos = body.localPosition;
        bobPhaseOffset = Random.Range(0f, Mathf.PI * 2f);
        swayPhaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    public void SetMoveDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.01f)
            moveDir = dir.normalized;
        else
            moveDir = Vector2.zero;
    }

    public void TickMovement(float speed = 1f)
    {
        MoveHorizontal(speed);
        BobAndSway();
        Flip();
    }

    private void MoveHorizontal(float speed)
    {
        Vector3 target =
            transform.position + (Vector3)moveDir * moveSpeed * speed * Time.deltaTime;

        transform.position =
            Vector3.Lerp(transform.position, target, turnSpeed * Time.deltaTime);
    }

    public void BobAndSway(bool idle = false)
    {
        float bobMul = idle ? 1f : 1f;
        float swayMul = idle ? 1f : 1f;

        float bob =
            Mathf.Sin(Time.time * bobSpeed + bobPhaseOffset)
            * bobHeight
            * bobMul;

        body.localPosition = new Vector3(
            bodyBaseLocalPos.x,
            bodyBaseLocalPos.y + bob,
            bodyBaseLocalPos.z
        );

        float jiggle =
            Mathf.Sin(Time.time * swaySpeed + swayPhaseOffset)
            * bodyJiggleAmp
            * swayMul;

        body.localRotation = Quaternion.Euler(0, 0, jiggle);
    }

    private void Flip()
    {
        var sr = body.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.flipX = moveDir.x < 0;
    }
}

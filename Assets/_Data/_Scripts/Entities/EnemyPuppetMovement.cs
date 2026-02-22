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
    public Transform shadow;
    [Tooltip("Tốc độ lật mặt của nhân vật")]
    public float flipSmoothTime = 15f; 

    private Vector2 moveDir = Vector2.right;
    private Vector3 bodyBaseLocalPos;
    private float bobPhaseOffset;
    private float swayPhaseOffset;

    private float _targetYRotation = 0f;

    void Awake()
    {
        if (body == null)
        {
            enabled = false;
            return;
        }
        bodyBaseLocalPos = body.localPosition;
        bobPhaseOffset = Random.Range(0f, Mathf.PI * 2f);
        swayPhaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    public void SetMoveDirection(Vector2 dir, bool isNormalized = false)
    {
        if (dir.sqrMagnitude > 0.01f)
            moveDir = isNormalized ? dir : dir.normalized;
        else
            moveDir = Vector2.zero;
    }

    public void TickMovement(float speed = 1f)
    {
        if (moveDir.sqrMagnitude > 0.01f)
        {
            MoveHorizontal(speed);
            //CalculateFlip(); // Tính toán góc xoay dựa trên hướng di chuyển
        }
        
        BobAndSway(moveDir.sqrMagnitude <= 0.01f);
    }

    private void MoveHorizontal(float speed)
    {
        Vector3 target = transform.position + (Vector3)moveDir * moveSpeed * speed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, target, turnSpeed * Time.deltaTime);
    }

    public void BobAndSway(bool idle = false)
    {
        float bob = Mathf.Sin(Time.time * bobSpeed + bobPhaseOffset) * bobHeight;

        body.localPosition = new Vector3(
            bodyBaseLocalPos.x,
            bodyBaseLocalPos.y + bob,
            bodyBaseLocalPos.z
        );

        float jiggle = Mathf.Sin(Time.time * swaySpeed + swayPhaseOffset) * bodyJiggleAmp;

        float currentY = Mathf.LerpAngle(body.localEulerAngles.y, _targetYRotation, flipSmoothTime * Time.deltaTime);
        
        body.localRotation = Quaternion.Euler(0, currentY, jiggle);
        
        if (shadow != null)
        {
            shadow.localRotation = Quaternion.Euler(0, currentY, 0);
        }
    }

    private void CalculateFlip()
    {
        if (moveDir.x < -0.01f) 
            _targetYRotation = 180f;
        else if (moveDir.x > 0.01f) 
            _targetYRotation = 0f;
    }
}

using System;
using UnityEngine;
using GlobalEnums;

[Serializable]
public class LaserAttackNode : EnemyStateNode
{
    public override EnemyStateType StateType => EnemyStateType.Attack;

    [Header("Laser Settings")]
    public LineRenderer LaserLinePrefab; // Prefab chứa LineRenderer
    public float LaserDuration = 1.5f;   // Thời gian tồn tại của tia laser
    public float DamagePerSecond = 10f;
    public float MaxDistance = 20f;
    public LayerMask TargetLayer;        // Layer của Player

    public Vector2 SpawnOffset = new Vector2(0.5f, 0.5f); // Độ lệch tia laser so với Boss

    [Header("Timing")]
    public float WarningTime = 0.8f;     // Thời gian hiện tia laser mờ để báo trước
    public float Cooldown = 1.0f;

    private LineRenderer _currentLine;
    private float _timer;
    private bool _isWarning;
    private bool _isFiring;

    public override void Enter()
    {
        IsFinished = false;
        _timer = WarningTime;
        _isWarning = true;
        _isFiring = false;

        // Tạo instance của LineRenderer từ Prefab
        if (LaserLinePrefab != null)
        {
            _currentLine = UnityEngine.Object.Instantiate(LaserLinePrefab, machine.CachedTransform);
            _currentLine.enabled = true;
            SetLaserAlpha(0.2f); // Làm mờ để báo hiệu
        }
    }

    public override void ExecuteLogic()
    {
        _timer -= Time.deltaTime;

        if (_currentLine == null) return;

        // Cập nhật điểm đầu và điểm cuối của Laser
        UpdateLaserPositions();

        if (_isWarning && _timer <= 0f)
        {
            _isWarning = false;
            _isFiring = true;
            _timer = LaserDuration;
            SetLaserAlpha(1.0f); // Hiện rõ tia laser (Bắt đầu gây sát thương)
        }
        else if (_isFiring)
        {
            CheckDamage(); // Gây sát thương liên tục khi đang bắn
            if (_timer <= 0f)
            {
                StopLaser();
                _timer = Cooldown;
            }
        }
        else if (!_isWarning && !_isFiring && _timer <= 0f)
        {
            IsFinished = true;
        }
    }

    private void UpdateLaserPositions()
    {
        Vector3 startPos = machine.CachedTransform.position;
        // Bắn tia laser theo hướng Boss đang nhìn (LocalScale.x)
        Vector2 direction = machine.CachedTransform.localScale.x > 0 ? Vector2.right : Vector2.left;
        
        // Nếu muốn laser đuổi theo Player, hãy tính hướng tại đây
        // direction = (machine.Target.position - startPos).normalized;

        _currentLine.SetPosition(0, startPos);
        _currentLine.SetPosition(1, startPos + (Vector3)direction * MaxDistance);
    }

    private void CheckDamage()
    {
        // Xác định hướng bắn dựa trên Scale của Boss (giống logic di chuyển của bạn)
        Vector2 direction = machine.CachedTransform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector3 startPos = machine.CachedTransform.position + (Vector3)SpawnOffset;

        // Bắn tia Raycast để kiểm tra va chạm với Player
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, MaxDistance, TargetLayer);
        
        // Vẽ tia debug trong Scene để bạn dễ kiểm tra (nhấn Gizmos để thấy)
        Debug.DrawRay(startPos, direction * MaxDistance, Color.red);

        if (hit.collider != null)
        {
            // Chỉ gây sát thương nếu trúng Player
            if (hit.collider.CompareTag("Player"))
            {
                // Sử dụng sát thương theo thời gian (Damage Per Second)
                hit.collider.GetComponent<PlayerHealth>()?.TakeDamage(Mathf.RoundToInt(DamagePerSecond * Time.deltaTime));
            }
        }
    }

    private void SetLaserAlpha(float alpha)
    {
        Gradient gradient = _currentLine.colorGradient;
        GradientAlphaKey[] alphas = gradient.alphaKeys;
        for (int i = 0; i < alphas.Length; i++) alphas[i].alpha = alpha;
        gradient.alphaKeys = alphas;
        _currentLine.colorGradient = gradient;
    }

    private void StopLaser()
    {
        _isFiring = false;
        if (_currentLine != null) UnityEngine.Object.Destroy(_currentLine.gameObject);
    }

    public override void Exit()
    {
        StopLaser();
    }
}

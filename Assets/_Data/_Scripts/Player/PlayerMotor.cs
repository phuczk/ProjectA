using System;
using UnityEngine;
using GlobalEnums;

public class PlayerMotor : MonoBehaviour
{
    public Action<bool, float> OnGroundedChanged;
    public Action OnJumped;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private ScriptableStats _stats;
    private GameObject _visuals;
    private PlayerAbility _ability;

    private Vector2 _frameVelocity;
    private bool _grounded;
    private float _frameLeftGrounded = float.MinValue;

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;

    private bool _isDashing;
    private float _dashEndTime;
    private float _dashCooldownUntil;

    public bool IsDashing => _isDashing;

    public void Configure(Rigidbody2D rb, Collider2D col, ScriptableStats stats, GameObject visuals, PlayerAbility ability)
    {
        _rb = rb;
        _col = col;
        _stats = stats;
        _visuals = visuals;
        _ability = ability;
    }

    public void SetJumpInput(bool jumpDown, bool jumpHeld, float time)
    {
        if (jumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = time;
        }
        if (!_endedJumpEarly && !_grounded)
        {
            var up = GetUpDir();
            var vUp = Vector2.Dot(_rb.linearVelocity, up);
            if (!jumpHeld && vUp > 0) _endedJumpEarly = true;
        }
    }

    public void CheckCollisions(float time)
    {
        var up = GetUpDir();
        var right = GetRightDir(up);

        Physics2D.queriesStartInColliders = false;
        var mask = ~_stats.PlayerLayer;
        bool groundHit = Physics2D.BoxCast(_col.bounds.center, _col.bounds.size, 0, -up, _stats.GrounderDistance, mask);

        if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = time;
            OnGroundedChanged?.Invoke(false, 0);
        }
        else if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            OnGroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        Physics2D.queriesStartInColliders = true;
    }

    public void HandleJump(float time)
    {
        if (!_jumpToConsume && !HasBufferedJump(time)) return;
        if (_grounded || CanUseCoyote(time)) ExecuteJump();
        _jumpToConsume = false;
    }

    public void HandleDirection(Vector2 move)
    {
        var up = GetUpDir();
        var right = GetRightDir(up);
        var vUp = Vector2.Dot(_frameVelocity, up);
        var vRight = Vector2.Dot(_frameVelocity, right);
        if (move.x == 0)
        {
            var decel = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            vRight = Mathf.MoveTowards(vRight, 0, decel * Time.fixedDeltaTime);
        }
        else
        {
            vRight = Mathf.MoveTowards(vRight, move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
        }
        _frameVelocity = up * vUp + right * vRight;
    }

    // public void HandleDash(bool waitingForCamera, float time, bool dashDown, float dashSpeed, float dashDuration, float dashCooldownTime)
    // {
    //     if (!_ability.Has(AbilityType.Dash)) return;
    //     if (waitingForCamera) return;
    //     if (!_isDashing)
    //     {
    //         if (time >= _dashCooldownUntil && dashDown)
    //         {
    //             var up = GetUpDir();
    //             var right = GetRightDir(up);
    //             float sign = _visuals != null && _visuals.transform.localScale.x < 0 ? -1f : 1f;
    //             var vRight = dashSpeed * sign;
    //             _frameVelocity = up * 0f + right * vRight;
    //             _isDashing = true;
    //             _dashEndTime = time + dashDuration;
    //             _dashCooldownUntil = time + dashCooldownTime;
    //         }
    //     }
    //     else
    //     {
    //         if (time >= _dashEndTime)
    //         {
    //             _isDashing = false;
    //             return;
    //         }
    //         var up = GetUpDir();
    //         var right = GetRightDir(up);
    //         float sign = _visuals != null && _visuals.transform.localScale.x < 0 ? -1f : 1f;
    //         var vRight = dashSpeed * sign;
    //         _frameVelocity = up * 0f + right * vRight;
    //     }
    // }

    public void HandleDashLogic(float time, float dashSpeed)
    {
        if (!_isDashing) return;

        if (time >= _dashEndTime)
        {
            _isDashing = false;
            
            var up = GetUpDir();
            var right = GetRightDir(up);
            var vRight = Vector2.Dot(_frameVelocity, right);
            
            vRight = Mathf.Clamp(vRight, -_stats.MaxSpeed, _stats.MaxSpeed);
            _frameVelocity = right * vRight; 
            
            return;
        }

        var upDir = GetUpDir();
        var rightDir = GetRightDir(upDir);
        float sign = _visuals != null && _visuals.transform.localScale.x < 0 ? -1f : 1f;
        var dashVelRight = dashSpeed * sign;
        
        _frameVelocity = upDir * 0f + rightDir * dashVelRight;
    }

    public bool TryStartDash(float time, float dashSpeed, float dashDuration, float dashCooldownTime)
    {
        if (!_ability.Has(AbilityType.Dash)) return false;
        if (_isDashing || time < _dashCooldownUntil) return false;

        _isDashing = true;
        _dashEndTime = time + dashDuration;
        _dashCooldownUntil = time + dashCooldownTime;

        var up = GetUpDir();
        var right = GetRightDir(up);
        float sign = _visuals != null && _visuals.transform.localScale.x < 0 ? -1f : 1f;
        _frameVelocity = right * (dashSpeed * sign);

        return true;
    }
    
    public void HandleGravity()
    {
        var up = GetUpDir();
        var right = GetRightDir(up);
        var vUp = Vector2.Dot(_frameVelocity, up);
        var vRight = Vector2.Dot(_frameVelocity, right);
        if (_grounded && vUp <= 0f)
        {
            vUp = _stats.GroundingForce;
        }
        else
        {
            var gravity = _stats.FallAcceleration;
            if (_endedJumpEarly && vUp > 0) gravity *= _stats.JumpEndEarlyGravityModifier;
            vUp = Mathf.MoveTowards(vUp, -_stats.MaxFallSpeed, gravity * Time.fixedDeltaTime);
        }
        _frameVelocity = up * vUp + right * vRight;
    }

    public void ApplyMovement()
    {
        _rb.linearVelocity = _frameVelocity;
    }

    private bool HasBufferedJump(float time) => _bufferedJumpUsable && time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote(float time) => _coyoteUsable && !_grounded && time < _frameLeftGrounded + _stats.CoyoteTime;

    private void ExecuteJump()
    {
        var up = GetUpDir();
        var right = GetRightDir(up);
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        var vRight = Vector2.Dot(_frameVelocity, right);
        _frameVelocity = up * _stats.JumpPower + right * vRight;
        OnJumped?.Invoke();
    }

    private Vector2 GetUpDir()
    {
        var g = Physics2D.gravity;
        if (g.sqrMagnitude < 0.0001f) return Vector2.up;
        return -g.normalized;
    }

    private Vector2 GetRightDir(Vector2 up)
    {
        return new Vector2(up.y, -up.x);
    }
}

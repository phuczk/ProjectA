using UnityEngine;
using DG.Tweening;
using GlobalEnums;

public class PlayerGravityFlip : MonoBehaviour
{
    private GravityFlipManager _manager;
    private Rigidbody2D _rb;
    private PlayerAbility _ability;

    public void Configure(GravityFlipManager manager, Rigidbody2D rb, PlayerAbility ability)
    {
        _manager = manager;
        _rb = rb;
        _ability = ability;
    }

    public void HandleInput(PlayerInputHandler input, Transform playerTransform, ref bool waitingForCamera, ref Vector2 pendingGravity)
    {
        if (!_ability.Has(AbilityType.GravityFlip)) return;
        if (_manager == null || waitingForCamera) return;
        var newDir = _manager.gravityDirection;

        if (input != null && input.FlipGravityUp())
        {
            newDir = _manager.gravityDirection == GravityDirection.North ? GravityDirection.South :
                     _manager.gravityDirection == GravityDirection.South ? GravityDirection.North :
                     _manager.gravityDirection == GravityDirection.East ? GravityDirection.West : GravityDirection.East;
        }
        else if (input != null && input.FlipGravityLeft())
        {
            newDir = _manager.gravityDirection == GravityDirection.North ? GravityDirection.East :
                     _manager.gravityDirection == GravityDirection.South ? GravityDirection.West :
                     _manager.gravityDirection == GravityDirection.East ? GravityDirection.South : GravityDirection.North;
        }
        else if (input != null && input.FlipGravityRight())
        {
            newDir = _manager.gravityDirection == GravityDirection.North ? GravityDirection.West :
                     _manager.gravityDirection == GravityDirection.South ? GravityDirection.East :
                     _manager.gravityDirection == GravityDirection.East ? GravityDirection.North : GravityDirection.South;
        }
        else
        {
            return;
        }

        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.linearVelocity = Vector2.zero;
        waitingForCamera = true;
        pendingGravity = newDir switch
        {
            GravityDirection.North => new Vector2(0, -28f),
            GravityDirection.South => new Vector2(0, 28f),
            GravityDirection.East => new Vector2(-38f, 0),
            GravityDirection.West => new Vector2(38f, 0),
            _ => new Vector2(0, -28f)
        };
        _manager.FlipGravity(newDir);
    }

    public void OnCameraRotationComplete(Transform playerTransform, ref bool waitingForCamera, ref Vector2 frameVelocity, ref Vector2 pendingGravity)
    {
        if (!waitingForCamera) return;
        waitingForCamera = false;
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.linearVelocity = Vector2.zero;
        Physics2D.gravity = pendingGravity;

        var up = GetUpDir();
        float angle = Mathf.Atan2(up.y, up.x) * Mathf.Rad2Deg - 90f;
        playerTransform.DORotate(new Vector3(0, 0, angle), 0.05f).SetEase(Ease.OutQuad);

        frameVelocity = Vector2.zero;
    }

    private Vector2 GetUpDir()
    {
        var g = Physics2D.gravity;
        if (g.sqrMagnitude < 0.0001f) return Vector2.up;
        return -g.normalized;
    }
}

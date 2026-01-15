using UnityEngine;

public class PlayerAudioBinder : MonoBehaviour 
{
    [Header("Health Sounds")]
    [SerializeField] private AudioClip _damageClip;
    [SerializeField] private AudioClip _deathClip;
    [SerializeField] private AudioClip _manaClip;

    [Header("Movement Sounds")]
    [SerializeField] private AudioClip[] _footstepClips;
    [SerializeField] private AudioClip _jumpClip;

    private ISoundEmitter[] _emitters;

    private void Awake()
    {
        _emitters = GetComponentsInChildren<ISoundEmitter>();
    }

    private void OnEnable()
    {
        foreach (var emitter in _emitters)
            emitter.OnRequestSound += HandleSoundRequest;
    }

    private void OnDisable()
    {
        foreach (var emitter in _emitters)
            emitter.OnRequestSound -= HandleSoundRequest;
    }

    private void HandleSoundRequest(PlayerSoundType type, AudioClip directClip)
    {
        if (directClip != null)
        {
            AudioManager.Instance?.PlaySFX(directClip);
            return;
        }
        AudioClip clipToPlay = type switch
        {
            PlayerSoundType.Damage => _damageClip,
            PlayerSoundType.Death => _deathClip,
            PlayerSoundType.ManaUsed => _manaClip,
            PlayerSoundType.Footstep => _footstepClips[Random.Range(0, _footstepClips.Length)],
            PlayerSoundType.Jump => _jumpClip,
            _ => null
        };

        if (clipToPlay != null)
            AudioManager.Instance?.PlaySFX(clipToPlay);
    }
}
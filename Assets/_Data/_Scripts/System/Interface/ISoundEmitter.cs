using UnityEngine;

public interface ISoundEmitter
{
    // Sự kiện này sẽ truyền cả Enum VÀ AudioClip (có thể null)
    event System.Action<PlayerSoundType, AudioClip> OnRequestSound;
}

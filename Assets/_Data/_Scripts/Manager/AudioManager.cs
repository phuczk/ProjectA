using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [SerializeField] private AudioClip backgroundMusic;

    protected override void Awake()
    {
        base.Awake();

        // Tạo 2 AudioSource riêng biệt
        _musicSource ??= gameObject.AddComponent<AudioSource>();
        _sfxSource ??= gameObject.AddComponent<AudioSource>();

        // Cấu hình cho nhạc nền
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
    }
    
    // Phát nhạc nền mặc định khi bắt đầu game
    private void Start()
    {
        PlayMusic(backgroundMusic);
    }

    // Phát nhạc nền (Background Music)
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null || _musicSource.clip == clip) return;

        _musicSource.clip = clip;
        _musicSource.volume = volume;
        _musicSource.Play();
    }

    // Phát hiệu ứng âm thanh (Sound Effects)
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        _sfxSource.PlayOneShot(clip, volumeScale);
    }

    // Dừng tất cả các hiệu ứng âm thanh
    public void StopAllSFX() => _sfxSource.Stop();

    // Dừng nhạc nền
    public void StopMusic() => _musicSource.Stop();
}
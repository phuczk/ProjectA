using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSetting : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Slider _sfxSlider;


    private void Start()
    {
        if (PlayerPrefs.HasKey("MusicVolume") && PlayerPrefs.HasKey("SfxVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSfxVolume();
        }
    }
    
    private void OnEnable()
    {
        if (PlayerPrefs.HasKey("MusicVolume") && PlayerPrefs.HasKey("SfxVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSfxVolume();
        }
    }

    public void SetMusicVolume()
    {
        float volume = _volumeSlider.value;
        myMixer.SetFloat("music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSfxVolume()
    {
        float volume = _sfxSlider.value;
        myMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SfxVolume", volume);
    }

    private void LoadVolume()
    {
        _volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        _sfxSlider.value = PlayerPrefs.GetFloat("SfxVolume");
        SetMusicVolume();
        SetSfxVolume();
    }
}

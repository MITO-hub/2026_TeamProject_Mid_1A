using UnityEngine;
using UnityEngine.UI;

public class SettingsVolumeController : MonoBehaviour
{
    private const string BgmVolumeKey = "Settings.BgmVolume";
    private const string SfxVolumeKey = "Settings.SfxVolume";

    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Text bgmValueText;
    [SerializeField] private Text sfxValueText;

    public float BgmVolume { get; private set; } = 0.5f;
    public float SfxVolume { get; private set; } = 0.5f;

    private void Awake()
    {
        BgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, 0.5f);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 0.5f);

        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(BgmVolume);
            bgmSlider.onValueChanged.AddListener(SetBgmVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(SfxVolume);
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        }

        ApplyBgmVolume();
        RefreshLabels();
    }

    public void SetBgmVolume(float value)
    {
        BgmVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(BgmVolumeKey, BgmVolume);
        ApplyBgmVolume();
        RefreshLabels();
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        RefreshLabels();
    }

    private void ApplyBgmVolume()
    {
        AudioListener.volume = BgmVolume;
    }

    private void RefreshLabels()
    {
        if (bgmValueText != null)
        {
            bgmValueText.text = Mathf.RoundToInt(BgmVolume * 100f).ToString();
        }

        if (sfxValueText != null)
        {
            sfxValueText.text = Mathf.RoundToInt(SfxVolume * 100f).ToString();
        }
    }
}

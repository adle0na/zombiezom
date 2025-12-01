using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [LabelText("배경음 슬라이더")]
    [SerializeField] private Slider bgmSlider;
    [LabelText("배경음 수치 표시")]
    [SerializeField] private TMP_Text bgmValue;
    [LabelText("효과음 슬라이더")]
    [SerializeField] private Slider sfxSlider;
    [LabelText("효과음 수치 표시")]
    [SerializeField] private TMP_Text sfxValue;

    private void Awake()
    {
        // 저장된 값으로 슬라이더 초기화 (없으면 SoundManager 기본값 사용)
        float defaultBgm = SoundManager.Instance != null ? SoundManager.Instance.CurrentBgmVolume : 0.5f;
        float defaultSfx = SoundManager.Instance != null ? SoundManager.Instance.CurrentSfxVolume : 0.5f;

        float savedBgm = PlayerPrefs.GetFloat(SoundManager.PREF_BGM_VOLUME, defaultBgm);
        float savedSfx = PlayerPrefs.GetFloat(SoundManager.PREF_SFX_VOLUME, defaultSfx);

        if (bgmSlider) bgmSlider.value = savedBgm;
        if (sfxSlider) sfxSlider.value = savedSfx;

        // 텍스트 갱신 + 실제 오디오 반영
        UpdateBGMSlider();
        UpdateSFXSlider();
    }

    public void UpdateBGMSlider()
    {
        if (!bgmSlider) return;

        float v = Mathf.Clamp01(bgmSlider.value);
        int percent = Mathf.RoundToInt(v * 100f);
        if (bgmValue) bgmValue.text = $"{percent}%";

        if (SoundManager.Instance != null)
            SoundManager.Instance.SetBgmVolume(v);
        else
        {
            PlayerPrefs.SetFloat(SoundManager.PREF_BGM_VOLUME, v);
            PlayerPrefs.Save();
        }
    }

    public void UpdateSFXSlider()
    {
        if (!sfxSlider) return;

        float v = Mathf.Clamp01(sfxSlider.value);
        int percent = Mathf.RoundToInt(v * 100f);
        if (sfxValue) sfxValue.text = $"{percent}%";

        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSfxVolume(v);
        else
        {
            PlayerPrefs.SetFloat(SoundManager.PREF_SFX_VOLUME, v);
            PlayerPrefs.Save();
        }
    }

    public void RestartScene()
    {
        PlayerDataManager.Instance.ResetData();
        
        Time.timeScale = 1f;
        
        SceneManager.LoadScene(2);
    }

    public void BackToMain()
    {
        PlayerDataManager.Instance.ResetData();
        
        Time.timeScale = 1f;
        
        SceneManager.LoadScene(1);
    }

    public void CloseSettingPopup()
    {
        UIManager.Instance.AllPopupClear();
    }
}

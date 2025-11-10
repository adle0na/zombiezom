using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

/*-------------------------------------------------------
                SoundManager

- 클라이언트 사운드 관리
- BGM/SFX 볼륨을 PlayerPrefs에 저장/로드
--------------------------------------------------------*/

public class SoundManager : GenericSingleton<SoundManager>
{
    // PlayerPrefs 키
    public const string PREF_BGM_VOLUME = "PREF_BGM_VOLUME";
    public const string PREF_SFX_VOLUME = "PREF_SFX_VOLUME";

    [Header("오디오 믹서")]
    [SerializeField, LabelText("BGM 믹서")] private AudioMixerGroup bgmMixer;
    [SerializeField, LabelText("SFX 믹서")] private AudioMixerGroup sfxMixer;

    [SerializeField, LabelText("BGM 리스트")] private List<AudioClip> bgmList;
    [SerializeField, LabelText("SFX 리스트")] private List<AudioClip> sfxList;

    private AudioSource bgmSource;
    private Coroutine bgmFadeCo;

    private readonly Queue<AudioSource> sfxPool = new();
    private readonly List<AudioSource> activeSfx = new();

    [SerializeField, Range(0f, 1f), LabelText("기본 BGM 볼륨")] private float bgmVolume = 0.5f;
    [SerializeField, Range(0f, 1f), LabelText("기본 SFX 볼륨")] private float sfxVolume = 0.5f;

    [SerializeField, LabelText("SFX 풀 크기")] private int sfxPoolSize = 10;

    // 현재 볼륨 public getter (UI 등에서 필요시)
    public float CurrentBgmVolume => bgmVolume;
    public float CurrentSfxVolume => sfxVolume;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        // BGM 소스 준비
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.outputAudioMixerGroup = bgmMixer;
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        // SFX 풀 준비
        for (int i = 0; i < sfxPoolSize; i++)
            sfxPool.Enqueue(CreateSfxSource());

        // 저장된 볼륨 로드 후 적용 (없으면 SerializeField 기본값 사용)
        float savedBgm = PlayerPrefs.GetFloat(PREF_BGM_VOLUME, bgmVolume);
        float savedSfx = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, sfxVolume);
        SetBgmVolume(savedBgm);
        SetSfxVolume(savedSfx);
    }

    private AudioSource CreateSfxSource()
    {
        var go = new GameObject("SFX");
        go.transform.parent = transform;
        var src = go.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = sfxMixer;
        src.playOnAwake = false;
        return src;
    }

    // BGM 재생/정지 -------------------------------------------------

    public void PlayBGM(int clipIndex, float fadeTime = 0.5f, Action onComplete = null)
    {
        if (bgmList == null || clipIndex < 0 || clipIndex >= bgmList.Count) return;
        if (bgmFadeCo != null) StopCoroutine(bgmFadeCo);
        bgmFadeCo = StartCoroutine(FadeBgmRoutine(bgmList[clipIndex], fadeTime, onComplete));
    }

    public void StopBGM(float fadeTime = 0.5f)
    {
        if (bgmFadeCo != null) StopCoroutine(bgmFadeCo);
        bgmFadeCo = StartCoroutine(FadeBgmRoutine(null, fadeTime));
    }

    private IEnumerator FadeBgmRoutine(AudioClip nextClip, float time, Action onComplete = null)
    {
        float startVol = bgmSource.volume;
        float t = 0f;

        // 페이드 아웃
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / time);
            yield return null;
        }

        if (nextClip != null)
        {
            bgmSource.clip = nextClip;
            bgmSource.Play();
        }
        else
        {
            bgmSource.Stop();
        }

        // 페이드 인 (타겟은 bgmVolume)
        t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(0f, bgmVolume, t / time);
            yield return null;
        }

        bgmSource.volume = bgmVolume;
        onComplete?.Invoke();
    }

    // SFX -----------------------------------------------------------

    public void PlaySFX(int sfxIndex, float pitchRandom = 0.05f)
    {
        if (sfxList == null || sfxIndex < 0 || sfxIndex >= sfxList.Count) return;
        if (sfxList[sfxIndex] == null) return;

        AudioSource src = sfxPool.Count > 0 ? sfxPool.Dequeue() : CreateSfxSource();
        src.clip = sfxList[sfxIndex];
        src.volume = sfxVolume;
        // src.pitch = 1f + Random.Range(-pitchRandom, pitchRandom);
        src.transform.position = Vector3.zero;
        src.Play();

        activeSfx.Add(src);
        StartCoroutine(ReleaseWhenDone(src));
    }

    private IEnumerator ReleaseWhenDone(AudioSource src)
    {
        yield return new WaitUntil(() => !src.isPlaying);
        activeSfx.Remove(src);
        sfxPool.Enqueue(src);
    }

    // 볼륨 세터 (UI/초기 로드시 공용) --------------------------------

    public void SetMasterVolume(float linear) =>
        AudioListener.volume = Mathf.Clamp01(linear);

    public void SetBgmVolume(float linear)
    {
        bgmVolume = Mathf.Clamp01(linear);
        // 믹서 파라미터(로그 스케일) + AudioSource.volume(페이드 타깃 일치)
        if (bgmMixer != null) bgmMixer.audioMixer.SetFloat("BGMVol", LinearToDb(bgmVolume));
        if (bgmSource != null) bgmSource.volume = bgmVolume;

        PlayerPrefs.SetFloat(PREF_BGM_VOLUME, bgmVolume);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float linear)
    {
        sfxVolume = Mathf.Clamp01(linear);
        if (sfxMixer != null) sfxMixer.audioMixer.SetFloat("SFXVol", LinearToDb(sfxVolume));

        PlayerPrefs.SetFloat(PREF_SFX_VOLUME, sfxVolume);
        PlayerPrefs.Save();
    }

    private float LinearToDb(float linear) => Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
}

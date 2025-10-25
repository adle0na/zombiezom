using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Audio;
using Random = UnityEngine.Random; // (옵션) 믹서 연결용

/*-------------------------------------------------------
				SoundManager

- 클라이언트 사운드 관리
--------------------------------------------------------*/

public class SoundManager : GenericSingleton<SoundManager>
{
    [Header("오디오 믹서")]
    [SerializeField, LabelText("BGM 믹서")] private AudioMixerGroup bgmMixer;
    [SerializeField, LabelText("SFX 믹서")] private AudioMixerGroup sfxMixer;

    [SerializeField, LabelText("BGM 리스트")] private List<AudioClip> bgmList;
    [SerializeField, LabelText("SFX 리스트")] private List<AudioClip> sfxList;
    
    private AudioSource bgmSource;
    private Coroutine bgmFadeCo;
    
    private readonly Queue<AudioSource> sfxPool = new();
    private readonly List<AudioSource> activeSfx = new();

    private float bgmVolume = 0.5f;
    private float sfxVolume = 0.5f;

    private int sfxPoolSize = 10;
    
    // 설정 초기화 (로컬 값 기준으로 수정해야함)
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.outputAudioMixerGroup = bgmMixer;
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        
        for (int i = 0; i < sfxPoolSize; i++)
            sfxPool.Enqueue(CreateSfxSource());
        
        // 로컬에 저장되어있는 설정값 으로 볼륨 설정
        // 없으면 디폴트값 설정
    }

    private AudioSource CreateSfxSource()
    {
        var src = new GameObject("SFX").AddComponent<AudioSource>();
        src.transform.parent = transform;
        src.outputAudioMixerGroup = sfxMixer;
        src.playOnAwake = false;
        return src;
    }
    
    // BGM 재생 (BGM 인덱스, 페이드 시간, 콜백 함수)
    public void PlayBGM(int clipIndex, float fadeTime = 0.5f, Action onComplete = null)
    {
        if (bgmFadeCo != null) StopCoroutine(bgmFadeCo);
        bgmFadeCo = StartCoroutine(FadeBgmRoutine(bgmList[clipIndex], fadeTime, onComplete));
    }

    public void StopBGM(float fadeTime = 0.5f)
    {
        if (bgmFadeCo != null) StopCoroutine(bgmFadeCo);
        bgmFadeCo = StartCoroutine(FadeBgmRoutine(null, fadeTime));
    }

    // 페이드 코루틴 (다음 음악, 페이드 시간 )
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

        // 페이드 인
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
    
    //
    public void PlaySFX(int sfxIndex, float pitchRandom = 0.05f)
    {
        if (sfxList[sfxIndex] == null) return;

        AudioSource src = sfxPool.Count > 0 ? sfxPool.Dequeue() : CreateSfxSource();
        src.clip = sfxList[sfxIndex];
        src.volume = sfxVolume;
        //src.pitch = 1f + Random.Range(-pitchRandom, pitchRandom);
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
    
    public void SetMasterVolume(float linear) =>
        AudioListener.volume = Mathf.Clamp01(linear);

    public void SetBgmVolume(float linear) =>
        bgmMixer?.audioMixer.SetFloat("BGMVol", LinearToDb(linear));

    public void SetSfxVolume(float linear) =>
        sfxMixer?.audioMixer.SetFloat("SFXVol", LinearToDb(linear));

    private float LinearToDb(float linear) => Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
}
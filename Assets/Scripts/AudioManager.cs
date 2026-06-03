using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("오디오 소스")]
    public AudioSource sfxSource;   // 효과음용 (원샷)
    public AudioSource bgmSource;   // 배경음용 (루프)
    public AudioSource driftSource; // 드리프트 지속음 (루프)

    [Header("클립")]
    public AudioClip explosionClip; // 폭발/피격
    public AudioClip boostClip;     // 부스터
    public AudioClip warningClip;   // 락온 경고
    public AudioClip bgmClip;       // 배경음악
    public AudioClip driftClip;     // 드리프트 지속음

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // PlayOneShot은 timeScale 0(게임오버)에서도 정상 재생됨
    public void PlayExplosion() { PlaySfx(explosionClip); }
    public void PlayBoost()     { PlaySfx(boostClip); }
    public void PlayWarning()   { PlaySfx(warningClip); }

    void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    // 드리프트 지속음 켜기/끄기
    public void SetDrift(bool on)
    {
        if (driftSource == null || driftClip == null) return;

        if (on && !driftSource.isPlaying)
        {
            driftSource.clip = driftClip;
            driftSource.loop = true;
            driftSource.Play();
        }
        else if (!on && driftSource.isPlaying)
        {
            driftSource.Stop();
        }
    }
}

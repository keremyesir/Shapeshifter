using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Muzik")]
    [SerializeField] private AudioClip bgmClip;          // BGM.wav
    [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.3f;

    [Header("Silah Sesleri")]
    [SerializeField] private AudioClip shootClip;        // Range.wav

    [Header("Vurus Sesleri")]
    [SerializeField] private AudioClip hit0Clip;         // Hit0.wav
    [SerializeField] private AudioClip hit1Clip;         // Hit1.wav

    [Header("Olum Sesleri")]
    [SerializeField] private AudioClip enemyDeadClip;    // Dead.wav
    [SerializeField] private AudioClip loseClip;         // Lose.wav

    [Header("Yetenek Sesleri")]
    [SerializeField] private AudioClip abilityClip;      // LevelUp.wav

    [Header("Yakin Dovus")]
    [SerializeField] private AudioClip melee0Clip;       // Melee0.wav
    [SerializeField] private AudioClip melee1Clip;       // Melee1.wav

    [Header("Menu Sesleri")]
    [SerializeField] private AudioClip selectClip;       // Select.wav

    [Header("SFX Ayarlari")]
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.5f;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        // Singleton - sahne yuklense bile hayatta kal
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // AudioSource bilesenlerini olustur
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    // --- BGM KONTROLLERI ---
    public void PlayBGM()
    {
        if (bgmClip == null || bgmSource.isPlaying) return;
        bgmSource.clip = bgmClip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // --- SFX METOTLARI ---
    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // Silah atesi
    public void PlayShoot()
    {
        PlaySFX(shootClip);
    }

    // Dusmana mermi isabet
    public void PlayHit()
    {
        // Rastgele Hit0 veya Hit1 cal
        AudioClip clip = Random.value > 0.5f ? hit0Clip : hit1Clip;
        if (clip == null) clip = hit0Clip; // fallback
        PlaySFX(clip);
    }

    // Dusman olumu
    public void PlayEnemyDead()
    {
        PlaySFX(enemyDeadClip);
    }

    // Yetenek aktivasyonu (Clone, Colossus, Shotgun, Raygun)
    public void PlayAbility()
    {
        PlaySFX(abilityClip);
    }

    // Yakin dovus (Colossus carpma)
    public void PlayMelee()
    {
        AudioClip clip = Random.value > 0.5f ? melee0Clip : melee1Clip;
        if (clip == null) clip = melee0Clip;
        PlaySFX(clip);
    }

    // Game Over
    public void PlayLose()
    {
        PlaySFX(loseClip);
    }

    // Menu buton tiklama
    public void PlaySelect()
    {
        PlaySFX(selectClip);
    }
}

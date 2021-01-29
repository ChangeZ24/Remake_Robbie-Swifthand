using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    static AudioManager current;

    [Header("环境声音")]
    public AudioClip ambientClip;//环境音效
    public AudioClip musicClip;//背景音乐

    [Header("FX声音")]
    public AudioClip deathFxClip;

    [Header("Robbie声音")]
    public AudioClip[] WalkStepClips;//走动声音，4种声音随机播放
    public AudioClip[] crouchStepClips;//下蹲声音，4种随机播放
    public AudioClip jumpClip;//跳跃声音
    public AudioClip deathClip;

    public AudioClip jumpVoiceClip;//跳跃人声
    public AudioClip deathVoiceClip;

    //声音播放Source，环境音效，背景音乐，fx，角色行动声，人声
    AudioSource ambientSource, musicSource, fxSource, playerSource, voiceSource;
    private void Awake()
    {
        current = this;
        //切换场景时，不销毁此对象
        DontDestroyOnLoad(gameObject);

        ambientSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        fxSource = gameObject.AddComponent<AudioSource>();
        playerSource = gameObject.AddComponent<AudioSource>();
        voiceSource = gameObject.AddComponent<AudioSource>();

        StartLevelAudio();
    }

    void StartLevelAudio()
    {
        current.ambientSource.clip = current.ambientClip;
        current.ambientSource.loop = true;
        current.ambientSource.Play();

        current.musicSource.clip = current.musicClip;
        current.musicSource.loop = true;
        current.musicSource.Play();
    }
    public static void PlayFootstepAudio()
    {
        int index = Random.Range(0, current.WalkStepClips.Length);
        current.playerSource.clip = current.WalkStepClips[index];
        current.playerSource.Play();

    }

    public static void PlayCrouchFootstepAudio()
    {
        int index = Random.Range(0, current.crouchStepClips.Length);
        current.playerSource.clip = current.crouchStepClips[index];
        current.playerSource.Play();
    }

    public static void PlayJumpAudio()
    {
        current.playerSource.clip = current.jumpClip;
        current.playerSource.Play();

        current.voiceSource.clip = current.jumpVoiceClip;
        current.voiceSource.Play();
    }
        
    
}

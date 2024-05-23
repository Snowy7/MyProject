using System;
using UnityEngine;

namespace Audio
{
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _soundManager;
        public static SoundManager Instance {
            get
            {
                if (_soundManager == null)
                {
                    _soundManager = FindFirstObjectByType<SoundManager>();
                }
                
                return _soundManager;
            }
            private set => _soundManager = value;
        }
        
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource defaultAudioSource;
        
        private void Awake()
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!uiAudioSource)
            {
                uiAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            if (!musicAudioSource)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            if (!defaultAudioSource)
            {
                defaultAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            uiAudioSource.loop = false;
            musicAudioSource.loop = true;
            defaultAudioSource.loop = false;
        }
        
        public void PlaySound(AudioClip clip, AudioSource source = null)
        {
            if (!clip) return;
            source ??= defaultAudioSource;
            source.PlayOneShot(clip);
        }
        
        public void PlayUISound(AudioClip clip)
        {
            if (!clip || !uiAudioSource) return;
            uiAudioSource.PlayOneShot(clip);
        }
        
        public void PlayMusic(AudioClip clip)
        {
            if (!clip  || !musicAudioSource) return;
            musicAudioSource.clip = clip;
            musicAudioSource.Play();
        }
        
        public void StopMusic()
        {
            musicAudioSource.Stop();
        }
    }
}
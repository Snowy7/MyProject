using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Snowy.Settings
{
    // Audio Profiles are used to store audio settings for different parts of the game.
    [Serializable] public class AudioProfile
    {
        [HideInInspector] public bool createNew = false;
        [HideInInspector] public string newName = "New Profile";
        public AudioMixerGroup mixerGroup = null;
        public float volume = 1;
    }
    
    [Serializable] public class AudioSettings
    {
        public AudioMixer masterMixer = null;
        public string masterVolumeParameter = "MasterVolume";
        public float masterVolume = 1;
        public List<AudioProfile> audioProfiles = new();
        
        public void Load()
        {
            masterVolume = PlayerPrefs.HasKey("MasterVolume") ? PlayerPrefs.GetFloat("MasterVolume") : 1;
            foreach (AudioProfile profile in audioProfiles)
            {
                profile.volume = PlayerPrefs.HasKey(profile.newName + "Volume") ? PlayerPrefs.GetFloat(profile.newName + "Volume") : 1;
            }
        }
        
        public void SetMasterVolume(float value)
        {
            masterVolume = value;
            masterMixer.SetFloat(masterVolumeParameter, Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat("MasterVolume", value);
        }
        
        public void SetProfileVolume(AudioProfile profile, float value)
        {
            profile.volume = value;
            profile.mixerGroup.audioMixer.SetFloat(profile.newName + "Volume", Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat(profile.newName + "Volume", value);
        }
        
        public void SetProfileVolume(string profileName, float value)
        {
            AudioProfile profile = audioProfiles.Find(x => x.newName == profileName);
            if (profile != null)
            {
                SetProfileVolume(profile, value);
            }
        }
    }
}
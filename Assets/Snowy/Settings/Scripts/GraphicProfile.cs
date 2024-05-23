using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Snowy.Settings
{
    //// Display Settings
    
    // Display Mode: Fullscreen, Windowed, Borderless
    // Screen Resolution: 1920x1080, 1280x720, 800x600
    // Brightness: 0.5, 0.75, 1
    // V-Sync: On, Off
    
    [Serializable] public class DisplaySettings
    {
        public enum DisplayMode
        {
            Fullscreen,
            Windowed
        }

        public DisplayMode displayMode;
        public Resolution ScreenResolution;
        public float brightness;
        public bool vSync;
        
        public void Load()
        {
            displayMode = PlayerPrefs.HasKey("DisplayMode") ? (DisplayMode) PlayerPrefs.GetInt("DisplayMode") : DisplayMode.Fullscreen;
            ScreenResolution = PlayerPrefs.HasKey("ScreenResolution") ? JsonUtility.FromJson<Resolution>(PlayerPrefs.GetString("ScreenResolution")) : Screen.currentResolution;
            brightness = PlayerPrefs.HasKey("Brightness") ? PlayerPrefs.GetFloat("Brightness") : 1;
            vSync = PlayerPrefs.HasKey("VSync") && Convert.ToBoolean(PlayerPrefs.GetInt("VSync"));
            
        }

        public Resolution[] ScreenResolutions => Screen.resolutions;

        private List<Resolution> resolutions;

        public List<Resolution> Resolutions
        {
            get
            {
                if (resolutions == null || resolutions.Count == 0)
                    resolutions = Screen.resolutions.ToList();
                
                return resolutions;
            }
        }

        public void SetScreenResolution(Resolution resolution)
        {
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
            ScreenResolution = resolution;
            PlayerPrefs.SetString("ScreenResolution", JsonUtility.ToJson(ScreenResolution));
        }
        
        public void SetDisplayMode(DisplayMode mode)
        {
            Screen.fullScreenMode = mode == DisplayMode.Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            displayMode = mode;
            PlayerPrefs.SetInt("DisplayMode", (int) mode);
        }
        
        public void SetBrightness(float value)
        {
            brightness = value;
            PlayerPrefs.SetFloat("Brightness", value);
        }
        
        public void SetVSync(bool value)
        {
            vSync = value;
            QualitySettings.vSyncCount = Convert.ToInt32(value);
            PlayerPrefs.SetInt("VSync", Convert.ToInt32(value));
        }
        
        public bool GetVSync()
        {
            vSync = QualitySettings.vSyncCount > 0;
            return vSync;
        }
        
        public float GetBrightness()
        {
            brightness = PlayerPrefs.HasKey("Brightness") ? PlayerPrefs.GetFloat("Brightness") : 1;
            return brightness;
        }
        
        public DisplayMode GetDisplayMode()
        {
            displayMode = Screen.fullScreenMode == FullScreenMode.FullScreenWindow ? DisplayMode.Fullscreen : DisplayMode.Windowed;
            return displayMode;
        }

        public int GetResolutionIndex()
        {
            var res = Screen.currentResolution;
            var index = Resolutions.FindIndex(r => r.width == res.width && r.height == res.height && r.refreshRateRatio.ToString() == res.refreshRateRatio.ToString());
            return index == -1 ? 0 : index;
        }
        
        public void SetResolutionIndex(int index)
        {
            SetScreenResolution(Resolutions[index]);
        }
        
        public string[] GetResolutionNames()
        {
            return Resolutions.Select(res => $"{res.width}x{res.height}@{res.refreshRateRatio}Hz").ToArray();
        }
    }
    
    //// Graphics Settings
    
    // Anti-aliasing
    public enum AntiAliasing
    {
        Off,
        Fxaa,
        Smaa,
        Taa
    }
    // Texture Quality
    public enum TextureQuality
    {
        Max,
        High,
        Medium,
        Low
    }
    // Shadow Quality
    public enum ShadowsQuality
    {
        Off,
        Low,
        Medium,
        High,
        Max,
    }
    
    public enum Toggle
    {
        On,
        Off
    }
    // Screen Space Reflections (Maybe) On, Off
    // Volumetric Fog (Maybe) On, Off
    // Volumetric Lighting (Maybe) On, Off
    // Bloom On, Off
    // Lens Flare On, Off
    // Chromatic Aberration On, Off
    // Depth of Field On, Off
    // Motion Blur On, Off
    // Film Noise On, Off
    // Jitter On, Off
    
    [Serializable] public class GraphicProfile
    {
        public bool isDirty;
        public string name = "New Porfile";
        public AntiAliasing antiAliasing;
        public TextureQuality textureQuality;
        public ShadowsQuality shadowsQuality;
        public bool screenSpaceReflections;
        public bool bloom;
        public bool lensFlare;
        public bool chromaticAberration;
        public bool depthOfField;
        public bool motionBlur;
        public bool filmNoise;

        public void Load()
        {
            if (isDirty)
            {
                antiAliasing = PlayerPrefs.HasKey("AntiAliasing") ? (AntiAliasing) PlayerPrefs.GetInt("AntiAliasing") : AntiAliasing.Fxaa;
                textureQuality = PlayerPrefs.HasKey("TextureQuality") ? (TextureQuality) PlayerPrefs.GetInt("TextureQuality") : TextureQuality.Max;
                shadowsQuality = PlayerPrefs.HasKey("ShadowQuality") ? (ShadowsQuality) PlayerPrefs.GetInt("ShadowQuality") : ShadowsQuality.High;
                screenSpaceReflections = PlayerPrefs.HasKey("ScreenSpaceReflections") && Convert.ToBoolean(PlayerPrefs.GetInt("ScreenSpaceReflections"));
                bloom = PlayerPrefs.HasKey("Bloom") && Convert.ToBoolean(PlayerPrefs.GetInt("Bloom"));
                lensFlare = PlayerPrefs.HasKey("LensFlare") && Convert.ToBoolean(PlayerPrefs.GetInt("LensFlare"));
                chromaticAberration = PlayerPrefs.HasKey("ChromaticAberration") && Convert.ToBoolean(PlayerPrefs.GetInt("ChromaticAberration"));
                depthOfField = PlayerPrefs.HasKey("DepthOfField") && Convert.ToBoolean(PlayerPrefs.GetInt("DepthOfField"));
                motionBlur = PlayerPrefs.HasKey("MotionBlur") && Convert.ToBoolean(PlayerPrefs.GetInt("MotionBlur"));
                filmNoise = PlayerPrefs.HasKey("FilmNoise") && Convert.ToBoolean(PlayerPrefs.GetInt("FilmNoise"));
            }
            
            Apply();
        }

        public GraphicProfile(string name)
        {
            this.name = name;
            antiAliasing = AntiAliasing.Fxaa;
            textureQuality = TextureQuality.Max;
            shadowsQuality = ShadowsQuality.High;
            screenSpaceReflections = true;
            bloom = true;
            lensFlare = true;
            chromaticAberration = true;
            depthOfField = true;
            motionBlur = true;
            filmNoise = true;
        }
        
        public GraphicProfile(AntiAliasing antiAliasing, TextureQuality textureQuality, ShadowsQuality shadowsQuality, bool screenSpaceReflections, bool bloom, bool lensFlare, bool chromaticAberration, bool depthOfField, bool motionBlur, bool filmNoise)
        {
            this.antiAliasing = antiAliasing;
            this.textureQuality = textureQuality;
            this.shadowsQuality = shadowsQuality;
            this.screenSpaceReflections = screenSpaceReflections;
            this.bloom = bloom;
            this.lensFlare = lensFlare;
            this.chromaticAberration = chromaticAberration;
            this.depthOfField = depthOfField;
            this.motionBlur = motionBlur;
            this.filmNoise = filmNoise;
        }

        // Apply
        public void Apply()
        {
            SetAntiAliasing(antiAliasing);
            SetTextureQuality(textureQuality);
            SetShadowQuality(shadowsQuality);
            SetScreenSpaceReflections(screenSpaceReflections);
            SetBloom(bloom);
            SetLensFlare(lensFlare);
            SetChromaticAberration(chromaticAberration);
            SetDepthOfField(depthOfField);
            SetMotionBlur(motionBlur);
            SetFilmNoise(filmNoise);
        }

        public void SetAntiAliasing(AntiAliasing value)
        {
            QualitySettings.antiAliasing = value == AntiAliasing.Off ? 0 : (int) value;
            antiAliasing = value;
            if (isDirty) PlayerPrefs.SetInt("AntiAliasing", (int) value);
        }
        
        public void SetTextureQuality(TextureQuality value)
        {
            QualitySettings.globalTextureMipmapLimit = value == TextureQuality.Max ? 0 : (int) value;
            textureQuality = value;
            if (isDirty) PlayerPrefs.SetInt("TextureQuality", (int) value);
        }
        
        public void SetShadowQuality(ShadowsQuality value)
        {
            QualitySettings.shadows = value == ShadowsQuality.Off ? ShadowQuality.Disable : ShadowQuality.All;
            QualitySettings.shadowResolution = value == ShadowsQuality.Off ? ShadowResolution.Low : (ShadowResolution) value + 1;
            shadowsQuality = value;
            if (isDirty) PlayerPrefs.SetInt("ShadowQuality", (int) value);
        }
        
        public void SetScreenSpaceReflections(bool value)
        {
            screenSpaceReflections = value;
            if (isDirty) PlayerPrefs.SetInt("ScreenSpaceReflections", Convert.ToInt32(value));
        }
        
        public void SetBloom(bool value)
        {
            bloom = value;
            if (isDirty) PlayerPrefs.SetInt("Bloom", Convert.ToInt32(value));
        }
        
        public void SetLensFlare(bool value)
        {
            lensFlare = value;
            if (isDirty) PlayerPrefs.SetInt("LensFlare", Convert.ToInt32(value));
        }
        
        public void SetChromaticAberration(bool value)
        {
            chromaticAberration = value;
            if (isDirty) PlayerPrefs.SetInt("ChromaticAberration", Convert.ToInt32(value));
        }
        
        public void SetDepthOfField(bool value)
        {
            depthOfField = value;
            if (isDirty) PlayerPrefs.SetInt("DepthOfField", Convert.ToInt32(value));
        }
        
        public void SetMotionBlur(bool value)
        {
            motionBlur = value;
            if (isDirty) PlayerPrefs.SetInt("MotionBlur", Convert.ToInt32(value));
        }
        
        public void SetFilmNoise(bool value)
        {
            filmNoise = value;
            if (isDirty) PlayerPrefs.SetInt("FilmNoise", Convert.ToInt32(value));
        }
    }
}
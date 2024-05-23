using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

// This class uses reflections a lot so it is pretty dangerous do not play with it! 

namespace Snowy.Settings
{
    [CustomEditor(typeof(Settings))] 
    public class SettingsEditor : Editor
    {
        private Settings m_settings;
        
        private bool m_showNewProfile;
        private AudioProfile m_newProfile;
        
        private bool m_showNewGraphicProfile;
        private GraphicProfile m_newGraphicProfile;
        
        private GUIStyle m_boxStyle;
        private GUIStyle m_titleStyle;
        
        // For the profiles foldouts
        private Dictionary<AudioProfile, bool> m_audio_foldouts = new Dictionary<AudioProfile, bool>();
        private Dictionary<GraphicProfile, bool> m_graphic_foldouts = new Dictionary<GraphicProfile, bool>();
        
        private void OnEnable()
        {
            if (target == null || target.GetType() != typeof(Settings)) return;
            m_settings = (Settings) target;
            
            var darkTex = new Texture2D(1, 1);
            darkTex.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f));
            darkTex.Apply();
            
            // Create a box style with border radius with white outline
            m_boxStyle = new GUIStyle
            {
                border = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(4, 4, 4, 4),
                margin = new RectOffset(4, 4, 4, 4),
                normal =
                {
                    textColor = Color.white,
                    background = !EditorGUIUtility.isProSkin ? EditorGUIUtility.whiteTexture : darkTex
                }
            };
            
            // Create a title style
            m_titleStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = {textColor = Color.white},
                border = new RectOffset(4, 4, 4, 4),
            };
        }
        
        public override void OnInspectorGUI()
        {
            if (m_settings == null) return;
            
            DrawDisplaySettings();
            
            DrawGraphicSettings();
            
            DrawAudioSettings();
        }
        
        private void DrawDisplaySettings()
        {
            // Title centered
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Default Display Settings", m_titleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            
            
            // Draw the display mode field
            var displayMode = (DisplaySettings.DisplayMode) EditorGUILayout.EnumPopup("Display Mode", m_settings.displaySettings.GetDisplayMode());
            m_settings.displaySettings.SetDisplayMode(displayMode);
            
            // Draw the resolution field
            int resolutionIndex = m_settings.displaySettings.GetResolutionIndex();
            string[] resolutionStrings = m_settings.displaySettings.GetResolutionNames();
            resolutionIndex = EditorGUILayout.Popup("Resolution", resolutionIndex, resolutionStrings);
            m_settings.displaySettings.SetResolutionIndex(resolutionIndex);
            
            // Draw the brightness field
            float brightness = EditorGUILayout.Slider("Brightness", m_settings.displaySettings.GetBrightness(), 0, 1);
            m_settings.displaySettings.SetBrightness(brightness);
            
            // Draw the vSync field
            bool vSync = EditorGUILayout.Toggle("VSync", m_settings.displaySettings.GetVSync());
            m_settings.displaySettings.SetVSync(vSync);
        }
        private void DrawGraphicSettings()
        {
            // Title centered
            // Horizontal line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Graphic Profiles", m_titleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            // Draw the profiles container
            DrawGraphicProfilesContainer();
            DrawNewGraphicProfile();
        }
        
        private void DrawGraphicProfilesContainer()
        {
            if (m_settings.graphicSettings.Count == 0)
            {
                EditorGUILayout.HelpBox("No graphic profiles found.", MessageType.Info);
            }
            else
            {
                EditorGUI.indentLevel++;
                foreach (var profile in m_settings.graphicSettings)
                {
                    m_graphic_foldouts.TryAdd(profile, false);
                    DrawGraphicProfile(profile);
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawGraphicProfile(GraphicProfile profile)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.BeginHorizontal();
            m_graphic_foldouts[profile] = EditorGUILayout.Foldout(m_graphic_foldouts[profile], profile.name);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                m_settings.graphicSettings.Remove(profile);
                return;
            }
            EditorGUILayout.EndHorizontal();

            if (m_graphic_foldouts[profile])
            {
                // IsFixed = isDirty
                // Helpbox
                EditorGUILayout.HelpBox("Is Fixed: If disabled, The player will be able to change this profile in game, 'Usually only used for the Custom Profile'.", MessageType.Info);
                profile.isDirty = !EditorGUILayout.Toggle("Is Fixed", !profile.isDirty);
                
                // Anti aliasing
                var antiAliasing = (AntiAliasing) EditorGUILayout.EnumPopup("Anti Aliasing", profile.antiAliasing);
                profile.SetAntiAliasing(antiAliasing);
                
                // Texture Quality
                var textureQuality = (TextureQuality) EditorGUILayout.EnumPopup("Texture Quality", profile.textureQuality);
                profile.SetTextureQuality(textureQuality);
                
                // Shadow Quality
                var shadowQuality = (ShadowsQuality) EditorGUILayout.EnumPopup("Shadow Quality", profile.shadowsQuality);
                profile.SetShadowQuality(shadowQuality);
                
                // Screen Space Reflections
                var screenSpaceReflections = (Toggle) EditorGUILayout.EnumPopup("Screen Space Reflections", profile.screenSpaceReflections ? Toggle.On : Toggle.Off);
                profile.SetScreenSpaceReflections(screenSpaceReflections == Toggle.On);
                
                // Bloom
                var bloom = (Toggle) EditorGUILayout.EnumPopup("Bloom", profile.bloom ? Toggle.On : Toggle.Off);
                profile.SetBloom(bloom == Toggle.On);
                
                // Lens Flare On, Off
                var lensFlare = (Toggle) EditorGUILayout.EnumPopup("Lens Flare", profile.lensFlare ? Toggle.On : Toggle.Off);
                profile.SetLensFlare(lensFlare == Toggle.On);
                // Chromatic Aberration On, Off
                var chromaticAberration = (Toggle) EditorGUILayout.EnumPopup("Chromatic Aberration", profile.chromaticAberration ? Toggle.On : Toggle.Off);
                profile.SetChromaticAberration(chromaticAberration == Toggle.On);
                // Depth of Field On, Off
                var depthOfField = (Toggle) EditorGUILayout.EnumPopup("Depth of Field", profile.depthOfField ? Toggle.On : Toggle.Off);
                profile.SetDepthOfField(depthOfField == Toggle.On);
                // Motion Blur On, Off
                var motionBlur = (Toggle) EditorGUILayout.EnumPopup("Motion Blur", profile.motionBlur ? Toggle.On : Toggle.Off);
                profile.SetMotionBlur(motionBlur == Toggle.On);
                // Film Noise On, Off
                var filmNoise = (Toggle) EditorGUILayout.EnumPopup("Film Noise", profile.filmNoise ? Toggle.On : Toggle.Off);
                profile.SetFilmNoise(filmNoise == Toggle.On);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawNewGraphicProfile()
        {
            if (!m_showNewGraphicProfile)
            {
                if (GUILayout.Button("Add New Graphic Profile"))
                {
                    m_newGraphicProfile = new GraphicProfile("New Profile");
                    m_showNewGraphicProfile = true;
                }
            }
            else
            {
                DrawNewGraphicProfileForm();
            }
        }

        private void DrawNewGraphicProfileForm()
        {
            // Takes only a name to create a new profile
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("New Graphics Profile", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Draw info text
            EditorGUILayout.HelpBox("Create a new Graphics profile to store it for when the game runs.", MessageType.Info);
            // Draw a horizontal line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // Draw the name field
            m_newGraphicProfile.name = EditorGUILayout.TextField("Name", m_newGraphicProfile.name);
            
            // Draw is fixed
            // Helpbox
            EditorGUILayout.HelpBox("Is Fixed: If disabled, The player will be able to change this profile in game, 'Usually only used for the Custom Profile'.", MessageType.Info);
            m_newGraphicProfile.isDirty = !EditorGUILayout.Toggle("Is Fixed", !m_newGraphicProfile.isDirty);
            
            EditorGUILayout.Space();
            
            // Draw the add and cancel buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                m_settings.AddProfile(m_newGraphicProfile);
                m_showNewGraphicProfile = false;
            }
            if (GUILayout.Button("Cancel"))
            {
                m_showNewGraphicProfile = false;
                m_newGraphicProfile = new GraphicProfile("New Profile");
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        # region Audio Settings Drawer

        private void DrawAudioSettings()
        {
            // Draw a button to add a new audio profile
            // Draw the mixer field
            bool hasMasterMixer = m_settings.audioSettings.masterMixer != null;
            EditorGUILayout.Space();
            DrawMaster(hasMasterMixer);
            if (hasMasterMixer) {
                DrawAudioProfilesContainer();
            }
        }

        private void DrawMaster(bool hasMasterMixer)
        {
            // Title centered
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Master Mixer Settings", m_titleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            if (!hasMasterMixer)
            {
                EditorGUILayout.HelpBox("Please assign a master mixer to the settings.", MessageType.Warning);
            }
            m_settings.audioSettings.masterMixer = (AudioMixer) EditorGUILayout.ObjectField("Master Mixer", m_settings.audioSettings.masterMixer, typeof(AudioMixer), false);
            if (!m_settings.audioSettings.masterMixer) return;
            m_settings.audioSettings.masterVolumeParameter = EditorGUILayout.TextField("Master Volume Parameter", m_settings.audioSettings.masterVolumeParameter);

            try
            {
                if (!m_settings.audioSettings.masterMixer.GetFloat(m_settings.audioSettings.masterVolumeParameter,
                        out _))
                {
                    // Warning if the parameter is not found
                    EditorGUILayout.HelpBox("Master Volume Parameter not found in the master mixer.", MessageType.Error);
                }
                else
                {
                    // Slider for the master volume
                    m_settings.audioSettings.masterVolume =
                        EditorGUILayout.Slider("Master Volume", m_settings.audioSettings.masterVolume, 0, 1);
                }
            } catch (IndexOutOfRangeException)
            {
                // Warning if the master group is not found
                EditorGUILayout.HelpBox("Master group not found in the master mixer.", MessageType.Error);
            }
            EditorGUILayout.Space();
        }
        
        private void DrawAudioProfilesContainer()
        {
            // Horizontal line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Audio Profiles", m_titleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            DrawAudioProfiles();
            if (!m_showNewProfile)
            {
                if (GUILayout.Button("Add New Audio Profile"))
                {
                    m_newProfile = new AudioProfile();
                    m_showNewProfile = true;
                }
            }
            else
            {
                DrawNewProfileForm();
            }
        }
        
        private void DrawAudioProfiles()
        {
            if (m_settings.audioSettings.audioProfiles.Count == 0)
            {
                EditorGUILayout.HelpBox("No audio profiles found.", MessageType.Info);
            }
            else
            {
                EditorGUI.indentLevel++;
                foreach (var profile in m_settings.audioSettings.audioProfiles)
                {
                    if (!m_audio_foldouts.ContainsKey(profile))
                    {
                        m_audio_foldouts.Add(profile, false);
                    }
                    
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    EditorGUILayout.BeginHorizontal();
                    var hasMixer = profile.mixerGroup != null;
                    m_audio_foldouts[profile] =
                        EditorGUILayout.Foldout(m_audio_foldouts[profile], hasMixer ? profile.mixerGroup.name : "No Mixer Found");
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        RemoveProfile(profile);
                        return;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (m_audio_foldouts[profile])
                    {
                        // Disable gui
                        if (!hasMixer)
                        {
                            EditorGUILayout.HelpBox("No mixer group found for this profile, Please select one, or remove the profile.", MessageType.Warning);
                        }
                        EditorGUI.BeginDisabledGroup(hasMixer);
                        profile.mixerGroup = (AudioMixerGroup) EditorGUILayout.ObjectField("Mixer Group", profile.mixerGroup, typeof(AudioMixerGroup), false);
                        EditorGUI.EndDisabledGroup();
                        profile.volume = EditorGUILayout.Slider("Default Volume", profile.volume, 0, 1);
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawNewProfileForm()
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("New Audio Profile", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Draw info text
            EditorGUILayout.HelpBox("Create a new audio profile to store audio settings for different parts of the game.", MessageType.Info);
            // Draw a horizontal line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // Draw the name field
            m_newProfile.createNew = EditorGUILayout.Toggle("Create New Mixer Group", m_newProfile.createNew);
            if (m_newProfile.createNew)
            {
                if (string.IsNullOrEmpty(m_newProfile.newName)) m_newProfile.newName = "New Profile";
                m_newProfile.newName = EditorGUILayout.TextField("Name", m_newProfile.newName);
            }
            else
            {
                // Draw the mixer group field
                m_newProfile.mixerGroup = (AudioMixerGroup) EditorGUILayout.ObjectField("Mixer Group", m_newProfile.mixerGroup, typeof(AudioMixerGroup), false);
            }
            
            // Draw the volume field
            m_newProfile.volume = EditorGUILayout.Slider("Volume", m_newProfile.volume, 0, 1);
            
            EditorGUILayout.Space();
            
            // Draw the add and cancel buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                AddNewProfile();
            }
            if (GUILayout.Button("Cancel"))
            {
                m_showNewProfile = false;
                m_newProfile = new AudioProfile();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void AddNewProfile()
        {
            var mixer = m_settings.audioSettings.masterMixer;
            if (mixer == null)
            {
                return;
            }
            
            if (m_newProfile.mixerGroup == null)
            {

                if (string.IsNullOrEmpty(m_newProfile.newName) || !m_newProfile.createNew)
                {
                    // Show popup with error message
                    EditorUtility.DisplayDialog("Error", "Please Select a Mixer Group or enable 'Create New Mixer Group' and enter a name.", "Ok");
                    return;
                }
                
                // Check in the reflection cache if the mixer group exists and has a function called "CreateNewGroup"
                // If it does, call the function and assign the result to the mixer group field
                // var controller = mixer as UnityEditor.Audio.AudioMixerController;
                // Controller.CreateNewGroup("New Group", bool storeUndo = true);
                // AddChildToParent => NEW GROUP, MASTER
                // AddGroupToCurrentView
                var method = mixer.GetType().GetMethod("CreateNewGroup");
                if (method != null)
                {
                    var newGroup = (AudioMixerGroup) method.Invoke(mixer, new object[] {m_newProfile.newName, true});
                    method = mixer.GetType().GetMethod("AddChildToParent");
                    if (method != null)
                    {
                        method.Invoke(mixer, new object[] {newGroup, mixer.FindMatchingGroups("Master")[0]});
                    }
                    
                    method = mixer.GetType().GetMethod("AddGroupToCurrentView");
                    if (method != null)
                    {
                        method.Invoke(mixer, new object[] {newGroup});
                    }
                    
                    m_newProfile.mixerGroup = newGroup;
                    Debug.Log("Created new group for the audio profile.");
                    
                    // Refresh the asset database
                    AssetDatabase.Refresh();
                    // Save the changes
                    EditorUtility.SetDirty(mixer);
                    // Reload the editor
                    AssetDatabase.SaveAssets();
                }
            }
            
            m_settings.AddAudioProfile(m_newProfile);
            m_showNewProfile = false;
            m_newProfile = new AudioProfile();
        }
        
        /// <summary>
        /// No clue in hell what happened here, but it works do not touch it!.
        /// </summary>
        /// <param name="profile"></param>
        private void RemoveProfile(AudioProfile profile)
        {
            var mixerGroup = profile.mixerGroup;
            if (mixerGroup != null)
            {
                var method = mixerGroup.audioMixer.GetType().GetMethod("GetAllAudioGroupsSlow");
                if (method != null)
                {
                    // Wish c# had "any"
                    var groups = method.Invoke(mixerGroup.audioMixer, null);
                     
                    if (groups == null)
                    {
                        Debug.LogWarning("No groups found in the mixer.");
                        return;
                    }
                    
                    // convert object to list using reflection
                    // Debug.Log(groups.GetType()); // System.Collections.Generic.List`1[UnityEditor.Audio.AudioMixerGroupController] but it is internal
                    
                    foreach (var group in (IEnumerable)groups)
                    {
                        if (group.ToString() == mixerGroup.name)
                        {
                            method = m_settings.audioSettings.masterMixer.GetType().GetMethod("DeleteGroups");

                            if (method != null)
                            {
                                var type = group.GetType();
                                // Create an array of AudioMixerGroupControllers
                                var array = Array.CreateInstance(type, 1);
                                array.SetValue(group, 0);
                                // Call the DeleteGroups method with the array as an argument
                                method.Invoke(m_settings.audioSettings.masterMixer, new object[] {array});
                            }
                            break;
                        }
                    }
                }
            }
            m_settings.audioSettings.audioProfiles.Remove(profile);
            
            // Refresh the asset database
            AssetDatabase.Refresh();
            // Save the changes
            EditorUtility.SetDirty(m_settings.audioSettings.masterMixer);
            // Reload the editor
            AssetDatabase.SaveAssets();
        }
        
        #endregion
    }
}
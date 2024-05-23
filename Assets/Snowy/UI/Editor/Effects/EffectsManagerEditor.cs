using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Snowy.UI.Effects
{
    enum EventType
    {
        Hover,
        Click
    }

    
    [CustomEditor(typeof(EffectsManager))]
    public class EffectsManagerEditor : Editor
    {
        EventType m_eventType = EventType.Hover;
        EffectsManager m_effectsManager;
        
        // List of effects with a foldout for each effect
        private Dictionary<Effect, bool> m_foldouts = new Dictionary<Effect, bool>();

        private GUIStyle m_boxStyle;
        
        void OnEnable()
        {
            m_effectsManager = (EffectsManager) target;
            
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
        }
        
        public override void OnInspectorGUI()
        {
            // Draw two areas one for OnHover and one for OnClick
            // Box -> Foldout -> Button
            // Tabs for all the events
            
            // Draw the tabs
            m_eventType = (EventType) GUILayout.Toolbar((int) m_eventType, new[] {"Hover", "Click"});
            switch (m_eventType)
            {
                case EventType.Hover:
                    DrawEffectArea("On Hover", m_effectsManager.onHover);
                    break;
                case EventType.Click:
                    DrawEffectArea("On Click", m_effectsManager.onClick);
                    break;
            }
        }

        void DrawEffectArea(string label, EffectsGroup effectsGroup)
        {
            // Draw a dark box with a label and a foldout button
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(label);
            if (effectsGroup == null)
            {
                EditorGUILayout.HelpBox("EffectsGroup is null", MessageType.Error);
                return;
            }

            if (effectsGroup.isParallel)
            {
                EditorGUILayout.HelpBox("Parallel Effects", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Sequential Effects", MessageType.Info);
            }
            effectsGroup.isParallel = EditorGUILayout.Toggle("Parallel", effectsGroup.isParallel);
            
            EditorGUILayout.Space();
            
            var effects = effectsGroup.GetEffects();
            if (effects != null)
            {
                for (int i = 0; i < effects.Length; i++)
                {
                    // A foldout for each effect
                    var effect = effects[i];
                    m_foldouts.TryGetValue(effect, out var foldout);
                    
                    EditorGUILayout.BeginHorizontal();
                    m_foldouts[effect] = EditorGUILayout.Foldout(foldout, effect.GetType().Name);
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        effectsGroup.RemoveEffect(effect);
                        m_foldouts.Remove(effect);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    if (!foldout)
                    {
                        continue;
                    }
                    
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel++;
                    // Draw the serialized properties of a normal effect
                    DrawEffectProperties(effect);
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUILayout.Space();

            if (GUILayout.Button("Add Effect"))
            {
                // Show a context menu with all the effects
                var menu = new GenericMenu();
                foreach (var effectType in System.Enum.GetValues(typeof(EffectType)))
                {
                    // Cannout use ref or out parameters in lambdas
                    var type = (EffectType) effectType;
                    menu.AddItem(new GUIContent(type.ToString()), false, () =>
                    {
                        var effect = effectsGroup.AddEffect(type);
                        m_foldouts.Add(effect, true);
                    });
                }
                
                menu.ShowAsContext();
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawEffectProperties(Effect effect)
        {
            // Draw the properties of the effect
            // FadeEffect -> Duration
            // ScaleEffect -> Duration
            // RotateEffect -> Duration
            // MoveEffect -> Duration
            // ColorizeEffect -> Duration
            switch (effect)
            {
                case FadeEffect fadeEffect:
                    fadeEffect.duration = EditorGUILayout.FloatField("Duration", fadeEffect.duration);
                    fadeEffect.to = EditorGUILayout.FloatField("To", fadeEffect.to);
                    break;
                case ScaleEffect scaleEffect:
                    scaleEffect.duration = EditorGUILayout.FloatField("Duration", scaleEffect.duration);
                    scaleEffect.to = EditorGUILayout.Vector3Field("To", scaleEffect.to);
                    break;
                case RotateEffect rotateEffect:
                    rotateEffect.duration = EditorGUILayout.FloatField("Duration", rotateEffect.duration);
                    rotateEffect.to = EditorGUILayout.Vector3Field("To", rotateEffect.to);
                    break;
                case MoveEffect moveEffect:
                    moveEffect.duration = EditorGUILayout.FloatField("Duration", moveEffect.duration);
                    moveEffect.to = EditorGUILayout.Vector3Field("To", moveEffect.to);
                    break;
                case ColorizeEffect colorizeEffect:
                    colorizeEffect.duration = EditorGUILayout.FloatField("Duration", colorizeEffect.duration);
                    colorizeEffect.to = EditorGUILayout.ColorField("To", colorizeEffect.to);
                    break;
            }
        }
    }
}
using UnityEditor;
using UnityEngine;

namespace Snowy.UI.DefaultElements
{
    [CustomEditor(typeof(SnButton), true)]
    public class SnButtonEditor : SnSelectableEditor
    {
        private SnButton m_button;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_button = (SnButton) target;
        }

        public override void OnInspectorGUI()
        {
            if (m_button == null) { return; }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Button Settings", m_titleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            // Serialize the onclick serialized property
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onClick"));
            serializedObject.ApplyModifiedProperties();
            
            base.OnInspectorGUI();
        }
    }
}
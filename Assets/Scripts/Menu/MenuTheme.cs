using TMPro;
using UnityEngine;

namespace Menu
{
    [CreateAssetMenu(fileName = "MenuTheme", menuName = "Menu/Menu Theme")]
    public class MenuTheme : ScriptableObject
    {
        [Title("Audio")]
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip buttonHover;
        [SerializeField] private AudioClip menuOpen;
        [SerializeField] private AudioClip menuClose;
        [SerializeField] private AudioClip menuBack;
        
        [Title("Logo")]
        [SerializeField] private Color logoFontColor;
        [SerializeField] private TMP_FontAsset logoFont;
        [SerializeField] private float logoFontSize;
        
        [Title("Button")]
        [SerializeField] private Color primaryColor;
        [SerializeField] private Color secondaryColor;
        
        [Title("Button Text")]
        [SerializeField] private Color buttonTextColor;
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private float fontSize;
        
        public Color PrimaryColor => primaryColor;
        public Color SecondaryColor => secondaryColor;
        public Color ButtonTextColor => buttonTextColor;
        public Color LogoFontColor => logoFontColor;
        
        public TMP_FontAsset LogoFont => logoFont;
        public float LogoFontSize => logoFontSize;
        public TMP_FontAsset Font => font;
        public float FontSize => fontSize;
        
        public AudioClip ButtonClick => buttonClick;
        public AudioClip ButtonHover => buttonHover;
        public AudioClip MenuOpen => menuOpen;
        public AudioClip MenuClose => menuClose;
        public AudioClip MenuBack => menuBack;
        
        # if UNITY_EDITOR
        private void OnValidate()
        {
            if (MenuManager.Instance)
            {
                MenuManager.Instance.UpdatedTheme(this);
            }
        }
        
        # endif
    }
}
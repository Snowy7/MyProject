using System;
using System.Collections.Generic;
using Audio;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Menu
{
    [AddComponentMenu("Menu/SnButton", 30)]
    [RequireComponent(typeof(Button), typeof(EventTrigger))]
    public class SnButton : MonoBehaviour, ISelectHandler
    {
        [Title("References")]
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private Image subImage;
        [SerializeField] private Button button;
        [SerializeField] private Image buttonImage;
        
        [Title("Button Settings")]
        [SerializeField] private string text;
        [SerializeField] private Sprite image;
        [SerializeField] private bool interactable;
        [SerializeField] private bool useCustomTheme;
        [SerializeField, ShowIf(nameof(useCustomTheme), true)] private Color primaryColor;
        [SerializeField, ShowIf(nameof(useCustomTheme), true)] private Color secondaryColor;
        [SerializeField, ShowIf(nameof(useCustomTheme), true)] private Color textColor;
        
        [Title("Audio Settings")]
        [Help("To enable custom sounds, enable both play sound and useCustomTheme")]
        [SerializeField] private bool playSound;
        private bool ShowSounds => playSound && useCustomTheme;
        // Hover sound, click sound
        [SerializeField, HideIf(nameof(ShowSounds), false)] private AudioClip hoverSound;
        [SerializeField, HideIf(nameof(ShowSounds), false)] private AudioClip clickSound;
        [SerializeField, HideIf(nameof(ShowSounds), false)] private EventTrigger eventTrigger;

        
        private ColorBlock colorBlock;
        
        # if UNITY_EDITOR
        public enum Behaviour
        {
            None,
            Quit,
            LoadScene,
            OpenMenu,
            Custom,
        }
        
        [Serializable]public class ButtonData
        {
            public string text;
            public Sprite image;
            public bool interactable;
            public bool useCustomTheme;
            public Color primaryColor;
            public Color secondaryColor;
            public bool playSound;
            public AudioClip hoverSound;
            public AudioClip clickSound;
            
            public Behaviour behaviour;
            public object BehaviourData;
            public UnityEvent behaviourEvent;
            public List<(int, object)> ListerArguments = new List<(int, object)>();
            public bool foldout;
        }
        
        
        private void OnValidate()
        {
            Init();
        }
        
        public ButtonData GetButtonData()
        {
            return new ButtonData
            {
                text = text,
                image = image,
                interactable = interactable,
                useCustomTheme = useCustomTheme,
                primaryColor = primaryColor,
                secondaryColor = secondaryColor,
                playSound = playSound,
                hoverSound = hoverSound,
                clickSound = clickSound,
            };
        }
        
        public void LoadButtonData(ButtonData data)
        {
            text = data.text;
            image = data.image;
            interactable = data.interactable;
            useCustomTheme = data.useCustomTheme;
            primaryColor = data.primaryColor;
            secondaryColor = data.secondaryColor;
            playSound = data.playSound;
            hoverSound = data.hoverSound;
            clickSound = data.clickSound;
            
            Init();

            if (button)
            {
                switch (data.behaviour)
                {
                    case Behaviour.None:
                        break;
                    case Behaviour.Quit:
                        // Add the listener visually
                        var quitMethod = MenuManager.Instance.GetType().GetMethod("Quit");
                        if (quitMethod == null)
                        {
                            return;
                        }
                        var quitAction = (UnityAction) Delegate.CreateDelegate(typeof(UnityAction), MenuManager.Instance, quitMethod);
                        UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, quitAction);
                        break;
                    case Behaviour.LoadScene:
                        var loadSceneMethod = MenuManager.Instance.GetType().GetMethod("LoadScene");
                        if (loadSceneMethod == null)
                        {
                            return;
                        }
                        var loadSceneAction = (UnityAction<string>) Delegate.CreateDelegate(typeof(UnityAction<string>), MenuManager.Instance, loadSceneMethod);
                        UnityEditor.Events.UnityEventTools.AddStringPersistentListener(button.onClick, loadSceneAction, (string) data.BehaviourData);
                        break;
                    case Behaviour.OpenMenu:
                        var openMenuMethod = MenuManager.Instance.GetType().GetMethod("OpenMenuWithIndex");
                        if (openMenuMethod == null)
                        {
                            return;
                        }
                        var openMenuAction = (UnityAction<int>) Delegate.CreateDelegate(typeof(UnityAction<int>), MenuManager.Instance, openMenuMethod);
                        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(button.onClick, openMenuAction, (int) data.BehaviourData);
                        break;
                    case Behaviour.Custom:
                        if (data.behaviourEvent != null)
                        {
                            Debug.Log($"Adding custom event to button with amount of listeners: {data.behaviourEvent.GetPersistentEventCount()}");
                            for (int i = 0; i < data.behaviourEvent.GetPersistentEventCount(); i++)
                            {
                                var target = data.behaviourEvent.GetPersistentTarget(i);
                                var methodName = data.behaviourEvent.GetPersistentMethodName(i);
                                var method = target.GetType().GetMethod(methodName);
                                if (method == null)
                                {
                                    return;
                                }
                                var arg = data.ListerArguments[i];
                                var mode = (PersistentListenerMode) arg.Item1;
                                // Switch to the correct listener mode
                                switch (mode)
                                {
                                    case PersistentListenerMode.Void:
                                        var action = (UnityAction) Delegate.CreateDelegate(typeof(UnityAction), target, method);
                                        UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, action);
                                        break;
                                    case PersistentListenerMode.Object:
                                        var actionObject = (UnityAction<Object>) Delegate.CreateDelegate(typeof(UnityAction<Object>), target, method);
                                        UnityEditor.Events.UnityEventTools.AddObjectPersistentListener(button.onClick, actionObject, (Object) arg.Item2);
                                        break;
                                    case PersistentListenerMode.Int:
                                        var actionInt = (UnityAction<int>) Delegate.CreateDelegate(typeof(UnityAction<int>), target, method);
                                        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(button.onClick, actionInt, (int) arg.Item2);
                                        break;
                                    case PersistentListenerMode.Float:
                                        var actionFloat = (UnityAction<float>) Delegate.CreateDelegate(typeof(UnityAction<float>), target, method);
                                        UnityEditor.Events.UnityEventTools.AddFloatPersistentListener(button.onClick, actionFloat, (float) arg.Item2);
                                        break;
                                    case PersistentListenerMode.String:
                                        var actionString = (UnityAction<string>) Delegate.CreateDelegate(typeof(UnityAction<string>), target, method);
                                        UnityEditor.Events.UnityEventTools.AddStringPersistentListener(button.onClick, actionString, (string) arg.Item2);
                                        break;
                                    case PersistentListenerMode.Bool:
                                        var actionBool = (UnityAction<bool>) Delegate.CreateDelegate(typeof(UnityAction<bool>), target, method);
                                        UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(button.onClick, actionBool, (bool) arg.Item2);
                                        break;
                                        
                                }
                            }
                        }
                        break;
                }
            }
        }
        
        # endif

        private void Start()
        {
            Init();
            
            button.onClick.AddListener(OnClick);

            EventTrigger.Entry entry = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            eventTrigger.triggers.Add(entry);
            eventTrigger.triggers[0].callback.AddListener(_ => { OnHover(); });
            
            // On Mouse Exit
            EventTrigger.Entry exit = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            eventTrigger.triggers.Add(exit);
            eventTrigger.triggers[1].callback.AddListener(_ => { SetToNormal(); });
        }

        private void Init()
        {
            
            if (eventTrigger == null)
            {
                eventTrigger = GetComponent<EventTrigger>() ?? gameObject.AddComponent<EventTrigger>();
            }
            
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            
            
            if (buttonImage == null)
            {
                buttonImage = GetComponent<Image>();
            }
            
            if (buttonText == null)
            {
                buttonText = GetComponentInChildren<TMP_Text>();
            }
            
            if (buttonText != null)
            {
                buttonText.text = text;
            }
            
            if (subImage != null)
            {
                subImage.color = textColor;
            }
            
            if (buttonImage != null)
            {
                buttonImage.sprite = image;
            }
            
            if (button != null)
            {
                colorBlock = button.colors;
                colorBlock.normalColor = primaryColor;
                colorBlock.highlightedColor = secondaryColor;
                colorBlock.pressedColor = secondaryColor;
                colorBlock.selectedColor = secondaryColor;
                button.colors = colorBlock;
                button.interactable = interactable;
            }
        }

        private void OnClick()
        {
            // Play sound
            if (playSound)
            {
                AudioClip clip = useCustomTheme ? clickSound : MenuManager.Instance.GetTheme().ButtonClick;
                SoundManager.Instance.PlayUISound(clip);
            }
        }
        
        private void OnHover()
        {
            // Play sound
            if (playSound)
            {
                AudioClip clip = useCustomTheme ? hoverSound : MenuManager.Instance.GetTheme().ButtonHover;
                SoundManager.Instance.PlayUISound(clip);
            }
        }

        private void SetToNormal()
        {
            // Set the button to normal
            button.OnDeselect(null);
        }
        
        public void AddListener(UnityAction action)
        {
            button.onClick.AddListener(action);
        }

        public void OnSelect(BaseEventData eventData)
        {
            OnHover();
        }
        
        public void LoadFromTheme(MenuTheme theme)
        {
            if (useCustomTheme || !theme) return;
            primaryColor = theme.PrimaryColor;
            secondaryColor = theme.SecondaryColor;
            textColor = theme.ButtonTextColor;
            
            
            colorBlock = button.colors;
            colorBlock.normalColor = primaryColor;
            colorBlock.highlightedColor = secondaryColor;
            colorBlock.pressedColor = secondaryColor;
            colorBlock.selectedColor = secondaryColor;
            button.colors = colorBlock;

            if (buttonText)
            {
                buttonText.color = textColor;
                buttonText.font = theme.Font;
                buttonText.enableAutoSizing = true;
                buttonText.fontSizeMax = theme.FontSize;
            }

            if (subImage)
            {
                subImage.color = textColor;
            }
            
        }

        public void SetIntractable(bool b)
        {
            interactable = b;
            button.interactable = b;
        }
    }
}
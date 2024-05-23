using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menu
{
    public class MenuObject : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] HorizontalOrVerticalLayoutGroup buttonGroup;
        [SerializeField] private SnButton backButton;
        [SerializeField] SnButton[] buttons;
        
        [Title("Settings")]
        [SerializeField] private string menuTitle;
        [SerializeField] private int spacing;
        [SerializeField] private bool isDefault;
        
        MenuTheme theme;
        
        # if UNITY_EDITOR
        
        public class MenuData
        {
            public string menuTitle = "Menu";
            public int spacing = 10;
            public bool isDefault;
            public bool hasBackButton;
        }
        
        private void OnValidate()
        {
            Init();
        }
        
        public void LoadMenuData(MenuData data)
        {
            menuTitle = data.menuTitle;
            spacing = data.spacing;
            isDefault = data.isDefault;
            
            if (data.hasBackButton)
            {
                if (!backButton)
                {
                    Debug.Log("No back button found, add one to the menu object.");
                    return;
                }
                
                if (backButton)
                {
                    backButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (backButton)
                {
                    DestroyImmediate(backButton.gameObject);
                    backButton = null;
                }
            }
        }
        
        public SnButton AddButton(SnButton button, SnButton.ButtonData data)
        {
            // Spawn button
            var newButton = Instantiate(button, buttonGroup.transform);
            newButton.transform.SetParent(buttonGroup.transform);

            if (data.behaviour == SnButton.Behaviour.OpenMenu)
            {
                if (isDefault)
                {
                    data.BehaviourData = (int)data.BehaviourData + 1;
                }
            }
            
            // Set button data
            newButton.LoadButtonData(data);
            
            // Add button to array
            buttons = buttons.Append(newButton).ToArray();
            
            // Button theme
            newButton.LoadFromTheme(theme);
            
            return newButton;
        }
        
        #endif
        
        private void Init()
        {
            if (!buttonGroup) buttonGroup = GetComponentInChildren<VerticalLayoutGroup>();
            buttonGroup.spacing = spacing;
            
            // Remove null buttons
            buttons = buttons.Where(b => b).ToArray();
            
            // Check for buttons
            foreach (var button in gameObject.GetComponentsInChildren<SnButton>())
            {
                // if is in the array
                if (buttons.Contains(button)) continue;
                
                // Add button to array
                buttons = buttons.Append(button).ToArray();
            }
        }
        
        public void SetTheme(MenuTheme newTheme)
        {
            if (!this) return;
            theme = newTheme;
            if (!theme) return;
            foreach (var button in buttons)
            {
                if (!button) continue;
                button.LoadFromTheme(theme);
            }
        }

        private void OnEnable()
        {
            FocusOnDefault();
        }

        public void FocusOnDefault()
        {
            if (!EventSystem.current) return;
            // Focus on the first button
            if (buttons.Length > 0)
            {
                EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            }
            else
            {
                if (backButton)
                {
                    EventSystem.current.SetSelectedGameObject(backButton.gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            // Remove from the MenuManager
            if (MenuManager.Instance)
            {
                MenuManager.Instance.RemoveMenu(this);
            }
        }
        
        public void Back()
        {
            MenuManager.Instance.GoToPreviousMenu();
        }
        
        public bool IsDefault => isDefault;
    }
}
using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using Utils.Attributes;

namespace Menu
{
    [Serializable] public class Menu
    {
        public string menuName = "Menu";
        public MenuObject menuObject;
        [ReadOnly] public int previousMenuIndex = -1;
        
        public void SetActive(bool active)
        {
            if (menuObject)
            {
                menuObject.gameObject.SetActive(active);
            }
        }
    }

    public class MenuManager : MonoBehaviour
    {
        private static MenuManager _menuManager;
        public static MenuManager Instance {
            get
            {
                if (_menuManager == null)
                {
                    _menuManager = FindFirstObjectByType<MenuManager>();
                }
                
                return _menuManager;
            }
            private set => _menuManager = value;
        }
        [SerializeField, InLineEditor] MenuTheme theme;
        
        [Title("Debug Variables"), Space(10)]
        [SerializeField] bool debugMode;
        [SerializeField, ReorderableListExposed(ListStyle = ListStyle.Lined), ShowIf(nameof(debugMode), true)] public List<Menu> menus = new List<Menu>();
        [SerializeField, EnableIf(nameof(debugMode), true)] private int defaultMenu;
        [SerializeField, ReadOnly] private int currentMenuIndex = -1;
        
        public MenuObject CurrentMenu {
            get
            {
                if (currentMenuIndex < 0 || currentMenuIndex >= menus.Count)
                {
                    return null;
                }
                return menus[currentMenuIndex].menuObject;
            }
        }
        
        # if UNITY_EDITOR
        private void OnValidate()
        {
            if (theme)
            {
                foreach (var menu in menus)
                {
                    if (menu.menuObject) menu.menuObject.SetTheme(theme);
                }
            }
        }
        
        public void UpdatedTheme(MenuTheme updatedTheme)
        {
            if (theme == updatedTheme)
            {
                theme = updatedTheme;
                foreach (var menu in menus)
                {
                    if (menu.menuObject) menu.menuObject.SetTheme(theme);
                }
            }
        }
        # endif
        
        private void Awake()
        {
            if (Instance == null || Instance == this)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (menus.Count == 0)
            {
                Debug.LogWarning("No menus available");
                return;
            }
            
            if (defaultMenu < 0 || defaultMenu >= menus.Count)
            {
                defaultMenu = 0;
                Debug.LogWarning("Default menu index is out of range, setting to 0");
            }
            
            currentMenuIndex = defaultMenu;
            
            OpenMenuWithIndex(defaultMenu);
        }

        public void OpenMenuWithIndex(int index)
        {
            OpenMenu(index, true);
        }
        
        public void OpenMenu(int index, bool playSound, bool registerPrevious = true)
        {
            foreach (var menu in menus)
            {
                if (menu.menuName == menus[index].menuName)
                {
                    if (registerPrevious && index != defaultMenu)
                    {
                        menu.previousMenuIndex = currentMenuIndex;
                    }
                    currentMenuIndex = index;
                    if (playSound)
                    {
                        SoundManager.Instance.PlaySound(theme.MenuOpen);
                    }
                    menu.SetActive(true);
                }
                else
                {
                    menu.SetActive(false);
                }
            }
        }
        
        public void AddMenu(MenuObject menuObject)
        {
            menus.Add(new Menu
            {
                menuName = menuObject.name,
                menuObject = menuObject,
            });
            
            // Setup theme
            menuObject.SetTheme(theme);
            
            Debug.Log($"Menu {menuObject.name} added to the menu manager");
            
            if (menuObject.IsDefault)
            {
                SetDefaultMenu(menus.Count - 1);
            }
        }
        
        public void RemoveMenu(MenuObject menuObject)
        {
            for (int i = 0; i < menus.Count; i++)
            {
                if (menus[i].menuObject == menuObject)
                {
                    menus.RemoveAt(i);
                    break;
                }
            }
        }

        public void Hide()
        {
            var menu = menus[currentMenuIndex];
            menu.SetActive(false);
        }
        
        public void Show()
        {
            var menu = menus[currentMenuIndex];
            menu.SetActive(true);
        }
        
        public void GoToPreviousMenu()
        {
            if (menus[currentMenuIndex].previousMenuIndex != -1)
            {
                OpenMenu(menus[currentMenuIndex].previousMenuIndex, false, false);
            }
            else
            {
                OpenMenu(defaultMenu, false, false);
            }
            
            SoundManager.Instance.PlaySound(theme.MenuBack);
        }
        
        public void SetDefaultMenu(int index)
        {
            // Rearrage so that the default menu is the first one
            var menu = menus[index];
            menus.RemoveAt(index);
            menus.Insert(0, menu);
         
            defaultMenu = 0;
            Debug.Log($"Default menu set to {menu.menuName}");
        }
        
        public (int, Menu) GetDefaultMenu()
        {
            if (defaultMenu < 0 || defaultMenu >= menus.Count)
            {
                defaultMenu = menus.Count > 0 ? 0 : -1;
            }
            return (defaultMenu, menus.Count > 0 ? menus[defaultMenu] : null);
        }

        public void Quit()
        {
            # if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            # else
            Application.Quit();
            # endif
        }
        
        public void LoadScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        
        public MenuTheme GetTheme()
        {
            return theme;
        }
    }
}
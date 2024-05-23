using SnInput;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Menu
{
    public class CustomEventSystem : MonoBehaviour
    {
        InputAction cancelAction;
        private bool canLook = true;
        protected void Start()
        {
            // Set the current control scheme to the gamepad
            InputManager.Instance.ListenToSchemeChange((scheme) =>
            {
                if (scheme == "Gamepad")
                {
                    if (!EventSystem.current) return;
                    if (!EventSystem.current.currentSelectedGameObject)
                    {
                        if (MenuManager.Instance)
                        {
                            if (MenuManager.Instance.CurrentMenu)
                            {
                                MenuManager.Instance.CurrentMenu.FocusOnDefault();
                                return;
                            }
                        }
                        
                        EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
                    }
                    
                    // Lock the cursor
                    Cursor.visible = false;
                }
                else
                {
                    // Unlock the cursor
                    Cursor.visible = true;
                }
            });
            
            var module = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if (module)
            {
                cancelAction = module.cancel.action;
            }
        }

        protected void Update()
        {
            // Check if the current active input is the gamepad
           if (InputManager.Instance.GetCurrentControlScheme() == "Gamepad")
           {
               // If nothing is selected, select the first button
               if (!EventSystem.current.currentSelectedGameObject)
               {
                   EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
               }
               
               Cursor.visible = false;
           } 
           else
           {
               Cursor.visible = true;
           }
           
           if (cancelAction != null && cancelAction.triggered)
           {
               // if was pressed rn
               if (MenuManager.Instance)
               {
                   if (MenuManager.Instance.CurrentMenu)
                   {
                       MenuManager.Instance.CurrentMenu.Back();
                   }
               }
           }
            
        }
    }
}
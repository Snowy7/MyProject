using System;
using System.Collections.Generic;
using Snowy.UI.interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Snowy.UI
{
    [Serializable] public class SnButtonEvent : UnityEvent { }
    
    public class SnButton : SnSelectable, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField] private SnButtonEvent onClick = new SnButtonEvent();
        
        public SnButtonEvent OnClick
        {
            get => onClick;
            set => onClick = value;
        }
        
        private void Press()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.onClick", this);
            onClick.Invoke();
        }
        
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Press();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !IsInteractable())
                return;
        }
    }
}
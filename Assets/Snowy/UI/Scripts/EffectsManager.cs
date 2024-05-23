using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Snowy.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class EffectsManager : EventTrigger, IEffectsManager
    {
        public EffectsGroup onHover;
        public EffectsGroup onClick;
        
        private CanvasGroup m_canvasGroup;
        Image m_image;
        private bool m_isHovered;
        // not being used yet
        # pragma warning disable 414
        private bool m_isClicked;
        # pragma warning restore 414
        
        public CanvasGroup CanvasGroup => m_canvasGroup;
        public Image Image => m_image;
        public Transform Transform => transform;
        
        public MonoBehaviour Mono => this;
        
        private void Awake()
        {
            m_canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            m_image = GetComponent<Image>();
            
            if (onHover == null) onHover = new EffectsGroup(false);
            if (onClick == null) onClick = new EffectsGroup(false);
            
            onHover.Initialize(this);
            onClick.Initialize(this);
        }
        
        # if UNITY_EDITOR
        private void OnValidate()
        {
            if (onHover == null) onHover = new EffectsGroup(false);
            if (onClick == null) onClick = new EffectsGroup(false);
            
            m_canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            m_image = GetComponent<Image>();
        }
        
        # endif

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            m_isHovered = true;
            if (onHover == null) return;
            StopAllCoroutines();
            StartCoroutine(onHover.Apply(this));
        }
        
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            m_isHovered = false;
            if (onHover == null) return;
            StopAllCoroutines();
            StartCoroutine(onHover.Cancel(this));
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            m_isClicked = true;
            if (onClick == null) return;
            StopAllCoroutines();
            StartCoroutine(onClick.Apply(this));
        }
        
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            m_isClicked = false;
            if (onClick == null) return;
            StopAllCoroutines();
            StartCoroutine(CancelClick());
        }

        private IEnumerator CancelClick()
        {
            yield return onClick.Cancel(this);
            if (m_isHovered)
            {
                StopAllCoroutines();
                StartCoroutine(onHover.Apply(this));
            }
        }
    }
}
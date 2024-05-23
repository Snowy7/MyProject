using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Snowy.UI
{
    public enum EffectType
    {
        Fade,
        Scale,
        Rotate,
        Move,
        Colorize,
    }
    
    public interface IEffectsManager
    {
        public Transform Transform { get; }
        public CanvasGroup CanvasGroup { get; }
        public Image Image { get; }
        public MonoBehaviour Mono { get; }
    }
    
    public interface IEffect
    {
        bool IsPlaying { get; }
        public void Initialize(IEffectsManager manager);
        public IEnumerator Apply(IEffectsManager manager);
        public IEnumerator Cancel(IEffectsManager manager);
        public void ImmediateCancel(IEffectsManager manager);
    }
    
    // Parallel and Sequential Effects
    [Serializable] public class EffectsGroup : IEffect
    {
        [SerializeReference] Effect[] effects;
        public bool IsPlaying { get; private set; }
        public bool isParallel;
        
        public void Initialize(IEffectsManager manager)
        {
            foreach (var effect in effects)
            {
                effect.Initialize(manager);
            }
        }
        
        public EffectsGroup(bool isParallel)
        {
            this.isParallel = isParallel;
            effects = new Effect[0];
        }
        
        public IEnumerator Apply(IEffectsManager manager)
        {
            IsPlaying = true;
            if (isParallel)
            {
                foreach (var effect in effects)
                {
                    manager.Mono.StartCoroutine(effect.Apply(manager));
                }
                yield return new WaitUntil(() => effects.All(effect => !effect.IsPlaying));
            }
            else
            {
                foreach (var effect in effects)
                {
                    yield return effect.Apply(manager);
                }
            }
            IsPlaying = false;
        }
        
        public IEnumerator Cancel(IEffectsManager manager)
        {
            foreach (var eff in effects)
            {
                if (eff.IsPlaying)
                {
                    manager.Mono.StopCoroutine(eff.Apply(manager));
                }
            }
            
            IsPlaying = true;
            if (isParallel)
            {
                foreach (var effect in effects)
                {
                    manager.Mono.StartCoroutine(effect.Cancel(manager));
                }
                
                yield return new WaitUntil(() => effects.All(effect => !effect.IsPlaying));
            }
            else
            {
                foreach (var effect in effects)
                {
                    yield return effect.Cancel(manager);
                }
            }
            IsPlaying = false;
        }
        
        public void ImmediateCancel(IEffectsManager manager)
        {
            foreach (var effect in effects)
            {
                effect.ImmediateCancel(manager);
            }
        }
        
        public Effect[] GetEffects()
        {
            return effects;
        }
        
        public Effect AddEffect(EffectType effectType)
        {
            Effect effect;
            switch (effectType)
            {
                case EffectType.Fade:
                    effect = new FadeEffect();
                    break;
                case EffectType.Scale:
                    effect = new ScaleEffect();
                    break;
                case EffectType.Rotate:
                    effect = new RotateEffect();
                    break;
                case EffectType.Move:
                    effect = new MoveEffect();
                    break;
                case EffectType.Colorize:
                    effect = new ColorizeEffect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effectType), effectType, null);
            }
            
            Array.Resize(ref effects, effects.Length + 1);
            effects[^1] = effect;
            
            return effect;
        }
        
        public void RemoveEffect(Effect effect)
        {
            effects = effects.Where(e => e != effect).ToArray();
        }
        
        public void ClearEffects()
        {
            effects = new Effect[0];
        }
    }
    
    // UI transition Effects like Fade, Scale, Rotate, etc.
    [Serializable] public abstract class Effect : IEffect
    {
        public float duration = 0.1f;
        public bool IsPlaying { get; protected set; }
        
        public abstract void Initialize(IEffectsManager manager);
        
        public abstract IEnumerator Apply(IEffectsManager manager);
        
        public abstract IEnumerator Cancel(IEffectsManager manager);

        public abstract void ImmediateCancel(IEffectsManager manager);
    }
    
    [Serializable] public class FadeEffect : Effect
    {
        public float to = 1f;
        
        private float m_from;
        
        public override void Initialize(IEffectsManager manager)
        {
            m_from = manager.CanvasGroup.alpha;
        }
        
        public override IEnumerator Apply(IEffectsManager manager)
        {
            IsPlaying = true;
            float time = 0f;
            var canvasGroup = manager.CanvasGroup;
            var from = canvasGroup.alpha;
            while (time < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(from, to, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            manager.CanvasGroup.alpha = to;
            IsPlaying = false;
        }
        
        public override IEnumerator Cancel(IEffectsManager manager)
        {
            IsPlaying = true;
            float time = 0f;
            var canvasGroup = manager.CanvasGroup;
            var from = canvasGroup.alpha;
            
            // Calculate the new duration based on the ration between the current alpha and the target alpha
            var dur = duration * Mathf.Abs((from - m_from) / (to - m_from));
            
            while (time < dur)
            {
                canvasGroup.alpha = Mathf.Lerp(from, m_from, time / dur);
                time += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = m_from;
            IsPlaying = false;
        }
        
        public override void ImmediateCancel(IEffectsManager manager)
        {
            manager.CanvasGroup.alpha = m_from;
            IsPlaying = false;
        }
    }
    
    [Serializable] public class ScaleEffect : Effect
    {
        public Vector3 to = Vector3.one;
        
        private Vector3 m_from;
        
        public override void Initialize(IEffectsManager manager)
        {
            m_from = manager.Transform.localScale;
        }
        
        public override IEnumerator Apply(IEffectsManager manager)
        {
            IsPlaying = true;
            float time = 0f;
            var transform = manager.Transform;
            var from = manager.Transform.localScale;
            while (time < duration)
            {
                transform.localScale = Vector3.Lerp(from, to, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            transform.localScale = to;
            IsPlaying = false;
        }
        
        public override IEnumerator Cancel(IEffectsManager manager)
        {
            IsPlaying = true;
            float time = 0f;
            var transform = manager.Transform;
            var from = manager.Transform.localScale;
            
            // Calculate the new duration based on the ration between the current scale and the target scale
            var dur = duration * Mathf.Abs((from.magnitude - m_from.magnitude) / (to.magnitude - m_from.magnitude));
            
            while (time < dur)
            {
                transform.localScale = Vector3.Lerp(from, m_from, time / dur);
                time += Time.deltaTime;
                yield return null;
            }
            transform.localScale = m_from;
            IsPlaying = false;
        }
        
        public override void ImmediateCancel(IEffectsManager manager)
        {
            manager.Transform.localScale = m_from;
            IsPlaying = false;
        }
    }
    
    [Serializable] public class RotateEffect : Effect
    {
        public Vector3 to = Vector3.zero;
        
        private Vector3 m_from;
        
        public override void Initialize(IEffectsManager manager)
        {
            m_from = manager.Transform.localEulerAngles;
        }
        
        public override IEnumerator Apply(IEffectsManager manager)
        {
            IsPlaying = true;
            float time = 0f;
            var transform = manager.Transform;
            var from = manager.Transform.localEulerAngles;
            while (time < duration)
            {
                transform.localEulerAngles = Vector3.Lerp(from, to, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            transform.localEulerAngles = to;
            IsPlaying = false;
        }
        
        public override IEnumerator Cancel(IEffectsManager manager)
        {
            IsPlaying = true;
            float time = 0f;
            var transform = manager.Transform;
            var from = manager.Transform.localEulerAngles;
            
            // Calculate the new duration based on the ration between the current rotation and the target rotation
            var dur = duration * Mathf.Abs((from.magnitude - m_from.magnitude) / (to.magnitude - m_from.magnitude));
            
            while (time < dur)
            {
                transform.localEulerAngles = Vector3.Lerp(from, m_from, time / dur);
                time += Time.deltaTime;
                yield return null;
            }
            transform.localEulerAngles = m_from;
            IsPlaying = false;
        }
        
        public override void ImmediateCancel(IEffectsManager manager)
        {
            manager.Transform.localEulerAngles = m_from;
            IsPlaying = false;
        }
    }
    
    [Serializable] public class MoveEffect : Effect
    {
        public Vector3 to = Vector3.zero;
        
        private Vector3 m_from;
        
        public override void Initialize(IEffectsManager manager)
        {
            m_from = manager.Transform.localPosition;
        }
        
        public override IEnumerator Apply(IEffectsManager manager)
        {
            IsPlaying = true;
            float time = 0f;
            var transform = manager.Transform;
            var from = manager.Transform.localPosition;
            
            while (time < duration)
            {
                // Offset the target position by the current position
                transform.localPosition = Vector3.Lerp(from, m_from + to, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = to;
            IsPlaying = false;
        }
        
        public override IEnumerator Cancel(IEffectsManager manager)
        {
            IsPlaying = true;
            float time = 0f;
            var transform = manager.Transform;
            var from = manager.Transform.localPosition;
            var finalTo = m_from + this.to;
            
            // Calculate the new duration based on the ration between the current position and the target position
            var dur = duration * Mathf.Abs((from.magnitude - m_from.magnitude) / (finalTo.magnitude - m_from.magnitude));
            
            while (time < dur)
            {
                transform.localPosition = Vector3.Lerp(from, m_from, time / dur);
                time += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = m_from;
            IsPlaying = false;
        }
        
        public override void ImmediateCancel(IEffectsManager manager)
        {
            manager.Transform.localPosition = m_from;
            IsPlaying = false;
        }
    }
    
    [Serializable] public class ColorizeEffect : Effect
    {
        public Color to = Color.white;
        
        private Color m_from;
        
        public override void Initialize(IEffectsManager manager)
        {
            var image = manager.Image;
            if (image == null) return;
            m_from = image.color;
        }
        
        public override IEnumerator Apply(IEffectsManager manager)
        {
            IsPlaying = true;
            var image = manager.Image;
            if (image == null) yield break;
            
            var from = image.color;
            float time = 0f;
            while (time < duration)
            {
                image.color = Color.Lerp(from, to, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            
            image.color = to;
            IsPlaying = false;
        }
        
        public override IEnumerator Cancel(IEffectsManager manager)
        {
            IsPlaying = true;
            var image = manager.Image;
            if (image == null) yield break;
            
            var from = image.color;
            float time = 0f;
            
            // Calculate the new duration based on the ration between the current color and the target color
            var dur = duration * Mathf.Abs((from.r - m_from.r) / (to.r - m_from.r));
            
            while (time < dur)
            {
                image.color = Color.Lerp(from, m_from, time / dur);
                time += Time.deltaTime;
                yield return null;
            }
            
            image.color = m_from;
            IsPlaying = false;
        }
        
        public override void ImmediateCancel(IEffectsManager manager)
        {
            var image = manager.Image;
            if (image == null) return;
            image.color = m_from;
            IsPlaying = false;
        }
    }
    
    [Serializable] public class AnimateScaleEffect : Effect
    {
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        private Vector3 m_from;
        
        public override void Initialize(IEffectsManager manager)
        {
            m_from = manager.Transform.localScale;
        }
        
        public override IEnumerator Apply(IEffectsManager manager)
        {
            IsPlaying = true;
            float time = 0f;
            var transform = manager.Transform;
            while (time < duration)
            {
                transform.localScale = m_from * curve.Evaluate(time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            transform.localScale = m_from * curve.Evaluate(1);
            IsPlaying = false;
        }
        
        public override IEnumerator Cancel(IEffectsManager manager)
        {
            IsPlaying = true;
            float time = 0f;
            var transform = manager.Transform;
            var from = manager.Transform.localScale;
            
            // Calculate the new duration based on the ration between the current scale and the target scale
            var dur = duration * Mathf.Abs((from.magnitude - m_from.magnitude) / (m_from.magnitude));
            
            while (time < dur)
            {
                transform.localScale = from * curve.Evaluate(1 - time / dur);
                time += Time.deltaTime;
                yield return null;
            }
            transform.localScale = m_from;
            IsPlaying = false;
        }
        
        public override void ImmediateCancel(IEffectsManager manager)
        {
            manager.Transform.localScale = m_from;
            IsPlaying = false;
        }
    }
}
using System;
using System.Diagnostics;
using UnityEngine;

namespace Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class AnimatorStateAttribute : PropertyAttribute
    {
        public string animatorRef;
        
        public AnimatorStateAttribute(string animatorRef)
        {
            this.animatorRef = animatorRef;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class CharacterAnimatorStateAttribute : PropertyAttribute
    {
        public string animatorRef;
        
        public CharacterAnimatorStateAttribute(string animatorRef)
        {
            this.animatorRef = animatorRef;
        }
    }
}
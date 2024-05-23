using System;
using System.Diagnostics;
using UnityEngine;

namespace Menu.Attribute
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class MenuIndexAttribute : PropertyAttribute { }
}
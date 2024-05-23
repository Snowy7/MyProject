using System;
using System.Diagnostics;
using UnityEngine;

namespace Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class ReadOnlyAttribute : ToolboxConditionAttribute
    { }
}
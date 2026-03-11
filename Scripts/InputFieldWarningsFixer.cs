using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace IngameDebugConsole
{
    // Fixes: https://github.com/yasirkula/UnityIngameDebugConsole/issues/77
    // This was caused by Canvas.ForceUpdateCanvases in InputField.UpdateLabel (added in 2022.1 to resolve another bug: https://issuetracker.unity3d.com/issues/input-fields-width-doesnt-change-after-entering-specific-combinations-of-text-when-the-content-size-fitter-is-used)
    // which is triggered from InputField.OnValidate. UpdateLabel isn't invoked if a variable called m_PreventFontCallback is true,
    // which is what this component is doing: temporarily switching that variable before InputField.OnValidate to avoid this issue.
    #if UNITY_2022_1_OR_NEWER && UNITY_EDITOR
    [DefaultExecutionOrder(-50)]
    #endif
    /// <summary>
    /// Editor-only workaround for a Unity 2022.1+ issue where <c>InputField.OnValidate</c> triggers
    /// <c>Canvas.ForceUpdateCanvases</c> and floods the console with layout-rebuild warnings.
    /// Temporarily disables the internal <c>m_PreventFontCallback</c> flag via reflection before the call.
    /// </summary>
    public class InputFieldWarningsFixer : MonoBehaviour
    {
        #if UNITY_2022_1_OR_NEWER && UNITY_EDITOR
        private static readonly FieldInfo preventFontCallback = typeof(InputField).GetField("m_PreventFontCallback", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        protected void OnValidate()
        {
            if (preventFontCallback != null && TryGetComponent(out InputField inputField))
            {
                preventFontCallback.SetValue(inputField, true);
                EditorApplication.delayCall += () => preventFontCallback.SetValue(inputField, false);
            }
        }
        #endif
    }
}
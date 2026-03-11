#if IDG_ENABLE_HELPER_COMMANDS
using UnityEngine;

namespace IngameDebugConsole.Commands
{
	/// <summary>
	///     Built-in console commands for manipulating <see cref="UnityEngine.Time.timeScale" />.
	///     Registered under the <c>IDG_ENABLE_HELPER_COMMANDS</c> scripting define.
	/// </summary>
	public class TimeCommands
	{
		/// <summary>Sets <see cref="UnityEngine.Time.timeScale" /> to <paramref name="value" />, clamped to ≥ 0.</summary>
		[ConsoleMethod( "time.scale", "Sets the Time.timeScale value" ), UnityEngine.Scripting.Preserve]
		public static void SetTimeScale( float value )
		{
			Time.timeScale = Mathf.Max( value, 0f );
		}

		/// <summary>Returns the current <see cref="UnityEngine.Time.timeScale" />.</summary>
		[ConsoleMethod( "time.scale", "Returns the current Time.timeScale value" ), UnityEngine.Scripting.Preserve]
		public static float GetTimeScale()
		{
			return Time.timeScale;
		}
	}
}
#endif
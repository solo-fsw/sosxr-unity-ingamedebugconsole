#if !UNITY_EDITOR && UNITY_WEBGL
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IngameDebugConsole
{
	/// <summary>
	/// WebGL-only component that bridges the copy-to-clipboard flow via JS interop.
	/// It invokes the native JS functions <c>IngameDebugConsoleStartCopy</c> and <c>IngameDebugConsoleCancelCopy</c>.
	/// </summary>
	public class DebugLogItemCopyWebGL : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		[DllImport( "__Internal" )]
		private static extern void IngameDebugConsoleStartCopy( string textToCopy );
		[DllImport( "__Internal" )]
		private static extern void IngameDebugConsoleCancelCopy();

		private DebugLogItem logItem;

		/// <summary>Associates this copy helper with the owning <see cref="DebugLogItem"/>.</summary>
		public void Initialize( DebugLogItem logItem )
		{
			this.logItem = logItem;
		}

		public void OnPointerDown( PointerEventData eventData )
		{
			string log = logItem.GetCopyContent();
			if( !string.IsNullOrEmpty( log ) )
				IngameDebugConsoleStartCopy( log );
		}

		public void OnPointerUp( PointerEventData eventData )
		{
			if( eventData.dragging )
				IngameDebugConsoleCancelCopy();
		}
	}
}
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


// Listens to scroll events on the scroll rect that debug items are stored
// and decides whether snap to bottom should be true or not
// 
// Procedure: if, after a user input (drag or scroll), scrollbar is at the bottom, then 
// snap to bottom shall be true, otherwise it shall be false
namespace IngameDebugConsole
{
    /// <summary>
    /// Monitors scroll-wheel and scrollbar-drag events on the log <see cref="UnityEngine.UI.ScrollRect"/> to maintain the
    /// snap-to-bottom flag on <see cref="DebugLogManager"/>.
    /// </summary>
    public class DebugsOnScrollListener : MonoBehaviour, IScrollHandler, IBeginDragHandler, IEndDragHandler
    {
        public ScrollRect debugsScrollRect;
        public DebugLogManager debugLogManager;


        /// <summary>Disables snap-to-bottom when the user begins manually dragging the content.</summary>
        public void OnBeginDrag(PointerEventData data)
        {
            debugLogManager.SnapToBottom = false;
        }


        /// <summary>Re-enables snap-to-bottom after a drag if the scrollbar has reached the bottom.</summary>
        public void OnEndDrag(PointerEventData data)
        {
            debugLogManager.SnapToBottom = IsScrollbarAtBottom();
        }


        /// <summary>Updates snap-to-bottom state after each scroll-wheel event.</summary>
        public void OnScroll(PointerEventData data)
        {
            debugLogManager.SnapToBottom = IsScrollbarAtBottom();
        }


        /// <summary>Disables snap-to-bottom when the user grabs the scrollbar thumb.</summary>
        public void OnScrollbarDragStart(BaseEventData data)
        {
            debugLogManager.SnapToBottom = false;
        }


        /// <summary>Re-enables snap-to-bottom after the scrollbar thumb is released, if the position is at the bottom.</summary>
        public void OnScrollbarDragEnd(BaseEventData data)
        {
            debugLogManager.SnapToBottom = IsScrollbarAtBottom();
        }


        private bool IsScrollbarAtBottom()
        {
            return debugsScrollRect.verticalNormalizedPosition <= 1E-6f;
        }
    }
}
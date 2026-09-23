using UnityEngine;
using UnityEngine.EventSystems;

// Listens to drag event on the DebugLogManager's resize button
namespace IngameDebugConsole
{
	public class DebugLogResizeListener : MonoBehaviour, IBeginDragHandler, IDragHandler
	{
		[SerializeField]
		private DebugLogManager debugManager;

		// This interface must be implemented in order to receive drag events
		void IBeginDragHandler.OnBeginDrag( PointerEventData eventData )
		{
		}

		void IDragHandler.OnDrag( PointerEventData eventData )
		{
			debugManager.Resize( eventData );
		}
	}
}
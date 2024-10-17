using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCore.Components
{
    public class UIDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Graphic graphic;
        private Vector2 offset;
        private bool isDragging = false;

        void Start()
        {
            graphic ??= GetComponent<Graphic>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            graphic.transform.SetAsLastSibling();
            graphic.GetDragOffset(Global.UI.GetUILayer(0), eventData, out offset);
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            graphic.DragUI(Global.UI.GetUILayer(0), eventData, offset);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }
    }
}
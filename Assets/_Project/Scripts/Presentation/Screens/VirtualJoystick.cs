using UnityEngine;
using UnityEngine.EventSystems;

namespace StreetLegends.Presentation.Screens
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 100f;

        public Vector2 Direction { get; private set; }
        public bool IsActive { get; private set; }

        private Vector2 _pointerOffset;

        private void Start()
        {
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
            Direction = Vector2.zero;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsActive = true;
            UpdateHandle(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateHandle(eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsActive = false;
            Direction = Vector2.zero;
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
        }

        private void UpdateHandle(Vector2 screenPosition)
        {
            if (background == null || handle == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, screenPosition, null, out Vector2 localPoint);

            Vector2 clamped = Vector2.ClampMagnitude(localPoint, handleRange);
            handle.anchoredPosition = clamped;
            Direction = clamped / handleRange;
        }
    }
}

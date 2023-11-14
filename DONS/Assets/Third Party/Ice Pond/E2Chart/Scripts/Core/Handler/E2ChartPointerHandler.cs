using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace E2C
{
    public class E2ChartPointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public UnityAction onPointerEnter;
        public UnityAction onPointerExit;
        public UnityAction<Vector2> onPointerDown;
        public UnityAction<Vector2> onPointerHover;

        Camera eventCam;
        bool m_hovering = false;

        public RectTransform rectTransform { get => (RectTransform)transform; }
        public bool IsHovering { get => m_hovering; }

        private void Update()
        {
            if (m_hovering)
            {
                if (onPointerHover != null) onPointerHover.Invoke(GetMousePosition());
            }
        }

        public Vector2 GetMousePosition()
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, eventCam, out mousePos);
            return mousePos;
        }

        public Vector2 GetMousePosition(RectTransform rectTrans)
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTrans, Input.mousePosition, eventCam, out mousePos);
            return mousePos;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            eventCam = eventData.pressEventCamera;
            if (onPointerDown != null) onPointerDown.Invoke(GetMousePosition());
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_hovering = true;
            eventCam = eventData.enterEventCamera;
            if (onPointerEnter != null) onPointerEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_hovering = false;
            if (onPointerExit != null) onPointerExit.Invoke();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace E2C
{
    public class E2ChartPointerDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public UnityAction<Vector2> onBeginDrag;
        public UnityAction<Vector2> onDragging;

        Camera eventCam;
        Vector2 beginPos, dragPos;

        public RectTransform rectTransform { get => (RectTransform)transform; }

        public Vector2 dragDistance { get => dragPos - beginPos; }

        public Vector2 GetMousePosition()
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, eventCam, out mousePos);
            return mousePos;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            eventCam = eventData.pressEventCamera;
            beginPos = GetMousePosition();
            if (onBeginDrag != null) onBeginDrag.Invoke(beginPos);
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragPos = GetMousePosition();
            if (onDragging != null) onDragging.Invoke(dragDistance);
        }

        public void OnEndDrag(PointerEventData eventData)
        {

        }
    }
}
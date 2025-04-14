using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI References")]
    public RectTransform background;
    public RectTransform handle;

    [Header("Settings")]
    public float handleLimit = 1f; // Typically normalized.

    // Current input vector (normalized to [-1, 1])
    private Vector2 inputVector = Vector2.zero;

    // Event to broadcast input changes
    public event Action<Vector2> InputValueChanged;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            // Normalize the coordinates (assumes pivot in center)
            localPoint.x /= background.sizeDelta.x;
            localPoint.y /= background.sizeDelta.y;

            // Multiply by 2 to cover the full range [-1, 1]
            inputVector = new Vector2(localPoint.x * 2, localPoint.y * 2);
            inputVector = (inputVector.magnitude > 1f) ? inputVector.normalized : inputVector;

            // Move the handle based on the input vector
            handle.anchoredPosition = new Vector2(
                inputVector.x * (background.sizeDelta.x / 2),
                inputVector.y * (background.sizeDelta.y / 2));

            // Broadcast the updated input value
            InputValueChanged?.Invoke(inputVector);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        InputValueChanged?.Invoke(inputVector);
    }
    
    // Optional accessor properties
    public float Horizontal => inputVector.x;
    public float Vertical => inputVector.y;
}
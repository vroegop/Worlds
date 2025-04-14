using UnityEngine;
using UnityEngine.EventSystems;

public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private BoolEvent onButtonStateChanged = new BoolEvent();

    // Expose the UnityEvent so you can assign listeners in the Editor if needed.
    public BoolEvent OnButtonStateChanged => onButtonStateChanged;

    public void OnPointerDown(PointerEventData eventData)
    {
        onButtonStateChanged.Invoke(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onButtonStateChanged.Invoke(false);
    }
}
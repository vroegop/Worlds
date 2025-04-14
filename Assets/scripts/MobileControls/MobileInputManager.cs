using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Vector2Event : UnityEvent<Vector2> { }
[System.Serializable]
public class BoolEvent : UnityEvent<bool> { }

public class MobileInputManager : MonoBehaviour
{
    [Header("Joysticks")]
    public Joystick leftJoystick;
    public Joystick rightJoystick;
    
    [Header("Buttons")]
    public MobileButton upButton;
    public MobileButton downButton;
    public MobileButton leftButton;
    public MobileButton rightButton;
    
    [Header("Joystick Events")]
    [SerializeField]
    private Vector2Event onLeftJoystickMovement = new Vector2Event();
    [SerializeField]
    private Vector2Event onRightJoystickMovement = new Vector2Event();
    
    [Header("Button Events")]
    [SerializeField]
    private BoolEvent onUpButtonState = new BoolEvent();
    [SerializeField]
    private BoolEvent onDownButtonState = new BoolEvent();
    [SerializeField]
    private BoolEvent onLeftButtonState = new BoolEvent();
    [SerializeField]
    private BoolEvent onRightButtonState = new BoolEvent();

    // Expose the events as properties so they can be assigned or read from other scripts, if needed.
    public Vector2Event OnLeftJoystickMovement => onLeftJoystickMovement;
    public Vector2Event OnRightJoystickMovement => onRightJoystickMovement;
    public BoolEvent OnUpButtonState => onUpButtonState;
    public BoolEvent OnDownButtonState => onDownButtonState;
    public BoolEvent OnLeftButtonState => onLeftButtonState;
    public BoolEvent OnRightButtonState => onRightButtonState;

    private void Start()
    {
        // Subscribe to the joystick inputs and propagate them via UnityEvents.
        if (leftJoystick != null)
        {
            leftJoystick.InputValueChanged += (Vector2 input) =>
            {
                onLeftJoystickMovement.Invoke(input);
            };
        }
        if (rightJoystick != null)
        {
            rightJoystick.InputValueChanged += (Vector2 input) =>
            {
                onRightJoystickMovement.Invoke(input);
            };
        }
        
        // Subscribe to the button state changes and forward them.
        if (upButton != null)
        {
            upButton.OnButtonStateChanged.AddListener((bool state) =>
            {
                onUpButtonState.Invoke(state);
            });
        }
        if (downButton != null)
        {
            downButton.OnButtonStateChanged.AddListener((bool state) =>
            {
                onDownButtonState.Invoke(state);
            });
        }
        if (leftButton != null)
        {
            leftButton.OnButtonStateChanged.AddListener((bool state) =>
            {
                onLeftButtonState.Invoke(state);
            });
        }
        if (rightButton != null)
        {
            rightButton.OnButtonStateChanged.AddListener((bool state) =>
            {
                onRightButtonState.Invoke(state);
            });
        }
    }
}

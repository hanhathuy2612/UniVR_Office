//===========================================================================//
//                       FreeFlyCamera (Version 1.2)                         //
//                        (c) 2019 Sergey Stafeyev                           //
//===========================================================================//

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    #region UI
    [SerializeField]
    private LayerMask interactionLayer;

    [Space]

    [SerializeField]
    [Tooltip("The script is currently active")]
    private bool _active = true;

    [SerializeField]
    [Tooltip("Free Look Mode active by default")]
    private bool freelookByDefault = true;

    [Space]

    [SerializeField]
    [Tooltip("The script is currently active")]
    [Range(0.1f, 3f)]
    private float _transitionSpeed = 0.5f;

    [Space]

    [SerializeField]
    [Tooltip("Camera rotation by mouse movement is active")]
    private bool _enableRotation = true;

    [SerializeField]
    [Tooltip("Sensitivity of mouse rotation")]
    private float _mouseSense = 1f;

    [Space]

    [SerializeField]
    [Tooltip("Camera zooming in/out by 'Mouse Scroll Wheel' is active")]
    private bool _enableTranslation = true;

    [SerializeField]
    [Tooltip("Velocity of camera zooming in/out")]
    private float _translationSpeed = 30f;

    [Space]

    [SerializeField]
    [Tooltip("Camera movement by 'W','A','S','D','Q','E' keys is active")]
    private bool _enableMovement = true;

    [SerializeField]
    [Tooltip("Camera movement speed")]
    private float _movementSpeed = 1f;

    [SerializeField]
    [Tooltip("Speed of the quick camera movement when holding the 'Left Shift' key")]
    private float _boostedSpeed = 2f;

    [SerializeField]
    [Tooltip("Move up")]
    private KeyCode _moveUp = KeyCode.E;

    [SerializeField]
    [Tooltip("Move down")]
    private KeyCode _moveDown = KeyCode.Q;

    [Space]

    [SerializeField]
    private float friction = 0.9f;

    [Space]

    [SerializeField]
    [Tooltip("This keypress will move the camera to initialization position")]
    private KeyCode _initPositonButton = KeyCode.R;

    [Space]

    [SerializeField]
    [Tooltip("Touch sensitive")]
    private float touchSensitive = 1.0f;

    [SerializeField]
    [Tooltip("Touch move speed factor")]
    private float touchMoveSpeed = 1.0f;

    [SerializeField]
    private float doubleTouchTime = 0.3f;

    [SerializeField]
    private float focusOnDoubleTouchSpeed = 10f;


    [Space]

    [SerializeField]
    [Tooltip("Enable block height base on ground object")]
    private bool blockHeight = false;

    [SerializeField]
    [Tooltip("Select base ground object")]
    private GameObject baseGround = null;

    [SerializeField]
    [Tooltip("Height in meter")]
    private float height = 1.7f;

    #endregion UI

    private GameObject canvas = null;
    private GameObject instuction = null;
    private GameObject cameraStatus = null;
    private GameObject controlPanel = null;
    private CursorLockMode _wantedMode;
    private bool _isEscape = false;
    private bool _onTransition = false;

    private bool onFocusView = false;
    private float _lerpTime = 0;

    private float touchTick = 0f;

    private float currentSpeed = 1.0f;
    private Vector3 movementVector = Vector3.zero;

    private Vector3 _initPosition;
    private Vector3 _initRotation;
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private Quaternion _startRotation;
    private Quaternion _endRotation;
    private float _startfieldOfView;
    private float _endfieldOfView;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_boostedSpeed < _movementSpeed)
            _boostedSpeed = _movementSpeed;
    }
#endif


    public void setCamera(Camera cam)
    {
        _startPosition = GetComponent<Camera>().transform.position;
        _endPosition = cam.transform.position;
        _startRotation = GetComponent<Camera>().transform.rotation;
        _endRotation = cam.transform.rotation;
        _startfieldOfView = GetComponent<Camera>().fieldOfView;
        _endfieldOfView = cam.fieldOfView;

        _lerpTime = 0.0f;
        _onTransition = true;

    }

    public void triggerCameraMode(){
        if (!_isEscape)
        { 
            if(_wantedMode == CursorLockMode.Locked)
            {
                _wantedMode = CursorLockMode.None;
            }
            else
            {
                _wantedMode = CursorLockMode.Locked;
                _onTransition = false;
            }
        }
    }

    private void Start()
    {
        _initPosition = transform.position;
        _initRotation = transform.eulerAngles;
        canvas = GameObject.Find("Canvas");
        instuction = GameObject.Find("CameraInstruction");
        cameraStatus = GameObject.Find("CameraStatus");
        controlPanel = GameObject.Find("ControlPanel");

        if (_transitionSpeed < 0)
        {
            _transitionSpeed = 0.5f;
        }

        updateCameraStatus();

        if(freelookByDefault){
            triggerCameraMode();
        }
    }

    private void OnEnable()
    {
        if (_active)
            _wantedMode = CursorLockMode.None;
    }

    // Set Camera status
    private void updateCameraStatus()
    {
        if(canvas != null){
            if (_wantedMode != CursorLockMode.Locked)
            {
                cameraStatus.GetComponent<Text>().text = "Locked";
                canvas.GetComponent<UI>().setInstructionUIEnabled(false);
            }
            else
            {
                cameraStatus.GetComponent<Text>().text = "Freelook";
                canvas.GetComponent<UI>().setInstructionUIEnabled(true);
            }

            if (_isEscape)
            {
                instuction.GetComponent<Text>().text = "Press [Left Mouse] to change";
            }
            else
            {
                instuction.GetComponent<Text>().text = "Press [Tab] to change";
            }
        }

    }

    // Apply requested cursor state
    private void SetCursorState()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_isEscape)
            {
                _isEscape = true;
                _wantedMode = CursorLockMode.None;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (_isEscape)
            {
                _isEscape = false;
                _wantedMode = CursorLockMode.Locked;
            }
            if(_wantedMode == CursorLockMode.None)
            {
                doRayCast(Input.mousePosition);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!_isEscape)
            { 
                if(_wantedMode == CursorLockMode.Locked)
                {
                    _wantedMode = CursorLockMode.None;
                }
                else
                {
                    _wantedMode = CursorLockMode.Locked;
                    _onTransition = false;
                }
            }
        }

        updateCameraStatus();

        // Apply cursor state
        Cursor.lockState = _wantedMode;
        // Hide cursor when locking
        Cursor.visible = (CursorLockMode.Locked != _wantedMode);
    }

    private void cameraTransition()
    {
        if (_onTransition)
        {
            if (_lerpTime < 1)
            {
                _lerpTime += _transitionSpeed * Time.deltaTime;
            }

            if (_lerpTime >= 1)
            {
                _lerpTime = 1.0f;
                _onTransition = false;
            }

            GetComponent<Camera>().transform.position = Vector3.Lerp(_startPosition, _endPosition, _lerpTime);
            GetComponent<Camera>().transform.rotation = Quaternion.Lerp(_startRotation, _endRotation, _lerpTime);
            GetComponent<Camera>().fieldOfView = Mathf.Lerp(_startfieldOfView, _endfieldOfView, _lerpTime);

        }else if(onFocusView){
            if (_lerpTime < 1)
            {
                _lerpTime += focusOnDoubleTouchSpeed * Time.deltaTime;
            }

            if (_lerpTime >= 1)
            {
                _lerpTime = 1.0f;
                onFocusView = false;
            }

            GetComponent<Camera>().transform.rotation = Quaternion.Slerp(_startRotation, _endRotation, _lerpTime);
        }
    }

    private void Update()
    {
        
        if(Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began){
                doRayCast(touch.position);
            }
        }

        // Free Camm Settings
        if (!_active)
            return;

        SetCursorState();
        cameraTransition();

        if (Cursor.visible)
            return;

        // Translation
        if (_enableTranslation)
        {
            transform.Translate(Vector3.forward * Input.mouseScrollDelta.y * Time.deltaTime * _translationSpeed);
        }

        // Movement
        if (_enableMovement)
        {
            Vector3 deltaPosition = movementVector;

            if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
                deltaPosition = Vector3.zero;
                if (Input.GetKey(KeyCode.LeftShift))
                    currentSpeed = _boostedSpeed;
                else
                    currentSpeed = _movementSpeed;
            }else {
                currentSpeed *= friction;
            }

            if (Input.GetKey(KeyCode.W))
                deltaPosition += transform.forward;

            if (Input.GetKey(KeyCode.S))
                deltaPosition -= transform.forward;

            if (Input.GetKey(KeyCode.A))
                deltaPosition -= transform.right;

            if (Input.GetKey(KeyCode.D))
                deltaPosition += transform.right;

            if (Input.GetKey(_moveUp))
                deltaPosition += transform.up;

            if (Input.GetKey(_moveDown))
                deltaPosition -= transform.up;



            // Touch movement
            // if(Input.touchCount > 1){
            //     Touch touchMove = Input.GetTouch(0);
            //     Vector2 deltaPos = touchMove.deltaPosition;
            //     deltaPosition -= transform.forward * deltaPos.y * touchMoveSpeed;
            //     deltaPosition -= transform.right * deltaPos.x * touchMoveSpeed;
            // }

            
            if(blockHeight){
                Vector3 scaleHeightVector = transform.position;
                scaleHeightVector[1] = baseGround.transform.position[1] + height;
                transform.position = scaleHeightVector;
                deltaPosition[1] = 0;
            }

            transform.position += deltaPosition * currentSpeed * Time.deltaTime;

            movementVector = deltaPosition;
        }

        // Rotation
        if (_enableRotation)
        {
            // Pitch
            transform.rotation *= Quaternion.AngleAxis(
                -Input.GetAxis("Mouse Y") * _mouseSense,
                Vector3.right
            );

            // Paw
            transform.rotation = Quaternion.Euler(
                transform.eulerAngles.x,
                transform.eulerAngles.y + Input.GetAxis("Mouse X") * _mouseSense,
                transform.eulerAngles.z
            );

            
            // if(Input.touchCount == 1){
            //     Touch touchRotate = Input.GetTouch(0);


            //     if(touchRotate.phase == TouchPhase.Began){
            //         if(Time.time - touchTick <= doubleTouchTime){
            //             focusViewOnDoubleTouch();
            //         }

            //         touchTick = Time.time;
            //     }
            //     else if(touchRotate.phase == TouchPhase.Moved){
            //         transform.rotation *= Quaternion.AngleAxis(
            //             -touchRotate.deltaPosition.y * touchSensitive,
            //             Vector3.right
            //         );

            //         transform.rotation = Quaternion.Euler(
            //             transform.eulerAngles.x,
            //             transform.eulerAngles.y + touchRotate.deltaPosition.x * touchSensitive,
            //             transform.eulerAngles.z
            //         );
            //     }
                
            // }
        }

        // Return to init position
        if (Input.GetKeyDown(_initPositonButton))
        {
            transform.position = _initPosition;
            transform.eulerAngles = _initRotation;
        }
    }

    private void focusViewOnDoubleTouch(){
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);


        _startRotation = transform.rotation;
        _endRotation = Quaternion.LookRotation(ray.direction);

        _lerpTime = 0f;
         _onTransition = false;
        onFocusView = true;

    }

    private void doRayCast(Vector3 dir)
    {
        RaycastHit hit;
        Ray ray = GetComponent<Camera>().ScreenPointToRay(dir);
        
        if (Physics.Raycast(ray, out hit, 10, interactionLayer))
        {
            TouchSwitchButton button = hit.transform.GetComponent<TouchSwitchButton>();
            button?.onPress();
        }
    }

}

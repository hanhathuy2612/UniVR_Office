using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchCamera : MonoBehaviour
{

    [SerializeField] private float _mouseSense = 1.0f;
    [SerializeField] private float _touchSense = 1.0f;
    [SerializeField] private float _movingSpeed = 1.0f;
    [SerializeField] private float _zoomSpeed = 1.0f;
    [SerializeField] private float _maxZoomDistance = 5.0f;
    [SerializeField] private float _height = 1.6f;
    [SerializeField] private float _falling = 0.02f;
    [SerializeField] private LayerMask _blocklayer;
    private Vector3 _heightOffset;
    private Vector3 _gravity;
    private Vector3 _targetPosition;

    private Vector3 _mouseClick;
    private Quaternion _currentRotation;
    private Transform _targetRotation;

    private bool _clicked = false;
    private bool _zoom = false;
    private bool _isRotate = false;
    private float _distance = 0f;
    private float _doubleTouchTime = 0.3f;
    private float _touchTick = 0f;

    void Start()
    {
        _heightOffset = new Vector3(0, _height, 0);
        _gravity = new Vector3(0, -_falling, 0);
        _targetPosition = transform.position;
        _currentRotation = transform.rotation;
        _targetRotation = transform;
    }

    void Update()
    {

        _zoom = false;

        if (Input.touchCount == 2)
        {
            _zoom = true;
        }

        if (!_zoom)
        {
            Transform preTransform = transform;
            bool _boost = 180 - Vector3.Angle(transform.forward, _targetPosition - transform.position) < 10;
            float _speed = _boost ? 20 * _movingSpeed : _movingSpeed;
            preTransform.position = Vector3.MoveTowards(preTransform.position, _targetPosition, _speed * Time.deltaTime);
            transform.position = preTransform.position;
        }
        else
        {
            float dist = Vector3.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
            float delta = dist - _distance;

            if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began)
            {
                delta = 0;
            }

            if (Vector3.Distance(transform.position, _targetPosition) < _maxZoomDistance)
            {
                transform.Translate(Vector3.forward * delta * _zoomSpeed / 100);
            }

            _distance = dist;
        }


        if (Input.GetMouseButtonDown(0) && !Input.touchSupported)
        {
            if (Time.time - _touchTick <= _doubleTouchTime)
            {
                onDoubleClick();
            }
            _mouseClick = Input.mousePosition;
            _clicked = true;
            _touchTick = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _clicked = false;
        }

        if (_clicked)
        {
            Vector3 _deltaPosition = (Input.mousePosition - _mouseClick) / 10;
            transform.rotation *= Quaternion.AngleAxis(
                -_deltaPosition.y * _mouseSense,
                Vector3.right
            );

            transform.rotation = Quaternion.Euler(
                transform.eulerAngles.x,
                transform.eulerAngles.y + _deltaPosition.x * _mouseSense,
                transform.eulerAngles.z
            );

            _mouseClick = Input.mousePosition;
        }

        if (Input.touchCount == 1 && !_clicked)
        {
            Touch touchRotate = Input.GetTouch(0);

            if (touchRotate.phase == TouchPhase.Began)
            {
                if (Time.time - _touchTick <= _doubleTouchTime)
                {
                    onDoubleClick();
                }

                _touchTick = Time.time;
            }
            else if (touchRotate.phase == TouchPhase.Moved)
            {
                transform.rotation *= Quaternion.AngleAxis(
                    -touchRotate.deltaPosition.y * _touchSense / 10,
                    Vector3.right
                );

                transform.rotation = Quaternion.Euler(
                    transform.eulerAngles.x,
                    transform.eulerAngles.y + touchRotate.deltaPosition.x * _touchSense / 10,
                    transform.eulerAngles.z
                );
            }
        }
        else if (Input.touchCount == 2)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began)
            {
                _targetPosition = transform.position;
            }
        }


        if (Vector3.Distance(transform.position, _targetPosition) == 0f)
        {
            if (_isRotate)
            {
                rotateCamera(_targetRotation);

                if (Quaternion.Angle(transform.rotation, _targetRotation.rotation) <= 2f)
                {
                    _isRotate = false;
                }
            }
        }
    }

    void onDoubleClick()
    {
        RaycastHit hit;
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 50, _blocklayer))
        {
            if (hit.transform.tag == "Ground")
            {
                _targetPosition = hit.point + _heightOffset;
            }
            Debug.Log(hit.transform.tag);
            if (hit.transform.tag == "Partical")
            {
                _targetPosition = hit.transform.position + _heightOffset;
                _targetRotation = hit.transform.GetChild(0);
                Debug.Log(hit.transform.GetChild(0).name);
                _isRotate = true;
            }
        }

        Debug.Log("Double Click");
    }

    void rotateCamera(Transform target)
    {
        bool _boost = 180 - Vector3.Angle(transform.forward, target.forward) < 10;

        float speed = _boost ? 10f * Time.deltaTime : 1.5f * Time.deltaTime;

        transform.rotation =
            Quaternion
                .Slerp(transform.rotation, target.rotation, speed);
        Debug.Log(_currentRotation);
    }
}

using UnityEngine;

public class TargetSwitch : MonoBehaviour
{
    [SerializeField] Transform _targetRotate;
    Quaternion _currentRotation;

    bool _isRotate;
    private void Start()
    {
        _currentRotation = transform.rotation;
        if (_targetRotate == null)
        {
            _targetRotate = GameObject.FindGameObjectWithTag("TargetSwitch").transform;
        }
    }

    private void Update()
    {
        if (_isRotate)
        {
            transform.rotation = _currentRotation;
        }

        if (Quaternion.Angle(transform.rotation, _targetRotate.rotation) <= 5f)
        {
            _isRotate = false;
        }
    }

    void RotateCamera(Transform target)
    {
        _isRotate = true;

        bool _boost = 180 - Vector3.Angle(transform.forward, target.forward) < 10;

        float speed = _boost ? 10f * Time.deltaTime : 1.5f * Time.deltaTime;

        _currentRotation =
            Quaternion
                .Slerp(transform.rotation, target.rotation, speed);

    }

    
}

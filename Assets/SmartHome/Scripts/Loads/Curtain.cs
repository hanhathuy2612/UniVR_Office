using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curtain :  BMSEngine.Device
{

    [SerializeField] private Transform _leftLightCurtain;
    [SerializeField] private Transform _rightLightCurtain;
    [SerializeField] private Transform _leftDarkCurtain;
    [SerializeField] private Transform _rightDarkCurtain;
    [SerializeField] private float _maxScale = 1.0f;
    [SerializeField] private float _minScale = 1.0f;
    [SerializeField] private float _scaleSpeed = 1.0f;
    [SerializeField] private float _lightScale = 1.0f;
    [SerializeField] private float _darkScale = 1.0f;

    public Transform leftLightCurtain {get => _leftLightCurtain; set => _leftLightCurtain = value;}
    public Transform rightLightCurtain {get => _rightLightCurtain; set => _rightLightCurtain = value;}
    public Transform leftDarkCurtain {get => _leftDarkCurtain; set => _leftDarkCurtain = value;}
    public Transform rightDarkCurtain {get => _rightDarkCurtain; set => _rightDarkCurtain = value;}
    public float maxScale {get => _maxScale; set => _maxScale = value;}
    public float minScale {get => _minScale; set => _minScale = value;}
    public float scaleSpeed {get => _scaleSpeed; set => _scaleSpeed = value;}
    public float lightScale {get => _lightScale; set => _lightScale = value;}
    public float darkScale {get => _darkScale; set => _darkScale = value;}

    private void Start() {
        deviceType = BMSEngine.Device.DeviceType.Curtain;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 leftLightScaleTarget = _leftLightCurtain.localScale;
        Vector3 rightLightScaleTarget = _rightLightCurtain.localScale;
        Vector3 leftDarkScaleTarget = _leftDarkCurtain.localScale;
        Vector3 rightDarkScaleTarget = _rightDarkCurtain.localScale;

        leftLightScaleTarget.x = _lightScale;
        rightLightScaleTarget.x = -_lightScale;

        leftDarkScaleTarget.x = _darkScale;
        rightDarkScaleTarget.x = -_darkScale;

        _leftLightCurtain.localScale = Vector3.MoveTowards(_leftLightCurtain.localScale, leftLightScaleTarget, Time.deltaTime * _scaleSpeed);
        _rightLightCurtain.localScale = Vector3.MoveTowards(_rightLightCurtain.localScale, rightLightScaleTarget, Time.deltaTime * _scaleSpeed);

        _leftDarkCurtain.localScale = Vector3.MoveTowards(_leftDarkCurtain.localScale, leftDarkScaleTarget, Time.deltaTime * _scaleSpeed);
        _rightDarkCurtain.localScale = Vector3.MoveTowards(_rightDarkCurtain.localScale, rightDarkScaleTarget, Time.deltaTime * _scaleSpeed);
    }

    public void setCurtain(uint percentIn, uint percentOut){
        _lightScale = _maxScale - (_maxScale - _minScale)*((float)percentOut/100);
        _darkScale = _maxScale - (_maxScale - _minScale)*((float)percentIn/100);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightDevice : BMSEngine.Device{
    // Start is called before the first frame update

    [SerializeField] private Material _lightMaterial;
    [SerializeField] [Range(0.1f, 3f)] private float _dimmerSpeed = 0.5f;
    private bool _deviceOn = false;
    private bool _onDimmer = false;
    private float _maxIntensity;
    private float _dimmerTime = 0;
    private float _startIntensity = 0;
    private float _endIntensity = 0;
    private Light _lightComponent;
    

    void Start()
    {
        gameObject.layer = 0;
        deviceType = BMSEngine.Device.DeviceType.Light;
        _lightComponent =  GetComponent<Light>();
        _maxIntensity = _lightComponent.intensity;
        _lightComponent.intensity = 0;
        _deviceOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_onDimmer)
        {
            if (_dimmerTime < 1)
            {
                _dimmerTime += _dimmerSpeed * Time.deltaTime;
            }

            if (_dimmerTime >= 1)
            {
                _dimmerTime = 1.0f;
                _onDimmer = false;
                if (_endIntensity == 0f)
                {
                    _deviceOn = false;
                }
            }

            _lightComponent.intensity = Mathf.Lerp(_startIntensity, _endIntensity, _dimmerTime);
        }
    }


    // Toggle the light on or off
    //      Normal:  input < 0
    //      Dimmer: 0 <= input <= 100       input % of light
    //      Auto Dimmer: input > 100

    public override void message(string msg)
    {
        if(msg == "Toggle"){
                if (_deviceOn)
                {
                    _lightComponent.intensity = 0;
                    _onDimmer = false;
                    _deviceOn = false;
                }
                else
                {
                    _lightComponent.intensity = _maxIntensity;
                    _onDimmer = false;
                    _deviceOn = true;
                }
        }else if(msg == "ToggleDimmerOff"){
            _dimmerTime = 0;
            _startIntensity = _maxIntensity;
            _endIntensity = 0;
            _dimmerTime = (_maxIntensity - _lightComponent.intensity) / _maxIntensity;
            _onDimmer = true;
        }else if(msg == "ToggleDimmerOn"){
            _dimmerTime = 0;
            _startIntensity = 0;
            _endIntensity = _maxIntensity;
            _dimmerTime = _lightComponent.intensity / _maxIntensity;
            _onDimmer = true;
            _deviceOn = true;
        }
    }

    public override void setInt(string key, int value){

        
        if(key == "PowerState"){

            if(value < 0){
                message("Toggle");
            }else if(value > 100){
                if(value == 101){ //On
                    message("ToggleDimmerOn");
                }else if(value == 102){ // Off
                    message("ToggleDimmerOff");
                }
            }else{
                _dimmerTime = 0;
                _deviceOn = true;
                _onDimmer = true;
                _startIntensity = _lightComponent.intensity;
                _endIntensity = ((float)value / 100) * _maxIntensity;
                
                if(_startIntensity > _endIntensity){
                    _dimmerTime = (_maxIntensity - _lightComponent.intensity) / _maxIntensity;
                }else {
                    _dimmerTime = _lightComponent.intensity / _maxIntensity;
                }
            }
        }
    }
}

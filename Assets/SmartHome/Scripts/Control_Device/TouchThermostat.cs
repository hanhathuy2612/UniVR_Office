using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TouchThermostat : BMSEngine.Device
{
    // Start is called before the first frame update
    [SerializeField] private string _hardwareId;
    [SerializeField] private bool _isVirtual = true;
    [SerializeField] private AirConditioner airConditioner;
    
    [Space]

    [SerializeField] private MeshRenderer _digit1;
    [SerializeField] private MeshRenderer _digit2;
    [SerializeField] private MeshRenderer _digit3;
    [SerializeField] private MeshRenderer _units;
    [SerializeField] private MeshRenderer _room;
    [SerializeField] private MeshRenderer _fanAuto;
    [SerializeField] private MeshRenderer _fanSpeed;
    [SerializeField] private MeshRenderer _swingAuto;
    [SerializeField] private MeshRenderer _swingSpeed;
    [SerializeField] private MeshRenderer _modeAuto;
    [SerializeField] private MeshRenderer _modeCool;
    [SerializeField] private MeshRenderer _modeHeat;
    [SerializeField] private MeshRenderer _modeDry;
    [SerializeField] private MeshRenderer _modeFan;

    private ModeAc[] modes = new ModeAc[5];
    private Material[] _digit1Materials = new Material[7];
    private Material[] _digit2Materials = new Material[7];
    private Material[] _digit3Materials = new Material[7];
    private Material[] _unitsMaterials = new Material[2];
    private Material[] _roomMaterials = new Material[2];
    private Material[] _fanSpeedMaterials = new Material[4];
    private Material[] _swingSpeedMaterials = new Material[4];
    private Material[] _modeCharMaterials = new Material[5];
    private Material[] _modeBackgroundMaterials = new Material[5];
    private Material _fanAutoMaterial;
    private Material _swingAutoMaterial;


    private Color _ledSegmentOn = new Color32(0xff, 0xff, 0xff, 0xff);
    private Color _ledSegmentOff = new Color32(0x20, 0x20, 0x20, 0xff);
    private AirConditionerState _AcState;
    private SlivingDeviceSim.Devices.AirCond _netAC;
    private float _timeOutStamp = 0f;
    private float _currentTemperature = 30f;
    private float _currentHumidity = 50f;
    private bool _displayTemperature = true;
    public bool isVirtual { get => _isVirtual; set => _isVirtual = value; }
    public SlivingDeviceSim.Devices.AirCond netAC {get => _netAC; set => _netAC = value;}

    private byte[] ledSegment = new byte[10]{0xC0, 0xF9, 0xA4, 0xB0, 0x99, 0x92, 0x82, 0xF8, 0x80, 0x90};

    private void Awake()
    {
        deviceType = BMSEngine.Device.DeviceType.Touch_Thermostat;

        if(!_hardwareId.Equals("") && !BMSEngine.DeviceManager.deviceList.ContainsKey(_hardwareId)) {
            BMSEngine.DeviceManager.deviceList.Add(_hardwareId, this);
            _netAC = new SlivingDeviceSim.Devices.AirCond(_hardwareId, "GW5R", "1.0.0", null);

        }else{
            Debug.Log("Doublicate or Invalid HWID: " + _hardwareId);
        }

    }

    private void Start()
    {
        _AcState = new AirConditionerState();

        _digit1Materials = _digit1.materials;
        _digit2Materials = _digit2.materials;
        _digit3Materials = _digit3.materials;
        _unitsMaterials = _units.materials;
        _roomMaterials = _room.materials;
        _fanSpeedMaterials = _fanSpeed.materials;
        _swingSpeedMaterials = _swingSpeed.materials;
        _fanAutoMaterial = _fanAuto.material;
        _swingAutoMaterial = _swingAuto.material;
        _modeCharMaterials[0] = _modeAuto.materials[1];
        _modeCharMaterials[1] = _modeCool.materials[1];
        _modeCharMaterials[2] = _modeHeat.materials[1];
        _modeCharMaterials[3] = _modeDry.materials[1];
        _modeCharMaterials[4] = _modeFan.materials[1];

        _modeBackgroundMaterials[0] = _modeAuto.materials[0];
        _modeBackgroundMaterials[1] = _modeCool.materials[0];
        _modeBackgroundMaterials[2] = _modeHeat.materials[0];
        _modeBackgroundMaterials[3] = _modeDry.materials[0];
        _modeBackgroundMaterials[4] = _modeFan.materials[0];

        _digit1.sharedMaterials = _digit1Materials;
        _digit2.sharedMaterials = _digit2Materials;
        _digit3.sharedMaterials = _digit3Materials;
        _units.sharedMaterials = _unitsMaterials;
        _room.sharedMaterials = _roomMaterials;
        _fanSpeed.sharedMaterials = _fanSpeedMaterials;
        _swingSpeed.sharedMaterials = _swingSpeedMaterials;
        _modeAuto.sharedMaterials[1] = _modeCharMaterials[0];
        _modeAuto.sharedMaterials[0] = _modeBackgroundMaterials[0];
        _modeCool.sharedMaterials[1] = _modeCharMaterials[1];
        _modeCool.sharedMaterials[0] = _modeBackgroundMaterials[1];
        _modeHeat.sharedMaterials[1] = _modeCharMaterials[2];
        _modeHeat.sharedMaterials[0] = _modeBackgroundMaterials[2];
        _modeDry.sharedMaterials[1] = _modeCharMaterials[3];
        _modeDry.sharedMaterials[0] = _modeBackgroundMaterials[3];
        _modeFan.sharedMaterials[1] = _modeCharMaterials[4];
        _modeFan.sharedMaterials[0] = _modeBackgroundMaterials[4];
        _fanAuto.sharedMaterial = _fanAutoMaterial;
        _swingAuto.sharedMaterial = _swingAutoMaterial;

        setDeviceValues(_AcState);
    }

    private void Update() {
        if(Time.time - _timeOutStamp > 5f){
            _displayTemperature = !_displayTemperature;

            if(_displayTemperature){ 
                displayNumber(getFormattedNumber(_currentTemperature));
                _roomMaterials[0].SetColor("_BaseColor", _ledSegmentOff);
                _roomMaterials[1].SetColor("_BaseColor", _ledSegmentOn);
                _unitsMaterials[0].SetColor("_BaseColor", _ledSegmentOff);
                _unitsMaterials[1].SetColor("_BaseColor", _ledSegmentOn);
            }else{
                displayNumber(getFormattedNumber(_currentHumidity));
                _unitsMaterials[0].SetColor("_BaseColor", _ledSegmentOn);
                _unitsMaterials[1].SetColor("_BaseColor", _ledSegmentOff);
            }

            _timeOutStamp = Time.time;
        }
    }

    public void init(){
        
    }

    public void setDeviceValues(AirConditionerState state){

        if(getFormattedNumber(state.Temperature) != getFormattedNumber(_AcState.Temperature)){
            setTemperature(getFormattedNumber(state.Temperature));
        }

        if(state.PowerState == PowerState.Off){
            setPower(state.PowerState);
            airConditioner?.setInt("PowerState", 0);
        }else{
            airConditioner?.setInt("PowerState", 10);
            setMode(state.Mode);
        }

        setFan(state.Fan);
        setSwing(state.Swing);

        _AcState = state;
    }

    private void displayNumber(uint number){


        uint digit1 = number % 10;
        number /= 10;
        uint digit2 = number % 10;
        number /= 10;
        uint digit3 = number % 10;

        setDigit(1, digit3);
        setDigit(2, digit2);
        setDigit(3, digit1);
    }

    private void setMode(ModeAc mode){

        for(int i = 0; i < _modeBackgroundMaterials.Length; i++){
            if(i == (int)mode){
                _modeBackgroundMaterials[i].SetColor("_BaseColor", _ledSegmentOn);
                _modeCharMaterials[i].SetColor("_BaseColor", _ledSegmentOff);
            }else{
                _modeBackgroundMaterials[i].SetColor("_BaseColor", _ledSegmentOff);
                _modeCharMaterials[i].SetColor("_BaseColor", _ledSegmentOn);
            }
        }

    }

    private void setSwing(SwingAc swing){
        if(_AcState.Swing != swing){
            
            if(swing == SwingAc.AutoSwing){
                _swingAutoMaterial.SetColor("_BaseColor", _ledSegmentOn);

                for(int i = 0; i < _swingSpeedMaterials.Length; i++){
                    _swingSpeedMaterials[i].SetColor("_BaseColor", _ledSegmentOn);
                }
            }else{
                _swingAutoMaterial.SetColor("_BaseColor", _ledSegmentOff);
                for(int i = 0; i < _swingSpeedMaterials.Length; i++){
                    if(i + 1 == (int)swing){
                        _swingSpeedMaterials[i].SetColor("_BaseColor", _ledSegmentOn);
                    }else{
                        _swingSpeedMaterials[i].SetColor("_BaseColor", _ledSegmentOff);
                    }
                }
            }
        }

    }

    private void setFan(FanAc fan){
        if(_AcState.Fan != fan){
            if(fan == FanAc.AutoFan){
                _fanAutoMaterial.SetColor("_BaseColor", _ledSegmentOn);

                for(int i = 0; i < _fanSpeedMaterials.Length; i++){
                    _fanSpeedMaterials[i].SetColor("_BaseColor", _ledSegmentOn);
                }
            }else{
                _fanAutoMaterial.SetColor("_BaseColor", _ledSegmentOff);
                for(int i = 0; i < _fanSpeedMaterials.Length; i++){
                    if(i + 1 <= (int)fan){
                        _fanSpeedMaterials[i].SetColor("_BaseColor", _ledSegmentOn);
                    }else{
                        _fanSpeedMaterials[i].SetColor("_BaseColor", _ledSegmentOff);
                    }
                }
            }
        }

    }

    private void setPower(PowerState power){
        for(int i = 0; i < _modeBackgroundMaterials.Length; i++){
            _modeBackgroundMaterials[i].SetColor("_BaseColor", _ledSegmentOff);
            _modeCharMaterials[i].SetColor("_BaseColor", _ledSegmentOff);
        }
    }

    private void setTemperature(uint temperature){
        _timeOutStamp = Time.time;
        _displayTemperature = false;
        displayNumber(temperature);
        _roomMaterials[0].SetColor("_BaseColor", _ledSegmentOn);
        _roomMaterials[1].SetColor("_BaseColor", _ledSegmentOff);
        _unitsMaterials[0].SetColor("_BaseColor", _ledSegmentOff);
        _unitsMaterials[1].SetColor("_BaseColor", _ledSegmentOn);
    }

    private uint getFormattedNumber(float number){
        return (uint)Mathf.FloorToInt(number * 10);
    }

    private void setDigit(uint id, uint number){

        byte bitLed = ledSegment[number];

        for(int i = 0; i < 7; i++){

            Color segColor = (bitLed & (1 << i)) == 0? _ledSegmentOn: _ledSegmentOff;

            switch(id){
                case 1:
                    _digit1Materials[i].SetColor("_BaseColor", segColor);
                    break;
                case 2:
                    _digit2Materials[i].SetColor("_BaseColor", segColor);
                    break;
                case 3:
                    _digit3Materials[i].SetColor("_BaseColor", segColor);
                    break;
            }
        }
    }


}

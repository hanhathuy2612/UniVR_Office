using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchSceneDevice : BMSEngine.Device
{
    // Start is called before the first frame update


    [SerializeField] private string _hardwareId;
    [SerializeField] private bool _isVirtual = true;
    [SerializeField] private BMSEngine.Device[] _loads = null;

    private SlivingDeviceSim.Devices.Touch _netTouch = null;


    public bool isVirtual {get => _isVirtual; set => _isVirtual = value;}
    public BMSEngine.Device[] loads {get => _loads;}
    public SlivingDeviceSim.Devices.Touch netTouch {get => _netTouch;}

    void Awake()
    {
        deviceType = BMSEngine.Device.DeviceType.Touch_Scene;

        if(!_hardwareId.Equals("") && !BMSEngine.DeviceManager.deviceList.ContainsKey(_hardwareId)) {
            BMSEngine.DeviceManager.deviceList.Add(_hardwareId, this);
            _netTouch = new SlivingDeviceSim.Devices.Touch(_hardwareId, 4, _loads.Length, "1.0.0", _isVirtual, "WT3");
        }else{
            Debug.Log("Doublicate or Invalid HWID: " + _hardwareId);
        }

        for(int i = 0; i < _loads.Length; i++){
            if(_loads[i] == null){
                _loads[i] = new GameObject().AddComponent<BMSEngine.Device>();
                Debug.Log("Missing at: " + _hardwareId);
            }
        }
    }

    public void setDeviceState(uint index, PowerState state){

        // if(_loads[index - 1]?.getDeviceType() == DeviceType.Airconditioner){
        //     if(state == PowerState.On){
        //         _loads[index - 1].setInt("PowerState", 10);
        //         _loads[index - 1].powerState = PowerState.On;
        //     }else if(state == PowerState.Off){
        //         _loads[index - 1].setInt("PowerState", 0);
        //         _loads[index - 1].powerState = PowerState.Off;
        //     }
        // }else if(_loads[index - 1]?.getDeviceType() == DeviceType.Light){
        //     if(state == PowerState.On){
        //         _loads[index - 1].setInt("PowerState", 101);
        //         _loads[index - 1].powerState = PowerState.On;
        //     }else if(state == PowerState.Off){
        //         _loads[index - 1].setInt("PowerState", 102);
        //         _loads[index - 1].powerState = PowerState.Off;
        //     }
        // }else{
        //     if(state == PowerState.On){
        //         _loads[index - 1].powerState = PowerState.On;
        //     }else if(state == PowerState.Off){
        //         _loads[index - 1].powerState = PowerState.Off;
        //     }
        // }

        // transform.GetChild((int)index - 1).GetComponent<TouchSwitchButton>().setState(state == PowerState.On);
    }
    

}

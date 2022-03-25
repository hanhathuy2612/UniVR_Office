using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class LedDriver : BMSEngine.Device
{

    [SerializeField] private string _hardwareId;
    [SerializeField] private bool _isVirtual = true;
    [SerializeField] public DeviceGroup[] lights;

    public int[] _brightnessLights = new int[10];
    public PowerState[] _lightPowerStates = new PowerState[10];
    private SlivingDeviceSim.Devices.LedDriver _netLedDriver;

    public bool isVirtual { get => _isVirtual; set => _isVirtual = value; }

    public SlivingDeviceSim.Devices.LedDriver netLedDriver { get => _netLedDriver; set => _netLedDriver = value; }

    private void Awake()
    {

        deviceType = BMSEngine.Device.DeviceType.LedDriver;

        if (!_hardwareId.Equals("") && !BMSEngine.DeviceManager.deviceList.ContainsKey(_hardwareId))
        {
            BMSEngine.DeviceManager.deviceList.Add(_hardwareId, this);
            _netLedDriver = new SlivingDeviceSim.Devices.LedDriver(_hardwareId, 4, "3.0.0", _isVirtual);
        }
        else
        {
            Debug.Log("Doublicate or Invalid HWID: " + _hardwareId);
        }
    }

    public void setLedDriver(uint index, int brightness, PowerState ps)
    {

        index -= 1;

        if (brightness > 0 && brightness <= 100)
        {
            _brightnessLights[index] = brightness;
            _lightPowerStates[index] = PowerState.On;
            lights[index]?.GetComponent<DeviceGroup>().setInt("PowerState", brightness);
        }
        else if (brightness == -1)
        {
            if (ps != PowerState.On)
            {
                _lightPowerStates[index] = ps;
                lights[index]?.GetComponent<DeviceGroup>().setInt("PowerState", 0);
            }
            else
            {
                _lightPowerStates[index] = ps;
                lights[index]?.GetComponent<DeviceGroup>().setInt("PowerState", _brightnessLights[index]);
            }
        }

        if (_isVirtual)
        {
            _netLedDriver.LedDriverOnChangedClientResponseFn(index + 1, _brightnessLights[index], _lightPowerStates[index]);
        }
        else
        {
            Debug.Log("Mobile");
            BMSEngine.DeviceManager.mobile.LedDriverRequest(_netLedDriver, _netLedDriver.RoomId, 0, _brightnessLights[index]);
        }
    }

    public void init()
    {

        for (uint i = 0; i < lights.Length; i++)
        {

            _brightnessLights[i] = 100;
            _lightPowerStates[i] = PowerState.Off;


            lights[i]?.setInt("PowerState", _brightnessLights[i]);

            if (_isVirtual)
            {
                _netLedDriver.LedDriverOnChangedClientResponseFn(i + 1, _brightnessLights[i], _lightPowerStates[i]);
            }
            else
            {
                BMSEngine.DeviceManager.mobile.LedDriverRequest(_netLedDriver, _netLedDriver.RoomId, 0, _brightnessLights[i]);
            }
        }
    }
}
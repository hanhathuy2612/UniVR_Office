using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TouchCurtainDevice : BMSEngine.Device
{
    [SerializeField] private string _hardwareId;
    [SerializeField] private bool _isVirtual = true;
    [SerializeField] private Curtain _curtain;
    private SlivingDeviceSim.Devices.Curtain _netCurtain;

    public bool isVirtual { get => _isVirtual; set => _isVirtual = value; }
    public SlivingDeviceSim.Devices.Curtain netCurtain { get => _netCurtain; set => _netCurtain = value; }

    void Awake()
    {
        deviceType = BMSEngine.Device.DeviceType.Touch_Curtain;

        if (!_hardwareId.Equals("") && !BMSEngine.DeviceManager.deviceList.ContainsKey(_hardwareId))
        {
            BMSEngine.DeviceManager.deviceList.Add(_hardwareId, this);
            _netCurtain = new SlivingDeviceSim.Devices.Curtain(_hardwareId, 3, 4, "3.0.0", _isVirtual);
        }
        else
        {
            Debug.Log("Doublicate or Invalid HWID: " + _hardwareId);
        }
    }

    public void setCurtain(uint percentIn, uint percentOut)
    {
        _curtain?.setCurtain(percentIn, percentOut);
    }

    public void setNetCurtain(uint id)
    {
        if (_curtain)
        {
            bool onMoving = true;

            if (id == 1 || id == 2)
            {
                onMoving = _curtain.lightScale != _curtain.leftLightCurtain.transform.localScale.x;

                if (onMoving)
                {
                    _curtain.lightScale = _curtain.leftLightCurtain.transform.localScale.x;
                }
                else
                {
                    if (id == 1)
                    {
                        _curtain.lightScale = _curtain.maxScale;
                    }
                    else if (id == 2)
                    {
                        _curtain.lightScale = _curtain.minScale;
                    }
                }


            }
            else
            {
                onMoving = _curtain.darkScale != _curtain.leftDarkCurtain.transform.localScale.x;

                if (onMoving)
                {
                    _curtain.darkScale = _curtain.leftDarkCurtain.transform.localScale.x;
                }
                else
                {
                    if (id == 3)
                    {
                        _curtain.darkScale = _curtain.minScale;
                    }
                    else if (id == 4)
                    {
                        _curtain.darkScale = _curtain.maxScale;
                    }
                }
            }

            uint percentIn = (uint)Mathf.CeilToInt(100 * (_curtain.maxScale - _curtain.darkScale) / (_curtain.maxScale - _curtain.minScale));
            uint percentOut = (uint)Mathf.CeilToInt(100 * (_curtain.maxScale - _curtain.lightScale) / (_curtain.maxScale - _curtain.minScale));


            if (isVirtual)
            {
                _netCurtain.CurtainSwitcherOnChangedClientResponseFn(percentIn, percentOut);
            }
            else
            {
                BMSEngine.DeviceManager.mobile.CurtainRequest(_netCurtain, percentIn, percentOut);
            }
        }
    }

}

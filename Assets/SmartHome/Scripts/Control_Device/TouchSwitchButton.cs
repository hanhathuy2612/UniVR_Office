using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TouchSwitchButton : MonoBehaviour
{
    [SerializeField] private bool _defaultState = false;
    [SerializeField] private RenderPipeline _renderPipeline = RenderPipeline.URP;

    [SerializeField] public GameObject[] _loads;
    private Color _deviceColorOn = new Color32(255, 10, 0, 255);
    private Color _deviceColorOff = new Color32(255, 255, 255, 255);
    private float _intensity = 10;
    private BMSEngine.Device.DeviceType _parentDeviceType;
    private Color _transformStartColor;
    private Material _buttonMaterial;
    private bool _deviceOn = false;
    private bool _onTransform = false;
    private float _transformSpeed = 3f;
    private float _transformTime = 0f;

    public enum RenderPipeline
    {
        URP,
        HDRP
    }

    void Start()
    {
        _buttonMaterial = createPipelineMaterial();
        _buttonMaterial.CopyPropertiesFromMaterial(GetComponent<MeshRenderer>().material);
        GetComponent<MeshRenderer>().sharedMaterial = _buttonMaterial;
        setMaterialColor(_buttonMaterial, _deviceColorOff);
        _parentDeviceType = transform.parent.GetComponent<BMSEngine.Device>().deviceType;

        if (_parentDeviceType == BMSEngine.Device.DeviceType.Touch_Curtain || _parentDeviceType == BMSEngine.Device.DeviceType.Touch_Scene || _parentDeviceType == BMSEngine.Device.DeviceType.Touch_Thermostat)
        {
            _transformSpeed = 2 * _transformSpeed;
            setState(_defaultState);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_onTransform)
        {
            if (_transformTime < 1)
            {
                _transformTime += _transformSpeed * Time.deltaTime;
            }

            if (_transformTime >= 1)
            {
                _transformTime = 1.0f;
                _onTransform = false;

                if ((_parentDeviceType == BMSEngine.Device.DeviceType.Touch_Curtain || _parentDeviceType == BMSEngine.Device.DeviceType.Touch_Scene || _parentDeviceType == BMSEngine.Device.DeviceType.Touch_Thermostat) && _deviceOn == !_defaultState)
                {
                    setState(_defaultState);
                }
            }

            if (_deviceOn)
            {
                setMaterialColor(_buttonMaterial, Color.Lerp(_transformStartColor, _deviceColorOn * _intensity, _transformTime));
            }
            else
            {
                setMaterialColor(_buttonMaterial, Color.Lerp(_transformStartColor, _deviceColorOff, _transformTime));
            }
        }
    }

    public void setState(bool deviceOn)
    {
        _deviceOn = deviceOn;
        _transformStartColor = getMaterialColor(_buttonMaterial);
        _transformTime = 0f;
        _onTransform = true;
    }

    public void setLoads(PowerState ps)
    {
        float emissiveIntensity = 0;
        Color emissiveColor = new Color(255, 96, 96, 96);

        for (int i = 0; i < _loads.Length; i++)
        {
            if (_loads[i] != null)
            {
                Material light = _loads[i].GetComponent<MeshRenderer>().materials.Where(x => !x.name.Contains("Plastic")).FirstOrDefault();
                if (light != null)
                {
                    if (ps == PowerState.Off)
                    {
                        emissiveIntensity = 0;
                        emissiveColor = new Color(255, 96, 96, 96);
                    }
                    else if (ps == PowerState.On)
                    {
                        emissiveIntensity = 10;
                        emissiveColor = Color.white;
                    }

                    light.SetColor("_EMISSION_COLOR", emissiveColor * emissiveIntensity);
                    Debug.Log("SetColor sucessful");
                }
            }
        }
    }

    public void onPress()
    {
        if (BMSEngine.DeviceManager.Ready)
        {
            int index = transform.GetSiblingIndex();

            if (_parentDeviceType == BMSEngine.Device.DeviceType.Touch_Curtain)
            {
                setState(!_defaultState);
                TouchCurtainDevice touchCurtain = transform.parent.GetComponent<TouchCurtainDevice>();
                touchCurtain?.setNetCurtain((uint)index + 1);
            }
            else if (_parentDeviceType == BMSEngine.Device.DeviceType.Touch_Normal)
            {
                TouchSwitchDevice touchSwitch = transform.parent.GetComponent<TouchSwitchDevice>();

                PowerState ps = PowerState.Off;
                if (touchSwitch.loads[index]?.powerState == PowerState.Off)
                {
                    ps = PowerState.On;
                }

                if (touchSwitch.isVirtual)
                {
                    touchSwitch.netTouch.SwitcherOnChangedClientResponseFn((uint)index + 1, ps);
                }
                else
                {
                    BMSEngine.DeviceManager.mobile.SwichRequest(touchSwitch?.netTouch, (uint)index + 1, ps);
                }
            }
            else if (_parentDeviceType == BMSEngine.Device.DeviceType.Touch_Scene)
            {
                Debug.Log("BMSEngine.Device.DeviceType.Touch_Scene");
                setState(!_defaultState);
                TouchSceneDevice touchScene = transform.parent.GetComponent<TouchSceneDevice>();
                PowerState ps = PowerState.SceneTrigger;

                if (touchScene.isVirtual)
                {
                    touchScene.netTouch.SwitcherOnChangedClientResponseFn((uint)index + 1, ps);
                }
                else
                {
                    BMSEngine.DeviceManager.mobile.SwichRequest(touchScene.netTouch, (uint)index + 1, ps);
                }

            }
            else if (_parentDeviceType == BMSEngine.Device.DeviceType.Touch_Thermostat)
            {
                Debug.Log("BMSEngine.Device.DeviceType.Touch_Thermostat");
                setState(!_defaultState);

            }
            else if (_parentDeviceType == BMSEngine.Device.DeviceType.LedDriver)
            {

                var ledDriver = transform.parent.GetComponent<LedDriver>();

                PowerState ps = PowerState.Off;
                var brightnessPercent = 0;
                if (ledDriver.lights[index]?.powerState == PowerState.Off)
                {
                    ps = PowerState.On;
                    brightnessPercent = 100;
                }

                if (ledDriver.isVirtual)
                {
                    ledDriver.netLedDriver.LedDriverOnChangedClientResponseFn((uint)index + 1, brightnessPercent, ps);
                }
                else
                {
                    Debug.Log("LedDriverRequest: ");
                    BMSEngine.DeviceManager.mobile.LedDriverRequest(ledDriver.netLedDriver, ledDriver.netLedDriver.RoomId, (uint)index + 1, brightnessPercent);
                    Debug.Log("LedDriverRequest: ");
                }
            }
        }
    }

    private Material createPipelineMaterial()
    {
        if (_renderPipeline == RenderPipeline.HDRP)
        {
            return new Material(Shader.Find("HDRP/Lit"));
        }
        else
        {
            return new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }
    }

    private void setMaterialColor(Material material, Color color)
    {
        if (_renderPipeline == RenderPipeline.HDRP)
        {
            material.SetColor("_BaseColor", color);
        }
        else
        {
            material.SetColor("_EmissionColor", color);
        }
    }

    private Color getMaterialColor(Material material)
    {
        if (_renderPipeline == RenderPipeline.HDRP)
        {
            return material.GetColor("_BaseColor");
        }
        else
        {
            return material.GetColor("_EmissionColor");
        }
    }

}

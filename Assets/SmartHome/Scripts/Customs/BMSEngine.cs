using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SlivingDeviceSim;

namespace BMSEngine
{

    static class UIManager
    {

    }

    static class DeviceManager
    {
        public static Dictionary<string, Device> deviceList = new Dictionary<string, Device>();

        // public static List<string> rooms = new();
        public static SlivingDeviceSim.DeviceManager manager;
        public static SlivingDeviceSim.Devices.Mobile mobile;
        public static SlivingDeviceSim.Devices.Gateway gateway;
        public static bool Ready { get => ready; }
        private static bool ready = false;
        public static void connectBMSServer()
        {

            Debug.Log("Connecting to BMS Server");

            foreach (var item in deviceList)
            {
                Debug.Log(item.Key);
            }

            gateway = new SlivingDeviceSim.Devices.Gateway(AppConfig.gatewayName, AppConfig.gatewayModel, AppConfig.gatewayVersion);
            mobile = new SlivingDeviceSim.Devices.Mobile(AppConfig.mobileNumber);
            manager = new SlivingDeviceSim.DeviceManager();
            manager.AddDevices(
                new SlivingDeviceSim.Devices.IDevice[] {
                    mobile, gateway
                }
            );
            Debug.Log("deviceList: " + deviceList.Count);
            foreach (KeyValuePair<string, Device> entry in deviceList)
            {
                if (entry.Value.deviceType == Device.DeviceType.Touch_Thermostat)
                {
                    manager.AddDevice(entry.Value.gameObject.GetComponent<TouchThermostat>().netAC);
                }
                else if (entry.Value.deviceType == Device.DeviceType.Touch_Curtain)
                {
                    manager.AddDevice(entry.Value.gameObject.GetComponent<TouchCurtainDevice>().netCurtain);
                }
                else if (entry.Value.deviceType == Device.DeviceType.Touch_Normal)
                {
                    manager.AddDevice(entry.Value.gameObject.GetComponent<TouchSwitchDevice>().netTouch);
                }
                else if (entry.Value.deviceType == Device.DeviceType.Touch_Scene)
                {
                    manager.AddDevice(entry.Value.gameObject.GetComponent<TouchSceneDevice>().netTouch);
                }
                else if (entry.Value.deviceType == Device.DeviceType.LedDriver)
                {
                    manager.AddDevice(entry.Value.gameObject.GetComponent<LedDriver>().netLedDriver);
                }

            }

            mobile.onMessage += Mobile_onMessage;
            manager.OnData += Manager_OnData;
            manager.OnInitialized += initDevices;
            manager.run();


            // if(gateway == null){
            //     gateway = new SlivingDeviceSim.Devices.Gateway("HOME-00000F", "HK1-1", "1.0.0");
            // }

            // if(mobile == null){
            //     mobile = new SlivingDeviceSim.Devices.Mobile("0948503305");
            // }

            // if(manager == null){

            //     manager = new SlivingDeviceSim.DeviceManager();
            //     manager.AddDevices(
            //         new SlivingDeviceSim.Devices.IDevice[] {
            //             mobile, gateway
            //         }
            //     );

            //     foreach(KeyValuePair<string, Device> entry in deviceList)
            //     {
            //         if(entry.Value.deviceType == Device.DeviceType.Touch_Thermostat){
            //             manager.AddDevice(entry.Value.gameObject.GetComponent<TouchThermostat>().netAC);
            //         }else if(entry.Value.deviceType == Device.DeviceType.Touch_Curtain){
            //             manager.AddDevice(entry.Value.gameObject.GetComponent<TouchCurtainDevice>().netCurtain);
            //         }else if(entry.Value.deviceType == Device.DeviceType.Touch_Normal){
            //             manager.AddDevice(entry.Value.gameObject.GetComponent<TouchSwitchDevice>().netTouch);
            //         }else if(entry.Value.deviceType == Device.DeviceType.Touch_Scene){
            //             manager.AddDevice(entry.Value.gameObject.GetComponent<TouchSceneDevice>().netTouch);
            //         }else if(entry.Value.deviceType == Device.DeviceType.LedDriver){
            //             manager.AddDevice(entry.Value.gameObject.GetComponent<LedDriver>().netLedDriver);
            //         }

            //     }

            //     mobile.onMessage += Mobile_onMessage;
            //     manager.OnData += Manager_OnData;
            //     manager.OnInitialized += initDevices;

            // }else {
            //     disconnectBMSServer();
            // }

            // manager.run();
        }

        public static void disconnectBMSServer()
        {
            manager?.stop();
            Debug.Log("Disconnected");
        }

        private static void Mobile_onMessage(object sender, EventArgs e)
        {
            SlivingDeviceSim.Devices.Mobile mobile = (SlivingDeviceSim.Devices.Mobile)sender;
            SlivingDeviceSim.Devices.DeviceEventArgs evt = (SlivingDeviceSim.Devices.DeviceEventArgs)e;
            Debug.Log("Mobile_OnData");
            Debug.Log(evt.MainMessage.ToString());

            SwitcherOnChangedServerResponse switcherOnChangedServerResponse = evt.MainMessage?.ServerMessage?.OnChangedServerMessage?.SwitcherOnChangedServerResponse;
            CurtainSwitcherOnChangedServerResponse curtainSwitcherClientResponse = evt.MainMessage?.ServerMessage?.OnChangedServerMessage?.CurtainSwitcherOnChangedServerResponse;
            
            string hwid = "";
            if (switcherOnChangedServerResponse != null)
            {
                hwid = mobile.GetHardwareIdFromDeviceId(switcherOnChangedServerResponse?.DeviceId);

            }
            else if (curtainSwitcherClientResponse != null)
            {
                hwid = mobile.GetHardwareIdFromDeviceId(curtainSwitcherClientResponse?.DeviceId);
            }

            Debug.Log("GetHardwareIdFromDeviceId: " + hwid);

            if (deviceList.ContainsKey(hwid))
            {
                SyncContext.RunOnUnityThread(() =>
                {
                    try
                    {
                        if (switcherOnChangedServerResponse != null && deviceList[hwid].deviceType == Device.DeviceType.Touch_Normal)
                        {
                            deviceList[hwid].gameObject.GetComponent<TouchSwitchDevice>().setDeviceState(switcherOnChangedServerResponse.Switcher, switcherOnChangedServerResponse.PowerState);
                        }
                        else if (curtainSwitcherClientResponse != null && deviceList[hwid].deviceType == Device.DeviceType.Touch_Curtain)
                        {
                            deviceList[hwid].gameObject.GetComponent<TouchCurtainDevice>().setCurtain(curtainSwitcherClientResponse.PercentIn, curtainSwitcherClientResponse.PercentOut);
                        }
                    }
                    catch (Exception er)
                    {
                        Debug.LogException(er);
                    }
                });

            }
        }

        private static void Manager_OnData(object sender, EventArgs e)
        {
            SlivingDeviceSim.Devices.DeviceEventArgs evt = (SlivingDeviceSim.Devices.DeviceEventArgs)e;
            Debug.Log("Manager_OnData");
            Debug.Log(evt.MainMessage.ToString());

            SwitcherClientRequest switcherClientRequest = evt.MainMessage?.ClientMessage?.SwitcherClientMessage?.SwitcherClientRequest;
            AirConditionerClientRequest airConditionerClientRequest = evt.MainMessage?.ClientMessage?.AirConditionerClientMessage?.AirConditionerClientRequest;
            CurtainSwitcherClientRequest curtainSwitcherClientRequest = evt.MainMessage?.ClientMessage?.CurtainSwitcherClientMessage?.CurtainSwitcherClientRequest;
            LedDriverClientRequest ledDriverClientRequest = evt.MainMessage?.ClientMessage?.LedDriverClientMessage?.LedDriverClientRequest;

            string hwid = "";

            if (switcherClientRequest != null)
            {
                hwid = switcherClientRequest.HardwareId;
            }
            else if (airConditionerClientRequest != null)
            {
                hwid = airConditionerClientRequest.HardwareId;
            }
            else if (curtainSwitcherClientRequest != null)
            {
                hwid = curtainSwitcherClientRequest.HardwareId;
            }
            else if (ledDriverClientRequest != null)
            {
                hwid = ledDriverClientRequest.HardwareId;
            }

            Debug.Log("Manager_OnDataManager_OnDataManager_OnData: " + hwid);

            if (deviceList.ContainsKey(hwid))
            {
                SyncContext.RunOnUnityThread(() =>
                {
                    try
                    {
                        if (switcherClientRequest != null && deviceList[hwid].deviceType == Device.DeviceType.Touch_Normal)
                        {
                            Debug.Log("Touch_Normal");
                            deviceList[hwid].gameObject.GetComponent<TouchSwitchDevice>().setDeviceState(switcherClientRequest.Switcher, switcherClientRequest.PowerState);
                        }
                        else if (airConditionerClientRequest != null && deviceList[hwid].deviceType == Device.DeviceType.Touch_Thermostat)
                        {
                            Debug.Log("Airconditioner");
                            deviceList[hwid].gameObject.GetComponent<TouchThermostat>().setDeviceValues(airConditionerClientRequest.State);
                        }
                        else if (curtainSwitcherClientRequest != null && deviceList[hwid].deviceType == Device.DeviceType.Touch_Curtain)
                        {
                            Debug.Log("Touch_Curtain");
                            deviceList[hwid].gameObject.GetComponent<TouchCurtainDevice>().setCurtain(curtainSwitcherClientRequest.PercentIn, curtainSwitcherClientRequest.PercentOut);
                        }
                        else if (ledDriverClientRequest != null && deviceList[hwid].deviceType == Device.DeviceType.LedDriver)
                        {
                            Debug.Log("Led_Driver");
                            deviceList[hwid].gameObject.GetComponent<LedDriver>().setLedDriver(ledDriverClientRequest.LedIndex, ledDriverClientRequest.BrightnessPercent, ledDriverClientRequest.PowerState);
                        }

                    }
                    catch (Exception er)
                    {
                        Debug.LogException(er);
                    }
                });

            }
        }

        public static void initDevices(object sender, EventArgs e)
        {

            foreach (KeyValuePair<string, Device> entry in deviceList)
            {

                if (entry.Value.deviceType == Device.DeviceType.Touch_Normal)
                {

                    TouchSwitchDevice touchDevice = entry.Value.gameObject.GetComponent<TouchSwitchDevice>();

                    Debug.Log("Init Device");
                    Debug.Log(touchDevice);

                    for (uint i = 0; i < touchDevice.loads.Length; i++)
                    {
                        if (touchDevice != null && touchDevice.loads[i] != null)
                        {
                            if (touchDevice.isVirtual)
                            {
                                Debug.Log("Virtual: " + i);
                                Debug.Log(touchDevice.netTouch);
                                Debug.Log(touchDevice.loads[i].powerState);
                                touchDevice.netTouch.SwitcherOnChangedClientResponseFn(i + 1, touchDevice.loads[i].powerState);
                            }
                            else
                            {
                                try
                                {
                                    Debug.Log("Real: " + i);
                                    Debug.Log(touchDevice.netTouch.ToString());
                                    Debug.Log(touchDevice.loads[i].powerState);
                                    mobile.SwichRequest(touchDevice.netTouch, i + 1, touchDevice.loads[i].powerState);

                                }
                                catch (Exception er)
                                {
                                    Debug.Log("Exception: " + i);
                                    Debug.LogException(er);
                                }
                            }
                        }
                    }

                }
                else if (entry.Value.deviceType == Device.DeviceType.Touch_Curtain)
                {

                }
                else if (entry.Value.deviceType == Device.DeviceType.Touch_Thermostat)
                {
                    // entry.Value.gameObject.GetComponent<TouchThermostat>().init();
                }
                else if (entry.Value.deviceType == Device.DeviceType.LedDriver)
                {
                    entry.Value.gameObject.GetComponent<LedDriver>().init();
                }
            }

            Debug.Log("Started");
            ready = true;
        }

    }


    public class Device : MonoBehaviour
    {

        public enum DeviceType
        {
            Not_Set = 0,
            Touch_Normal = 1,
            Touch_Curtain = 2,
            Touch_Scene = 3,
            Touch_Thermostat = 4,
            Light = 5,
            Air_Conditioner = 6,
            Curtain = 7,
            LedDriver = 8,
        }

        public PowerState powerState = PowerState.Off;
        public DeviceType deviceType = DeviceType.Not_Set;
        public virtual void message(string msg) { }
        public virtual void setBool(string key, bool value) { }
        public virtual void setFloat(string key, float value) { }
        public virtual void setInt(string key, int value) { }
        public virtual void setVector3(string key, Vector3 value) { }
        public virtual void setString(string key, string value) { }

    }
}

// public static class BMSEngine
// {

//     public static Color32 getTextureToneColor(RenderTexture renderTexture){
//         Color32 result = Color.black;        
//         Color32 [] colorMap;
//         int samples = 0, red = 0, green = 0, blue = 0, size = renderTexture.width* renderTexture.height;

//         Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);
//         RenderTexture.active = renderTexture;
//         texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
//         // texture.Resize(32, 32);
//         // size = 32 * 32;
//         colorMap = texture.GetPixels32();

//         for(int i=0; i< size; i += 32){
//             red += colorMap[i].r;
//             green += colorMap[i].g;
//             blue += colorMap[i].b;
//             samples++;
//         }

//         result.r = (byte) (red/samples);
//         result.g = (byte) (green/samples);
//         result.b = (byte) (blue/samples);
//         result.a = 255;

//         return result;
//     }

// }

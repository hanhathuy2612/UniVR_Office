using System;
namespace SlivingDeviceSim.Devices
{
    public class Touch : DeviceBase
    {
        public Touch(string HardwareId, int TouchVersion, int TouchNum, string FirmwareVersion, bool NeedLogin = true, string modelPrefix = "ZT4") :
            base(HardwareId, GetModelString(TouchVersion, TouchNum, modelPrefix), FirmwareVersion, DeviceType.TOUCH, NeedLogin)
        {
            base.SetNumChannel(TouchNum);
        }

        private static string GetModelString(int TouchVersion, int TouchNum, string modelPrefix)
        {
            return String.Format("{0}-{1}N", modelPrefix, TouchNum);
        }

        public void SwitcherOnChangedClientResponseFn(uint switcher, PowerState powerState)
        {
            MainMessage mainMessage = new MainMessage
            {
                ClientMessage = new ClientMessage
                {
                    OnChangedClientMessage = new OnChangedClientMessage
                    {
                        SwitcherOnChangedClientResponse = new SwitcherOnChangedClientResponse
                        {
                            Switcher = switcher,
                            DeviceId = DeviceId,
                            PowerState = powerState,
                            StatusCode = new StatusCode
                            {
                                Code = 0,
                                Message = "Success"
                            }
                        }
                    }
                }
            };
            Gateway?.WebsocketRequestWithoutResponse(mainMessage);

        }

        private void UpdateClientResponseFn()
        {
            MainMessage mainMessage = new MainMessage
            {
                ClientMessage = new ClientMessage
                {
                    DeviceClientMessage = new DeviceClientMessage
                    {
                        UpdateDeviceClientResponse = new UpdateDeviceClientResponse
                        {
                            DeviceId = DeviceId,
                            StatusCode = new StatusCode
                            {
                                Code = 0,
                                Message = "Success",
                            }
                        }
                    }
                }
            };
            Gateway?.WebsocketRequestWithoutResponse(mainMessage);
        }

        public override void ProcessMessage(MainMessage mainMessage)
        {
            SwitcherClientRequest switcherClientRequest = mainMessage?.ClientMessage?.SwitcherClientMessage?.SwitcherClientRequest;
            UpdateDeviceClientRequest updateDeviceClientRequest = mainMessage?.ClientMessage?.DeviceClientMessage?.UpdateDeviceClientRequest;

            if (switcherClientRequest?.HardwareId == HardwareId)
            {
                SwitcherOnChangedClientResponseFn(switcherClientRequest.Switcher, switcherClientRequest.PowerState);
                DeviceDispachEvent(new DeviceEventArgs(mainMessage));
            }

            if (updateDeviceClientRequest?.HardwareId == HardwareId)
            {
                Log.Debug("reactivity: " + updateDeviceClientRequest.Reactivity.ToString() + " brightness: " + updateDeviceClientRequest.Brightness.ToString());
                UpdateClientResponseFn();
                DeviceDispachEvent(new DeviceEventArgs(mainMessage));
            }

        }
    }
}

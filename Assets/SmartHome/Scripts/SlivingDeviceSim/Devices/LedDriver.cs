using System;

namespace SlivingDeviceSim.Devices
{
    public class LedDriver : DeviceBase
    {
        public LedDriver(string hardwareId, int version, string firmwareVersion, bool needLogin = true) : base(hardwareId, GetModelString(version), firmwareVersion, DeviceType.LEDDRIVER, needLogin)
        {
        }

        private static string GetModelString(int TouchVersion)
        {
            return String.Format("ZL{0}-8N", TouchVersion);
        }

        public void LedDriverOnChangedClientResponseFn(uint index, int brightnessPercent, PowerState ps)
        {
            MainMessage mainMessage = new MainMessage
            {
                ClientMessage = new ClientMessage
                {
                    OnChangedClientMessage = new OnChangedClientMessage
                    {
                        LedDriverOnChangedClientResponse = new LedDriverOnChangedClientResponse
                        {
                            DeviceId = DeviceId,
                            LedIndex = index,
                            BrightnessPercent = brightnessPercent,
                            PowerState = ps,
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
            LedDriverClientRequest leddriverClientRequest = mainMessage?.ClientMessage?.LedDriverClientMessage?.LedDriverClientRequest;

            if (leddriverClientRequest != null && leddriverClientRequest.HardwareId == HardwareId)
            {
                LedDriverOnChangedClientResponseFn(leddriverClientRequest.LedIndex, leddriverClientRequest.BrightnessPercent, leddriverClientRequest.PowerState);
                DeviceDispachEvent(new DeviceEventArgs(mainMessage));
            }
        }
    }
}
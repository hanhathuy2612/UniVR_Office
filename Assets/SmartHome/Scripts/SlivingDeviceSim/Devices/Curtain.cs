using System;
namespace SlivingDeviceSim.Devices
{
    public class Curtain : DeviceBase
    {
        public Curtain(string hardwareId, int version, int numTouch, string firmwareVersion, bool needLogin = true) : base(hardwareId, GetModelString(version, numTouch), firmwareVersion, DeviceType.CURTAIN, needLogin)
        {
        }

        private static string GetModelString(int TouchVersion, int TouchNum)
        {
            return String.Format("WC{0}-{1}", TouchVersion, TouchNum);
        }

        public void CurtainSwitcherOnChangedClientResponseFn(uint percentIn, uint percentOut)
        {
            MainMessage mainMessage = new MainMessage
            {
                ClientMessage = new ClientMessage
                {
                    OnChangedClientMessage = new OnChangedClientMessage
                    {
                        CurtainSwitcherOnChangedClientResponse = new CurtainSwitcherOnChangedClientResponse
                        {
                            DeviceId = DeviceId,
                            PercentIn = percentIn,
                            PercentOut = percentOut,
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
            CurtainSwitcherClientRequest curtainSwitcherClientRequest = mainMessage?.ClientMessage?.CurtainSwitcherClientMessage?.CurtainSwitcherClientRequest;

            if (curtainSwitcherClientRequest != null && curtainSwitcherClientRequest.HardwareId == HardwareId)
            {
                CurtainSwitcherOnChangedClientResponseFn(curtainSwitcherClientRequest.PercentIn, curtainSwitcherClientRequest.PercentOut);
                DeviceDispachEvent(new DeviceEventArgs(mainMessage));
            }
        }
    }
}

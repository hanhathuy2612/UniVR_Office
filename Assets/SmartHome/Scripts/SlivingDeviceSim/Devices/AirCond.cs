using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
namespace SlivingDeviceSim.Devices
{
    public class AirCond : DeviceBase
    {
        private List<AirConditionerState> _indoors = new List<AirConditionerState>();
        public AirCond(string hardwareId, string model, string firmwareVersion, AirConditionerState[] indoors) : base(hardwareId, model, firmwareVersion, DeviceType.AIRCOND)
        {
            if (indoors != null && indoors.Length > 0)
            {
                _indoors.AddRange(indoors);
            } else
            {
                _indoors.Add(new AirConditionerState
                {
                    ConnectionState = ConnectionState.Online,
                    IndoorId = 0,
                    Temperature = 25.0F,
                    TemperatureRoom = 25.0F,
                    HumidityRoom = 98.0F,
                    Swing = SwingAc.AutoSwing,
                    Fan = FanAc.AutoFan,
                    Mode = ModeAc.AutoMode,
                    PowerState = PowerState.On,
                });
            }
            
        }

        private async Task AirConditionerAddIndoorsRequestFn()
        {

            AirConditionerAddIndoorsRequest airConditionerAddIndoorsRequest = new AirConditionerAddIndoorsRequest
            {
                DeviceId = DeviceId,
            };
            _indoors.ForEach(indoor =>
            {
                indoor.DeviceId = DeviceId;
                airConditionerAddIndoorsRequest.IndoorId.Add(indoor.IndoorId);
            });
            

            MainMessage mainMessage = new MainMessage
            {
                ClientMessage = new ClientMessage
                {
                    AirConditionerClientMessage = new AirConditionerClientMessage
                    {
                        AirConditionerAddIndoorsRequest = airConditionerAddIndoorsRequest
                    }
                }
            };
            await Gateway?.WebsocketRequest(mainMessage);
        }

        public void Update(AirConditionerState airConditionerState)
        {
            uint indoor = airConditionerState.IndoorId;

            int idx = _indoors.FindIndex(e => e.IndoorId == indoor);

            _indoors[idx].PowerState = airConditionerState?.PowerState == null ? PowerState.Off : airConditionerState.PowerState;
            _indoors[idx].Fan = airConditionerState?.Fan == null ? FanAc.AutoFan : airConditionerState.Fan;
            _indoors[idx].Swing = airConditionerState?.Swing == null ? SwingAc.AutoSwing : airConditionerState.Swing;
            _indoors[idx].Temperature = airConditionerState?.Temperature == null ? 0.0F : airConditionerState.Temperature;
            if (_indoors[idx].Temperature < 15.0F)
            {
                _indoors[idx].Temperature = 15.0F;
            }
            _indoors[idx].TemperatureRoom = airConditionerState?.Temperature == null ? 0.0F : airConditionerState.Temperature; 
            _indoors[idx].HumidityRoom = airConditionerState?.HumidityRoom == null ? 0.0F : airConditionerState.HumidityRoom; 
            _indoors[idx].Mode = airConditionerState?.Mode == null ? ModeAc.AutoMode : airConditionerState.Mode;
            _indoors[idx].DeviceId = DeviceId;
            MainMessage mainMessage = new MainMessage
            {
                ClientMessage = new ClientMessage
                {
                    OnChangedClientMessage = new OnChangedClientMessage
                    {
                        
                        AirConditionerOnChangedClientResponse = new AirConditionerOnChangedClientResponse
                        {
                            DeviceId = DeviceId,
                            State = _indoors[idx]
                        }
                    }
                }
            };
            Gateway?.WebsocketRequestWithoutResponse(mainMessage);
        }

        private void GetAirConditionerClientResponseFn(uint indoorId, AirConditionerState airConditionerState)
        {
            GetAirConditionerClientResponse getAirConditionerClientResponse = new GetAirConditionerClientResponse
            {
               DeviceId = DeviceId,
               State = airConditionerState,
               StatusCode = new StatusCode
               {
                   Code = 0,
                   Message = "Success",
               }
            };
            
            MainMessage mainMessage = new MainMessage
            {
                ClientMessage = new ClientMessage
                {
                    AirConditionerClientMessage = new AirConditionerClientMessage
                    {
                        GetAirConditionerClientResponse = getAirConditionerClientResponse,
                    }
                }
            };
            Gateway?.WebsocketRequestWithoutResponse(mainMessage);
        }

        public override async Task OnLoginSuccess()
        {
            Log.Debug("Aircond login success" + this.ToString());
            await base.OnLoginSuccess();
            await AirConditionerAddIndoorsRequestFn();
            _indoors.ForEach(indoor => Update(indoor));
        }

        public override void ProcessMessage(MainMessage mainMessage)
        {
            GetAirConditionerClientRequest getAirConditionerClientRequest = mainMessage?.ClientMessage?.AirConditionerClientMessage?.GetAirConditionerClientRequest;
            AirConditionerClientRequest airConditionerClientRequest = mainMessage?.ClientMessage?.AirConditionerClientMessage?.AirConditionerClientRequest;

            if (getAirConditionerClientRequest?.HardwareId == HardwareId)
            {
                Log.Debug("getAirConditionerClientRequest ========== " + getAirConditionerClientRequest.ToString());
                uint indoorId = getAirConditionerClientRequest?.IndoorId != null ? getAirConditionerClientRequest.IndoorId : 0;
                
                AirConditionerState airConditionerState = _indoors.FindLast(e => e.IndoorId == indoorId);
                airConditionerState.DeviceId = DeviceId;
                GetAirConditionerClientResponseFn(indoorId, airConditionerState);
                DeviceDispachEvent(new DeviceEventArgs(mainMessage));
            }
            else if (airConditionerClientRequest?.HardwareId == HardwareId)
            {
                Log.Debug("airConditionerClientRequest ========== " + airConditionerClientRequest.ToString());
                Update(airConditionerClientRequest.State);
                AirConditionerState airConditionerState = _indoors.FindLast(e => e.IndoorId == airConditionerClientRequest.State.IndoorId);

                Log.Debug("airConditionerClientRequest after updated: " + airConditionerState.ToString());
                DeviceDispachEvent(new DeviceEventArgs(mainMessage));
            } else
            {
                Log.Debug("Not at all: " + mainMessage.ToString());
            }
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using UnityEngine;
using System.Linq;

namespace SlivingDeviceSim.Devices
{
    public class Mobile : DeviceBase
    {
        private string _phone;
        private LoginServerResponse _loginServerResponse = new LoginServerResponse();
        public GetRoomsServerResponse _getRoomsServerResponse = new GetRoomsServerResponse();
        public GetDevicesServerResponse _getDevicesServerResponse = null;

        public Mobile(string phone) : base("MOBILE_01", "PHONE", "1.0.0", DeviceType.MOBILE)
        {
            _phone = phone;
            InitWebsocket();
        }

        public async Task LoginAndLoad()
        {
            var otp = await RequestOtpCode(_phone);
            Debug.Log("OTP: " + otp);
            var token = await VerifyCode(_phone, otp);
            Debug.Log("TOKEN: " + token);
            await LoginByAccessToken(token);
            _getRoomsServerResponse = await LoadDefaultRoom(AppConfig.homeId);
            _getDevicesServerResponse = await GetDevices(AppConfig.homeId);
        }

        public string GetHardwareIdFromDeviceId(string deviceId)
        {
            var rooms = _getDevicesServerResponse?.Rooms;
            foreach (var room in rooms)
            {
                foreach (var device in room?.Devices)
                {
                    if (device.DeviceId == deviceId)
                    {
                        return device.HardwareId;
                    }
                }
            }
            return null;
        }

        private async Task LoginByAccessToken(string token)
        {
            MainMessage mainMessage = new MainMessage
            {
                ServerMessage = new ServerMessage
                {
                    AuthServerMessage = new AuthServerMessage
                    {
                        LoginServerRequest = new LoginServerRequest
                        {
                            LoggedOS = "ios",
                            VersionOS = "14.0.0",
                            Model = "IPHONE11",
                            BrandName = "Apple",
                            DeviceId = "PHONE1",
                            LoggedIP = "127.0.0.1",
                            AccessToken = token
                        }
                    }
                }
            };
            var res = await WebsocketRequest(mainMessage);
            var _loginServerRes = res?.ServerMessage?.AuthServerMessage?.LoginServerResponse;
            if (_loginServerRes.AccessToken?.Length > 0)
            {
                _loginServerResponse = _loginServerRes;
                AccessKey = _loginServerResponse?.AccessKey;
                Log.Debug("Authenticated");
            }
            else if (_loginServerRes?.StatusCode?.Code == 400)
            {
                await HandlerRefreshToken(_loginServerResponse.AccessToken, _loginServerResponse.RefreshToken);
            }
        }

        private async Task HandlerRefreshToken(string accessToken, string refreshToken)
        {
            MainMessage mainMessage = new MainMessage
            {
                ServerMessage = new ServerMessage
                {
                    AuthServerMessage = new AuthServerMessage
                    {
                        RefreshTokenServerRequest = new RefreshTokenServerRequest
                        {
                            AccessToken = accessToken,
                            RefreshToken = refreshToken
                        }
                    }
                }
            };
            var res = await WebsocketRequest(mainMessage);
            if (res?.ServerMessage?.AuthServerMessage?.RefreshTokenServerResponse?.RefreshToken != "")
            {
                _loginServerResponse.RefreshToken = res?.ServerMessage?.AuthServerMessage?.RefreshTokenServerResponse?.RefreshToken;
                _loginServerResponse.AccessToken = res?.ServerMessage?.AuthServerMessage?.RefreshTokenServerResponse?.AccessToken;
            }
        }

        private async Task<string> RequestOtpCode(string phone)
        {
            MainMessage mainMessage = new MainMessage
            {
                ServerMessage = new ServerMessage
                {
                    AuthServerMessage = new AuthServerMessage
                    {
                        SendCodeServerRequest = new SendCodeServerRequest
                        {
                            PhoneNumber = phone
                        }
                    }
                }
            };
            var res = await WebsocketRequest(mainMessage);
            return await Task.FromResult(res?.ServerMessage?.AuthServerMessage?.SendCodeServerResponse?.Code);
        }

        private async Task<string> VerifyCode(string phone, string code)
        {
            MainMessage mainMessage = new MainMessage
            {
                ServerMessage = new ServerMessage
                {
                    AuthServerMessage = new AuthServerMessage
                    {
                        VerifyCodeServerRequest = new VerifyCodeServerRequest
                        {
                            PhoneNumber = phone,
                            Code = code
                        }
                    }
                }
            };
            var res = await WebsocketRequest(mainMessage);

            return await Task.FromResult(res?.ServerMessage?.AuthServerMessage?.VerifyCodeServerResponse?.AccessToken);
        }

        private async Task<GetRoomsServerResponse> LoadDefaultRoom(string homeid)
        {
            MainMessage mainMessage = new MainMessage
            {
                ServerMessage = new ServerMessage
                {
                    RoomServerMessage = new RoomServerMessage
                    {
                        GetRoomsServerRequest = new GetRoomsServerRequest
                        {
                            HomeId = homeid
                        }
                    }
                }
            };
            var res = await WebsocketRequest(mainMessage);

            return await Task.FromResult(res?.ServerMessage?.RoomServerMessage?.GetRoomsServerResponse);
        }

        private async Task<GetDevicesServerResponse> GetDevices(string homeid)
        {
            MainMessage mainMessage = new MainMessage
            {
                ServerMessage = new ServerMessage
                {
                    DeviceServerMessage = new DeviceServerMessage
                    {
                        GetDevicesServerRequest = new GetDevicesServerRequest
                        {
                            HomeId = homeid
                        }
                    }
                }
            };
            var res = await WebsocketRequest(mainMessage);
            return await Task.FromResult(res?.ServerMessage?.DeviceServerMessage?.GetDevicesServerResponse);
        }
        public async Task<(string DeviceId, string RoomId)> AssignDevice(IDevice device)
        {
            string roomId = _getRoomsServerResponse?.Rooms?[0].Id;
            var rooms = _getDevicesServerResponse?.Rooms;

            var deviceID = "";

            foreach (var room in rooms)
            {
                // Debug.Log($"Rooms name: {room.Name}, ID: {room.Id}");
                foreach (var roomDevice in room?.Devices)
                {
                    // Debug.Log($"\tDevice name: {roomDevice.DeviceName}, ID: {roomDevice.DeviceId}, HardwareId: {roomDevice.HardwareId}");

                    if (roomDevice.HardwareId == device.HardwareId.Trim())
                    {
                        deviceID = roomDevice.DeviceId;
                        roomId = roomDevice.RoomId;
                        break;
                    }
                }
            }
           
            Device dev = new Device
            {
                RoomId = roomId,
                HardwareId = device.HardwareId,
                DeviceName = device.HardwareId + "|" + device.Model,
                BrandName = "SunshineTECH",
                Model = device.Model,
                Ip = "1.1.1.1",
                Mdns = "tuan.local",
                Signal = -1,
                FirmwareVersion = device.FirmwareVersion,
                HardwareVersion = device.HardwareVersion,
            };

            AddDevicesServerRequest addDevicesServerRequest = new AddDevicesServerRequest();
            addDevicesServerRequest.Devices.Add(dev);
            MainMessage mainMessage = new MainMessage
            {
                ServerMessage = new ServerMessage
                {
                    DeviceServerMessage = new DeviceServerMessage
                    {
                        AddDevicesServerRequest = addDevicesServerRequest
                    }
                }
            };
            var res = await WebsocketRequest(mainMessage);
            return await Task.FromResult((res?.ServerMessage?.DeviceServerMessage?.AddDevicesServerResponse?.Devices?.FirstOrDefault()?.DeviceId ?? deviceID, roomId));
        }

        public void SwichRequest(IDevice device, uint touchNo, PowerState powerState)
        {
            MainMessage mainMessage = new MainMessage
            {
                ServerMessage = new ServerMessage
                {
                    SwitcherServerMessage = new SwitcherServerMessage
                    {
                        SwitcherServerRequest = new SwitcherServerRequest
                        {
                            DeviceId = device?.DeviceId,
                            Switcher = touchNo,
                            PowerState = powerState,
                        }
                    }
                }
            };
            WebsocketRequestWithoutResponse(mainMessage);
        }

        public void CurtainRequest(IDevice device, uint percentIn, uint percentOut)
        {
            MainMessage mainMessage = new MainMessage
            {
                ServerMessage = new ServerMessage
                {
                    CurtainSwitcherServerMessage = new CurtainSwitcherServerMessage
                    {
                        CurtainSwitcherServerRequest = new CurtainSwitcherServerRequest
                        {
                            DeviceId = device.DeviceId,
                            PercentIn = percentIn,
                            PercentOut = percentOut
                        }
                    }
                }
            };
            WebsocketRequestWithoutResponse(mainMessage);
        }

        public void LedDriverRequest(IDevice device, string roomId ,uint groupControlActive, int brightness)
        {
            MainMessage mainMessage = new MainMessage
            {
                ServerMessage = new ServerMessage
                {
                    LedDriverServerMessage = new LedDriverServerMessage
                    {
                        LedDriverServerRequest = new LedDriverServerRequest
                        {
                            DeviceId = device.DeviceId,
                            GroupControl = groupControlActive,
                            BrightnessPercent = brightness,
                            RoomId = roomId,
                        },
                        
                    }
                }
            };
            WebsocketRequestWithoutResponse(mainMessage);
        }
    }
}

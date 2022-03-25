using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using TinyJson;

namespace SlivingDeviceSim.Devices
{
    public class DeviceEventArgs : EventArgs
    {
        public MainMessage MainMessage { get; set; }
        internal DeviceEventArgs(MainMessage mainMessage)
        {
            MainMessage = mainMessage;
        }

    }

    public class DeviceBase : IDevice
    {
        private const string DEVICE_REGISTER_ENDPOINT = "https://dev.iot.sunshinetech.vn/api/v1/devices/register";
        private const string WEBSOCKET_ENDPOINT = "wss://dev.iot.sunshinetech.vn/websocket";
        private const string MANUFACTURER_TOKEN = "11E8C7BC66B905A4969200155D6C8503";
        private string _hwid;
        private string _fwVer;
        private string _model;
        private string _token;
        private string _deviceId;
        private string _roomId;
        private string _accessKey;
        private int _channel = 1;
        private bool _needLogin;
        private DeviceType _deviceType;

        private WsClient _wsClient;
        private TaskCompletionSource<MainMessage> _mainMessageCompleted = null;
        private TaskCompletionSource<bool> _connectCompleted = null;

        public event EventHandler onMessage;

        public DeviceBase(string HardwareId, string Model, string FirmwareVersion, DeviceType Type = DeviceType.TOUCH, bool needLogin = true)
        {
            _hwid = HardwareId;
            _fwVer = FirmwareVersion;
            _model = Model;
            _deviceType = Type;
            _needLogin = needLogin;

            if(_model.Contains("WT")){
                _model = _model.Remove(_model.Length - 1);
            }
        }

        public void DeviceDispachEvent(DeviceEventArgs e)
        {
            EventHandler handler = onMessage;
            handler?.Invoke(this, e);
        }

        public async Task<string> DeviceRegister()
        {
            HttpClient client = new HttpClient();
            string deviceToken = null;
            try
            {
                string url = String.Format("{0}?manToken={1}&hardwareId={2}&model={3}&fwVer={4}",
                    DEVICE_REGISTER_ENDPOINT,
                    MANUFACTURER_TOKEN,
                    _hwid,
                    _model,
                    _fwVer);
                Log.Debug("HTTP Request >>> " + url);
                HttpResponseMessage response = await client.PostAsync(url, null);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                Log.Debug("HTTP Response <<< " + responseBody);

                var jsonBody = responseBody.FromJson<object>();
                var data = ((Dictionary<string, object>)jsonBody)["data"];
                deviceToken = ((Dictionary<string, object>)data)["deviceToken"].ToString();
                _token = deviceToken;
            }
            catch (Exception e)
            {
                Log.Debug("Error " + e.ToString());
            }
            client.Dispose();

            return deviceToken;
        }

        public string Token { get => _token; }
        public string HardwareId { get => _hwid; }
        public string DeviceId { get => _deviceId; set => _deviceId = value; }
        public string RoomId { get => _roomId; set => _roomId = value; }
        public string AccessKey { get => _accessKey; set => _accessKey = value; }
        public string Model { get => _model; }
        public string FirmwareVersion { get => _fwVer; }
        public bool NeedLogin { get => _needLogin; }
        public string HardwareVersion { get => "SIM_1.0.0"; }
        public DeviceType Type { get => _deviceType; }

        public Gateway Gateway { get; set; }


        public LoginServerRequest GetLoginRequestPacket()
        {
            LoginServerRequest req = new LoginServerRequest
            {
                AccessToken = _token,
                //DeviceId = 
            };
            return req;
        }

        public override string ToString()
        {
            return $"HardwareId: {_hwid}{Environment.NewLine}"
                + $"Model: {_model}{Environment.NewLine}"
                + $"FwVersion: {_fwVer}{Environment.NewLine}"
                + $"Token: {_token}{Environment.NewLine}"
                + $"DeviceId: {_deviceId}{Environment.NewLine}"
                + $"AccessKey: {_accessKey}{Environment.NewLine}"
                + $"Channel: {_channel}{Environment.NewLine}"
                ;
        }

        public void On(int channel)
        {

        }

        public void On()
        {

        }

        public void Off()
        {

        }

        public void Off(int channel)
        {

        }

        public void SetNumChannel(int num)
        {
            _channel = num;
        }

        #region Websocket
        public void InitWebsocket()
        {
            _wsClient = new WsClient(WEBSOCKET_ENDPOINT);
            _wsClient.OnConnect += _wsClient_OnConnect;
            _wsClient.OnDisconnect += _wsClient_OnDisconnect;
            _wsClient.OnData += _wsClient_OnData;
        }

        public async Task<bool> Connect(int timeoutMs = 30000)
        {
            _wsClient.start();
            _connectCompleted = new TaskCompletionSource<bool>();
            var ct = new CancellationTokenSource(timeoutMs);
            ct.Token.Register(() => _connectCompleted.TrySetCanceled(), useSynchronizationContext: false);
            var result = await _connectCompleted.Task;

            return await Task.FromResult(result);
        }

        public void Disconnect()
        {
            try
            {
                _wsClient.stop();
            } catch (Exception e)
            {
                Log.Debug("Device disconnect " + e.ToString());
            }
        }

        public async Task<MainMessage> WebsocketRequest(MainMessage mainMessage, int timeoutMs = 10000)
        {
            _mainMessageCompleted = new TaskCompletionSource<MainMessage>();
            var ct = new CancellationTokenSource(timeoutMs);
            ct.Token.Register(() => _mainMessageCompleted?.TrySetCanceled(), useSynchronizationContext: false);

            Log.Debug("Request: =======>");
            Log.Debug(mainMessage.ToString());
            await _wsClient.SendMessageAsync(mainMessage.ToByteArray());
            MainMessage result = await _mainMessageCompleted.Task;

            Log.Debug("Response: <========");
            Log.Debug(result.ToString());
            _mainMessageCompleted = null;
            return await Task.FromResult(result);
        }

        public async void WebsocketRequestWithoutResponse(MainMessage mainMessage)
        {
            Log.Debug("RequestWithoutResponse: =======>");
            Log.Debug(mainMessage.ToString());
            await _wsClient.SendMessageAsync(mainMessage.ToByteArray());
        }

        private void _wsClient_OnData(object sender, EventArgs e)
        {
            MessageEventArgs dataArg = (MessageEventArgs)e;
            MainMessage mainMessage = MainMessage.Parser.ParseFrom(dataArg.Data());
            if (_mainMessageCompleted != null)
            {
                _mainMessageCompleted?.SetResult(mainMessage);
            }
            else
            {
                //Log.Debug("DeviceDispachEvent onMessage ");
                DeviceDispachEvent(new DeviceEventArgs(mainMessage));
                
            }

        }

        public async Task<bool> DeviceLogin(IDevice device)
        {
            MainMessage mainMessage = new MainMessage
            {
                ServerMessage = new ServerMessage
                {
                    AuthServerMessage = new AuthServerMessage
                    {
                        LoginServerRequest = new LoginServerRequest
                        {
                            DeviceId = device.DeviceId,
                            DeviceToken = device.Token,
                            HardwareId = device.HardwareId,
                            GatewayId = HardwareId,
                            AccessKey = AccessKey,
                            Model = device.Model,
                            Firmware = "1.0.0",
                            SceneActiveMap = device.Model.Contains("WT3")?(uint)0x10222222:0,
                        }
                    }
                }
            };
            Log.Debug("LOGIN FOR: " + device.Model);
            var res = await WebsocketRequest(mainMessage);
            return await Task.FromResult(res?.ServerMessage?.AuthServerMessage?.LoginServerResponse?.StatusCode?.Code == 0);
        }

        public virtual async Task OnLoginSuccess()
        {
            Log.Debug(HardwareId + " Login success");
            await Task.Run(() => Thread.Sleep(0));
            
        }

        public virtual void ProcessMessage(MainMessage mainMessage)
        {

        }

        private void _wsClient_OnDisconnect(object sender, EventArgs e)
        {
            try
            {
                _connectCompleted?.SetResult(false);

            } catch(Exception ex)
            {
                Log.Debug("Exception " + ex.ToString());
            }
            
        }

        private void _wsClient_OnConnect(object sender, EventArgs e)
        {
            _connectCompleted?.SetResult(true);
        }
        #endregion
    }
}

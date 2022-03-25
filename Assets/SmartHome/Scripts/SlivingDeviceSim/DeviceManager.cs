using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SlivingDeviceSim.Devices;

namespace SlivingDeviceSim
{


    public class DeviceManager
    {
        private List<IDevice> _devices;
        private Gateway _gateway;
        private Mobile _mobile;

        public event EventHandler OnData;
        public event EventHandler OnInitialized;

        public DeviceManager()
        {
            _devices = new List<IDevice>();
        }

        private void ManagerDispachEvent(DeviceEventArgs e)
        {
            EventHandler handler = OnData;
            handler?.Invoke(this, e);
        }

        public void AddDevice(IDevice device)
        {
            if (device.Type == DeviceType.GATEWAY)
            {
                _gateway = (Gateway)device;
                _gateway.onMessage += _gateway_onMessage;
            }
            else if (device.Type == DeviceType.MOBILE)
            {
                _mobile = (Mobile)device;
            }
            else
            {
                _devices.Add(device);
                device.onMessage += Device_onMessage;
            }
        }

        private void Device_onMessage(object sender, EventArgs e)
        {
            ManagerDispachEvent((DeviceEventArgs)e);
        }

        private void _gateway_onMessage(object sender, EventArgs e)
        {
            MainMessage mainMessage = ((DeviceEventArgs)e).MainMessage;
            //Log.Debug("_gateway_onMessage: =======>");
            //Log.Debug(mainMessage.ToString());
            _devices.ForEach(_device =>
            {
                _device.ProcessMessage(mainMessage);
            });

        }

        public void AddDevices(IDevice[] devices)
        {
            for (int i = 0; i < devices.Length; i++)
            {
                AddDevice(devices[i]);
            }
            //_devices.AddRange(devices);
        }


        public async void run()
        {
            _deviceAssignGateway(_gateway);

            await _deviceRegister();

            await _mobile.Connect();
            await _mobile.LoginAndLoad();

            await _mobileAssignDevicesToRoom();

            await _gateway.Connect();
            await _deviceLoginViaGateway();
            EventHandler handler = OnInitialized;
            handler?.Invoke(this, null);
        }

        public void stop()
        {
            _mobile?.Disconnect();
            _gateway?.Disconnect();
            _devices.ForEach(d => d?.Disconnect());
        }


        private async Task _mobileAssignDevicesToRoom()
        {
            Log.Debug("Add Gateway to default room ");
            var dataGatewayAssign = await _mobile.AssignDevice(_gateway);
            string deviceId = dataGatewayAssign.DeviceId;
            string roomId = dataGatewayAssign.RoomId;

            _gateway.DeviceId = deviceId;
            _gateway.AccessKey = _mobile.AccessKey;

            UnityEngine.Debug.Log($"_mobileAssignDevicesToRoom");
            UnityEngine.Debug.Log($"_mobileAssignDevicesToRoom");
            UnityEngine.Debug.Log($"_mobileAssignDevicesToRoom");
            UnityEngine.Debug.Log($"_mobileAssignDevicesToRoom");

            var rooms = _mobile._getDevicesServerResponse?.Rooms;

            foreach (var room in rooms)
            {
                foreach (var device in room.Devices)
                {
                    UnityEngine.Debug.Log($"RoomName: {device.RoomName}, DeviceName: {device.DeviceName}, DeviceId: {device.DeviceId}, HardwareId: {device.HardwareId}, RoomId: {device.RoomId}");
                }
            }

            for (int i = 0; i < _devices.Count; i++)
            {
                Log.Debug("Add device to default room " + i.ToString());
                var dataDeviceAssign = await _mobile.AssignDevice(_devices[i]);
                _devices[i].DeviceId = dataDeviceAssign.DeviceId;
                _devices[i].AccessKey = _mobile.AccessKey;
                _devices[i].RoomId = dataDeviceAssign.RoomId;
            }
        }

        private async Task _deviceRegister()
        {
            Log.Debug("Registering Gateway");
            await _gateway.DeviceRegister();

            for (int i = 0; i < _devices.Count; i++)
            {
                Log.Debug("Register device " + i.ToString());
                await _devices[i].DeviceRegister();
            }
        }

        private async Task _deviceLoginViaGateway()
        {
            Log.Debug("Gateway Logging");
            var logged = await _gateway.DeviceLogin(_gateway);

            if (logged)
            {
                await _gateway.OnLoginSuccess();
                Log.Debug("Gateway Logged " + _gateway.ToString());
            }

            for (int i = 0; i < _devices.Count; i++)
            {
                Log.Debug("Device Logging " + i.ToString());
                if (_devices[i].NeedLogin)
                {
                    logged = await _gateway.DeviceLogin(_devices[i]);
                }
                else
                {
                    logged = true;
                }

                Log.Debug(_devices[i].HardwareId + " | logged:" + logged.ToString());
                if (logged)
                {
                    await _devices[i].OnLoginSuccess();
                }

            }
        }

        private void _deviceAssignGateway(Gateway gateway)
        {
            for (int i = 0; i < _devices.Count; i++)
            {
                _devices[i].Gateway = gateway;
            }
        }

    }


}

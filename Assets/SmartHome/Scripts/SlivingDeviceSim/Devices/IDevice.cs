using System;
using System.Threading.Tasks;

namespace SlivingDeviceSim.Devices
{
    public interface IDevice
    {
        Task<string> DeviceRegister();
        string Token { get; }
        string DeviceId { get; set; }
        string RoomId { get; set; }
        string AccessKey { get; set; }
        string HardwareId { get; }
        
        string Model { get;  }
        string FirmwareVersion { get;  }
        string HardwareVersion { get;  }
        bool NeedLogin { get;  }
        DeviceType Type { get;  }
        Gateway Gateway { get; set; }

        LoginServerRequest GetLoginRequestPacket();

        void On(int channel);
        void On();
        void Off(int channel);
        void Off();

        Task<bool> Connect(int timeoutMs = 10000);
        void Disconnect();

        void InitWebsocket();
        Task<MainMessage> WebsocketRequest(MainMessage mainMessage, int timeoutMs = 2000);
        Task<bool> DeviceLogin(IDevice device);
        void ProcessMessage(MainMessage mainMessage);
        Task OnLoginSuccess();

        event EventHandler onMessage;
        void DeviceDispachEvent(DeviceEventArgs e);
    }

}

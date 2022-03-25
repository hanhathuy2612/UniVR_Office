using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;

namespace SlivingDeviceSim.Devices
{
    public class Gateway: DeviceBase
    {
        public Gateway(string hardwareId, string model, string firmwareVersion): base(hardwareId, model, firmwareVersion, DeviceType.GATEWAY)
        {
            InitWebsocket();
        }
    }
}

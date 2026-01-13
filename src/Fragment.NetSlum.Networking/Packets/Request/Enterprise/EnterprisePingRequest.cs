using System.Collections.Generic;
using System.Threading.Tasks;
using Fragment.NetSlum.Networking.Attributes;
using Fragment.NetSlum.Networking.Constants;
using Fragment.NetSlum.Networking.Objects;

namespace Fragment.NetSlum.Networking.Packets.Request.Enterprise;

[FragmentPacket(MessageType.EnterprisePing)]
public class EnterprisePingRequest : BaseRequest
{
    public override ValueTask<ICollection<FragmentMessage>> GetResponse(Fragment.NetSlum.Networking.Sessions.FragmentTcpSession session, FragmentMessage request)
    {
        // Reply with same message type as a simple acknowledgement
        var resp = new FragmentMessage
        {
            MessageType = MessageType.EnterprisePing,
            Data = request.Data.ToArray(),
        };

        return SingleMessage(resp);
    }
}

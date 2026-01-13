using System.Collections.Generic;
using System.Threading.Tasks;
using Fragment.NetSlum.Networking.Attributes;
using Fragment.NetSlum.Networking.Constants;
using Fragment.NetSlum.Networking.Objects;
using Fragment.NetSlum.Networking.Packets.Request;

namespace Fragment.NetSlum.Networking.Packets.Request.Enterprise;

[FragmentPacket(MessageType.Data, OpCodes.DataUpdateCheckRequest)]
public class UpdateCheckRequest : BaseRequest
{
    public override ValueTask<ICollection<FragmentMessage>> GetResponse(Fragment.NetSlum.Networking.Sessions.FragmentTcpSession session, FragmentMessage request)
    {
        // Default behavior: reply with NoUpdate opcode (0x6822) and empty payload
        var resp = new FragmentMessage
        {
            MessageType = MessageType.Data,
            DataPacketType = OpCodes.DataUpdateCheckNoUpdate,
            Data = System.Array.Empty<byte>(),
        };

        return SingleMessage(resp);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Fragment.NetSlum.Networking.Attributes;
using Fragment.NetSlum.Networking.Constants;
using Fragment.NetSlum.Networking.Objects;
using Fragment.NetSlum.Networking.Packets.Request;

namespace Fragment.NetSlum.Networking.Packets.Request.Enterprise;

[FragmentPacket(MessageType.HostAssignment)]
public class HostAssignmentRequest : BaseRequest
{
    public override ValueTask<ICollection<FragmentMessage>> GetResponse(Fragment.NetSlum.Networking.Sessions.FragmentTcpSession session, FragmentMessage request)
    {
        Log.Information("Host assignment received for session {SessionId}", session.Id);

        // Typically a HostAssignment is informational; no response required by default.
        return NoResponse();
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fragment.NetSlum.Networking.Attributes;
using Fragment.NetSlum.Networking.Constants;
using Fragment.NetSlum.Networking.Objects;
using Fragment.NetSlum.Networking.Packets.Request;

namespace Fragment.NetSlum.Networking.Packets.Request.Enterprise;

[FragmentPacket(MessageType.GameServerTransfer)]
public class GameServerTransferRequest : BaseRequest
{
    private readonly Fragment.NetSlum.Networking.Services.Enterprise.EnterpriseRouter _router;

    public GameServerTransferRequest(Fragment.NetSlum.Networking.Services.Enterprise.EnterpriseRouter router)
    {
        _router = router;
    }

    public override ValueTask<ICollection<FragmentMessage>> GetResponse(Fragment.NetSlum.Networking.Sessions.FragmentTcpSession session, FragmentMessage request)
    {
        Log.Information("GameServerTransfer request from session {SessionId}; payload length={Len}", session.Id, request.Data.Length);

        var raw = request.Data.ToArray();

        Fragment.NetSlum.Networking.Sessions.FragmentTcpSession? target = null;

        if (raw.Length >= 4)
            target = _router.FindByServerId(raw.AsSpan());

        if (target == null)
        {
            try
            {
                var name = System.Text.Encoding.UTF8.GetString(raw).Split('\0')[0];
                if (!string.IsNullOrEmpty(name))
                    target = _router.FindByName(name);
            }
            catch { }
        }

        if (target != null)
        {
            var fwd = new FragmentMessage
            {
                MessageType = MessageType.GameServerTransfer,
                DataPacketType = request.DataPacketType,
                Data = request.Data.ToArray(),
            };

            target.Send(new System.Collections.Generic.List<FragmentMessage> { fwd });
            Log.Information("Forwarded GameServerTransfer to AreaServer {Name} (Session: {SessionId})", target.AreaServerInfo?.ServerName, target.Id);
        }
        else
        {
            Log.Warning("GameServerTransfer: no matching target found for session {SessionId}", session.Id);
        }

        return NoResponse();
    }
}

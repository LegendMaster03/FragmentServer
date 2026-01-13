using System.Collections.Generic;
using System.Threading.Tasks;
using Fragment.NetSlum.Networking.Attributes;
using Fragment.NetSlum.Networking.Constants;
using Fragment.NetSlum.Networking.Objects;
using Fragment.NetSlum.Networking.Packets.Request;

namespace Fragment.NetSlum.Networking.Packets.Request.Enterprise;

[FragmentPacket(MessageType.HostRegistration)]
public class HostRegistrationRequest : BaseRequest
{
    private readonly Fragment.NetSlum.Networking.Services.Enterprise.EnterpriseRouter _router;
    private readonly Fragment.NetSlum.Networking.Services.DynaLink.DynaLinkHostRegistry _hostRegistry;

    public HostRegistrationRequest(Fragment.NetSlum.Networking.Services.Enterprise.EnterpriseRouter router, Fragment.NetSlum.Networking.Services.DynaLink.DynaLinkHostRegistry hostRegistry)
    {
        _router = router;
        _hostRegistry = hostRegistry;
    }

    public override ValueTask<ICollection<FragmentMessage>> GetResponse(Fragment.NetSlum.Networking.Sessions.FragmentTcpSession session, FragmentMessage request)
    {
        Log.Information("Host registration request from session {SessionId}; payload len={Len}", session.Id, request.Data.Length);

        // If session already has AreaServerInfo (published through existing login flow), register it.
        if (session.AreaServerInfo != null)
        {
            _router.Register(session);
            // Also register as a DynaLink host if applicable
            _hostRegistry?.RegisterHost(session, session.AreaServerInfo?.ServerId, session.AreaServerInfo?.ServerName);
            return NoResponse();
        }

        // Otherwise attempt to parse a simple registration payload: [serverId(<=64)][0x00][name...]
        try
        {
            var raw = request.Data.ToArray();
            // Heuristic: find first null byte as separator
            var sep = System.Array.IndexOf(raw, (byte)0);
            var idBytes = sep > 0 ? raw[..sep] : raw;
            var name = sep > 0 && sep + 1 < raw.Length ? System.Text.Encoding.UTF8.GetString(raw[(sep + 1)..]).Split('\0')[0] : string.Empty;

            session.AreaServerInfo = new Fragment.NetSlum.Networking.Models.AreaServerInformation
            {
                ServerId = idBytes,
                ServerName = name,
                ActiveSince = System.DateTime.UtcNow
            };

            _router.Register(session);
            _hostRegistry?.RegisterHost(session, session.AreaServerInfo?.ServerId, session.AreaServerInfo?.ServerName);
        }
        catch
        {
            // Ignore parse errors, treat as no-op
        }

        return NoResponse();
    }
}

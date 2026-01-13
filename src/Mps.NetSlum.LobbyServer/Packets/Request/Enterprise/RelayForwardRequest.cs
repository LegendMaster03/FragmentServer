using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fragment.NetSlum.Networking.Attributes;
using Mps.NetSlum.LobbyServer.Constants;
using Fragment.NetSlum.Networking.Objects;
using Fragment.NetSlum.Networking.Packets.Request;

namespace Fragment.NetSlum.Networking.Packets.Request.Enterprise;

[FragmentPacket(MessageType.RelayForward)]
public class RelayForwardRequest : BaseRequest
{
    private readonly Fragment.NetSlum.Networking.Services.Enterprise.EnterpriseRouter _router;
    private readonly Fragment.NetSlum.Networking.Services.DynaLink.DynaLinkHostRegistry _hostRegistry;

    public RelayForwardRequest(Fragment.NetSlum.Networking.Services.Enterprise.EnterpriseRouter router, Fragment.NetSlum.Networking.Services.DynaLink.DynaLinkHostRegistry hostRegistry)
    {
        _router = router;
        _hostRegistry = hostRegistry;
    }

    public override ValueTask<ICollection<FragmentMessage>> GetResponse(Fragment.NetSlum.Networking.Sessions.FragmentTcpSession session, FragmentMessage request)
    {
        Log.Information("Received RelayForward from session {SessionId}; payload len={Len}", session.Id, request.Data.Length);

        var span = request.Data.Span;

        // New encapsulation format (DynaLink): [targetLen:byte][targetBytes][innerPayload...]
        // Fallback: existing heuristics (treat whole payload as id or name)
        Fragment.NetSlum.Networking.Sessions.FragmentTcpSession? target = null;
        byte targetLen = 0;
        if (span.Length >= 1)
        {
            targetLen = span[0];
        }

        ReadOnlySpan<byte> innerPayload = ReadOnlySpan<byte>.Empty;
        if (targetLen > 0 && span.Length >= 1 + targetLen)
        {
            var targetBytes = span.Slice(1, targetLen).ToArray();
            innerPayload = span.Slice(1 + targetLen);

            // Try id match first
            target = _router.FindByServerId(targetBytes);
            if (target == null)
            {
                // try host registry
                try
                {
                    var asString = System.Text.Encoding.UTF8.GetString(targetBytes).Split('\0')[0];
                    if (!string.IsNullOrEmpty(asString))
                    {
                        target = _router.FindByName(asString) ?? _hostRegistry?.FindByName(asString);
                    }
                }
                catch { }
            }
        }
        else
        {
            // Fallback to legacy heuristics: try full payload as id or name
            var raw = span.ToArray();
            if (raw.Length >= 4)
            {
                target = _router.FindByServerId(raw.AsSpan()) ?? _hostRegistry?.FindById(raw.AsSpan());
            }

            if (target == null)
            {
                try
                {
                    var asString = System.Text.Encoding.UTF8.GetString(raw).Split('\0')[0];
                    if (!string.IsNullOrEmpty(asString))
                        target = _router.FindByName(asString) ?? _hostRegistry?.FindByName(asString);
                }
                catch { }
            }

            innerPayload = span;
        }

        if (target != null)
        {
            var fwd = new FragmentMessage
            {
                MessageType = MessageType.Data,
                DataPacketType = request.DataPacketType,
                Data = innerPayload.ToArray(),
            };

            target.Send(new System.Collections.Generic.List<FragmentMessage> { fwd });
            Log.Information("RelayForward forwarded to {Name} (session {Id})", target.AreaServerInfo?.ServerName ?? "<unknown>", target.Id);
        }
        else
        {
            Log.Warning("RelayForward: no target found for session {SessionId}", session.Id);
        }

        return NoResponse();
    }
}


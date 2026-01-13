using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fragment.NetSlum.Networking.Attributes;
using Mps.NetSlum.LobbyServer.Constants;
using Fragment.NetSlum.Networking.Objects;
using Fragment.NetSlum.Networking.Packets.Request;
using Fragment.NetSlum.Networking.Services.Enterprise;

namespace Fragment.NetSlum.Networking.Packets.Request.Enterprise;

[FragmentPacket(MessageType.ServerListUpdate)]
public class ServerListUpdateRequest : BaseRequest
{
    private readonly EnterpriseRouter _router;

    public ServerListUpdateRequest(EnterpriseRouter router)
    {
        _router = router;
    }

    public override ValueTask<ICollection<FragmentMessage>> GetResponse(Fragment.NetSlum.Networking.Sessions.FragmentTcpSession session, FragmentMessage request)
    {
        // Expect payload as newline-separated entries of `id|name` where id is hex string of server id
        try
        {
            var text = System.Text.Encoding.UTF8.GetString(request.Data.ToArray());
            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split('|', 2);
                if (parts.Length < 1)
                    continue;

                var idHex = parts[0].Trim();
                var name = parts.Length > 1 ? parts[1].Trim() : string.Empty;

                try
                {
                    var idBytes = Convert.FromHexString(idHex);
                    var target = _router.FindByServerId(idBytes);
                    if (target != null && target.AreaServerInfo != null)
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            target.AreaServerInfo.ServerName = name;
                        }
                    }
                }
                catch
                {
                    // ignore malformed hex
                }
            }
        }
        catch (Exception)
        {
            // ignore parse errors
        }

        return NoResponse();
    }
}

using System;
using System.Collections.Concurrent;
using Fragment.NetSlum.Networking.Models;
using Fragment.NetSlum.Core.Extensions;
using Fragment.NetSlum.Networking.Sessions;
using Fragment.NetSlum.Networking.Objects;
using Serilog;

namespace Fragment.NetSlum.Networking.Services.Enterprise;

public class EnterpriseRouter
{
    private readonly ConcurrentDictionary<string, FragmentTcpSession> _byName = new();
    private readonly ConcurrentDictionary<string, FragmentTcpSession> _byId = new();

    private ILogger Log => Serilog.Log.ForContext<EnterpriseRouter>();

    public void Register(FragmentTcpSession session)
    {
        if (session.AreaServerInfo == null)
            return;

        var id = session.AreaServerInfo.ServerId.Span.ToHexString();
        var name = session.AreaServerInfo.ServerName ?? string.Empty;

        if (!string.IsNullOrEmpty(id))
            _byId[id] = session;

        if (!string.IsNullOrEmpty(name))
            _byName[name] = session;

        Log.Information("Registered area server '{Name}' id={Id} session={SessionId}", name, id, session.Id);
    }

    public void Unregister(FragmentTcpSession session)
    {
        if (session.AreaServerInfo == null)
            return;

        var id = session.AreaServerInfo.ServerId.Span.ToHexString();
        var name = session.AreaServerInfo.ServerName ?? string.Empty;

        if (!string.IsNullOrEmpty(id))
            _byId.TryRemove(id, out _);

        if (!string.IsNullOrEmpty(name))
            _byName.TryRemove(name, out _);

        Log.Information("Unregistered area server '{Name}' id={Id} session={SessionId}", name, id, session.Id);
    }

    public FragmentTcpSession? FindByServerId(ReadOnlySpan<byte> serverId)
    {
        var id = serverId.ToArray().ToHexString();
        if (_byId.TryGetValue(id, out var s))
            return s;
        return null;
    }

    public FragmentTcpSession? FindByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        if (_byName.TryGetValue(name, out var s))
            return s;

        return null;
    }
}

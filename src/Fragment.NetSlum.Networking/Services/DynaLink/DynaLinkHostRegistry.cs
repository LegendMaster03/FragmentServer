using System;
using System.Collections.Concurrent;
using Fragment.NetSlum.Networking.Sessions;
using Fragment.NetSlum.Core.Extensions;

namespace Fragment.NetSlum.Networking.Services.DynaLink
{
    public class DynaLinkHostRegistry
    {
        private readonly ConcurrentDictionary<string, FragmentTcpSession> _byName = new();
        private readonly ConcurrentDictionary<string, FragmentTcpSession> _byId = new();

        public void RegisterHost(FragmentTcpSession session, ReadOnlyMemory<byte>? id = null, string? name = null)
        {
            if (id != null && id.Value.Length > 0)
            {
                var key = id.Value.ToArray().ToHexString();
                _byId[key] = session;
            }

            if (!string.IsNullOrEmpty(name))
            {
                _byName[name] = session;
            }
        }

        public void UnregisterHost(FragmentTcpSession session)
        {
            // remove by id
            foreach (var kv in _byId)
            {
                if (kv.Value == session)
                    _byId.TryRemove(kv.Key, out _);
            }

            foreach (var kv in _byName)
            {
                if (kv.Value == session)
                    _byName.TryRemove(kv.Key, out _);
            }
        }

        public FragmentTcpSession? FindByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return _byName.TryGetValue(name, out var s) ? s : null;
        }

        public FragmentTcpSession? FindById(ReadOnlySpan<byte> id)
        {
            var key = id.ToArray().ToHexString();
            return _byId.TryGetValue(key, out var s) ? s : null;
        }
    }
}

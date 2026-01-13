using System.Buffers.Binary;
using System.Collections.Generic;
using Fragment.NetSlum.Networking.Constants;
using Fragment.NetSlum.Networking.Compatibility;
using Fragment.NetSlum.Networking.Objects;

namespace Fragment.NetSlum.Networking.Pipeline.Decoders;

public class AdapterCompatibilityDecoder : IPacketDecoder
{
    private readonly CompatibilityAdapterRegistry _registry;

    public AdapterCompatibilityDecoder(CompatibilityAdapterRegistry registry)
    {
        _registry = registry;
    }

    public int Decode(System.Memory<byte> data, List<FragmentMessage> messages)
    {
        // Delegate decoding to registered adapters. The first adapter that consumes bytes wins.
        return _registry.TryDecode(data, messages);
    }
}

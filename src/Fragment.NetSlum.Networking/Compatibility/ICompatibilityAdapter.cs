using System.Collections.Generic;
using Fragment.NetSlum.Networking.Objects;

namespace Fragment.NetSlum.Networking.Compatibility
{
    public interface ICompatibilityAdapter
    {
        /// <summary>
        /// Adapter name (e.g. "SBOL").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Try decode one or more messages from the provided buffer.
        /// Return the number of bytes consumed from the buffer (0 if not handled).
        /// If messages are produced, they should be added to <paramref name="messages"/>.
        /// </summary>
        int Decode(System.Memory<byte> data, List<FragmentMessage> messages);
    }
}

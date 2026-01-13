using System.Buffers.Binary;
using System.Collections.Generic;
using Fragment.NetSlum.Networking.Constants;
using Fragment.NetSlum.Networking.Objects;

namespace Fragment.NetSlum.Networking.Compatibility
{
    public class SbolCompatibilityAdapter : ICompatibilityAdapter
    {
        public string Name => "SBOL";

        public int Decode(System.Memory<byte> data, List<FragmentMessage> messages)
        {
            // Allow enabling via environment variable to avoid changing runtime behavior by default
            var enabled = System.Environment.GetEnvironmentVariable("MPS_SBOL_COMPAT") == "1";
            if (!enabled)
                return 0;

            var span = data.Span;
            int pos = 0;

            if (span.Length < 2)
                return 0;

            // SBOL packets: [size: ushort BE][type: ushort BE][subtype: ushort BE][payload...]
            ushort datalen = BinaryPrimitives.ReadUInt16BigEndian(span[pos..2]);
            pos += 2;

            if (datalen > span.Length - 2)
                return 0;

            if (datalen < 2)
                return pos;

            var messageContent = span[pos..(pos + datalen)];
            pos += messageContent.Length;

            // Read opcodes/type and subtype if present
            var type = BinaryPrimitives.ReadUInt16BigEndian(messageContent[..2]);
            ushort subType = 0;
            if (messageContent.Length >= 4)
                subType = BinaryPrimitives.ReadUInt16BigEndian(messageContent[2..4]);

            var payload = messageContent.Length > 4 ? messageContent[4..].ToArray() : System.Array.Empty<byte>();

            messages.Add(new FragmentMessage
            {
                MessageType = MessageType.Data,
                DataPacketType = (OpCodes)type,
                Data = payload,
            });

            return pos;
        }
    }
}

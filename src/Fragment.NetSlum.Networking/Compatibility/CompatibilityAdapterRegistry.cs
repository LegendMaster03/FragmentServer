using System.Collections.Generic;
using System.Linq;
using Fragment.NetSlum.Networking.Objects;

namespace Fragment.NetSlum.Networking.Compatibility
{
    public class CompatibilityAdapterRegistry
    {
        private readonly List<ICompatibilityAdapter> _adapters;

        public CompatibilityAdapterRegistry(IEnumerable<ICompatibilityAdapter> adapters)
        {
            _adapters = adapters?.ToList() ?? new List<ICompatibilityAdapter>();
        }

        public int TryDecode(System.Memory<byte> data, List<FragmentMessage> messages)
        {
            foreach (var a in _adapters)
            {
                var consumed = a.Decode(data, messages);
                if (consumed > 0)
                    return consumed;
            }

            return 0;
        }

        public void Register(ICompatibilityAdapter adapter)
        {
            lock (_adapters)
            {
                _adapters.Add(adapter);
            }
        }

        public IReadOnlyList<ICompatibilityAdapter> Adapters => _adapters;
    }
}

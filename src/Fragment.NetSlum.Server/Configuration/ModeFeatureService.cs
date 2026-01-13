using System;

namespace Fragment.NetSlum.Server.Configuration
{
    public class ModeFeatureService
    {
        public ServerCapabilities GetCapabilities(GameMode mode)
        {
            return mode switch
            {
                // These legacy modes currently only have compatibility adapter support by default
                GameMode.Classic => ServerCapabilities.ClassicMode | ServerCapabilities.CompatibilityAdapters,
                GameMode.Light => ServerCapabilities.LightMode | ServerCapabilities.CompatibilityAdapters,
                GameMode.Cluster => ServerCapabilities.ClusterMode | ServerCapabilities.CompatibilityAdapters,
                GameMode.Enterprise => ServerCapabilities.EnterpriseRouting | ServerCapabilities.CompatibilityAdapters | ServerCapabilities.HostAssignment,
                GameMode.DynaLink => ServerCapabilities.Relay | ServerCapabilities.CompatibilityAdapters,
                GameMode.PlayerServer => ServerCapabilities.PeerMatchmaking | ServerCapabilities.CompatibilityAdapters,
                _ => ServerCapabilities.None
            };
        }

        public bool HasCapability(GameMode mode, ServerCapabilities cap)
        {
            var c = GetCapabilities(mode);
            return (c & cap) == cap;
        }
    }
}

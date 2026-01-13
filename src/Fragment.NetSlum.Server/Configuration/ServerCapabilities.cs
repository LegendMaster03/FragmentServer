using System;

namespace Fragment.NetSlum.Server.Configuration
{
    [Flags]
    public enum ServerCapabilities
    {
        None = 0,
        // Supports enterprise-style area-server registration and routing
        EnterpriseRouting = 1 << 0,
        // Supports relaying traffic to hide player IPs (DynaLink implementation)
        Relay = 1 << 1,
        // Supports peer-to-peer matchmaking / player-hosted servers
        PeerMatchmaking = 1 << 2,
        // Compatibility adapter system available
        CompatibilityAdapters = 1 << 3,
        // Host assignment / system controller behaviors
        HostAssignment = 1 << 4,
        // Legacy/placeholder flags for modes that are not fully implemented yet
        ClassicMode = 1 << 8,
        LightMode = 1 << 9,
        ClusterMode = 1 << 10,
    }
}

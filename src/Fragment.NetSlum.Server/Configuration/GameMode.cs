namespace Fragment.NetSlum.Server.Configuration
{
    /// <summary>
    /// Logical modes a game can run under. These drive which server
    /// features/capabilities are enabled for that game.
    ///
    /// Notes:
    /// - <c>Classic</c> and <c>Cluster</c> are legacy modes that were
    ///   discontinued and their behaviors were merged into <c>Enterprise</c>.
    ///   They are retained for compatibility with older games but are not
    ///   a priority to re-implement fully.
    /// - <c>PlayerServer</c> is the peer-to-peer matchmaking/hosted-server
    ///   mode (the original P2P flow).
    /// - <c>DynaLink</c> is the user-hosted server mode with server-side
    ///   relaying to hide player IP addresses.
    /// </summary>
    public enum GameMode
    {
        Classic,
        Light,
        Cluster,
        Enterprise,
        DynaLink,
        PlayerServer
    }
}

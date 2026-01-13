using System;

namespace Fragment.NetSlum.Server.Configuration
{
    /// <summary>
    /// Guard that enforces which modes/features are considered implemented.
    /// Unimplemented modes will throw a clear NotSupportedException when attempted to be used.
    /// </summary>
    public class ModeGuard
    {
        private readonly ModeFeatureService _features;

        public ModeGuard(ModeFeatureService features)
        {
            _features = features;
        }

        public void EnsureModeSupported(GameMode mode)
        {
            // For now, Classic, Light and Cluster are considered unimplemented stubs
            // Classic and Cluster are legacy modes that were merged into Enterprise;
            // we keep them here for compatibility with older game configurations,
            // but they will raise a clear error until implemented.
            if (mode == GameMode.Classic || mode == GameMode.Light || mode == GameMode.Cluster)
            {
                throw new NotSupportedException($"Game mode '{mode}' is not yet implemented. Classic and Cluster are legacy modes (merged into Enterprise). Please use a supported mode: PlayerServer, DynaLink, or Enterprise.");
            }
        }

        public void EnsureHasCapability(GameMode mode, ServerCapabilities capability)
        {
            var caps = _features.GetCapabilities(mode);
            if ((caps & capability) != capability)
                throw new NotSupportedException($"Mode '{mode}' does not support capability '{capability}'.");
        }
    }
}

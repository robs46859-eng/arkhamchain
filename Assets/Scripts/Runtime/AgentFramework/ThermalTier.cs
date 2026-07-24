namespace ArkhamIsland.AgentFramework
{
    /// <summary>
    /// Defines the thermal tiers used in the Cascade Agent Architecture.
    /// Temperature controls the creative freedom vs. deterministic rigor tradeoff.
    /// </summary>
    public enum ThermalTier
    {
        /// <summary>
        /// Hot (≈ 0.85–1.1): Creative Engine.
        /// Generates cosmic horror, surreal aesthetics, dream-logic geometry.
        /// High variance, unconstrained creative output.
        /// </summary>
        Hot,

        /// <summary>
        /// Balanced (≈ 0.4–0.6): Gameplay Integrator.
        /// Balances creative intent with system loops, sanity mechanics, biometrics,
        /// and player interaction logic.
        /// </summary>
        Balanced,

        /// <summary>
        /// Cool (≈ 0.0–0.2): Deterministic Physics & Engine Anchor.
        /// Zero creative freedom. Strict math, spatial mapping, VR optimization,
        /// hitboxes, collision meshes, inverse kinematics (IK), and frame-rate budgets.
        /// </summary>
        Cool
    }

    /// <summary>
    /// Utility methods for working with thermal tiers and temperature values.
    /// </summary>
    public static class ThermalTierUtils
    {
        /// <summary>
        /// Returns the ThermalTier classification for a given temperature value.
        /// </summary>
        public static ThermalTier Classify(float temperature)
        {
            if (temperature >= 0.7f) return ThermalTier.Hot;
            if (temperature >= 0.3f) return ThermalTier.Balanced;
            return ThermalTier.Cool;
        }

        /// <summary>
        /// Validates that a downstream handoff respects temperature monotonicity.
        /// A hot agent MUST NOT call a hotter agent downstream.
        /// </summary>
        public static bool IsValidCascade(float sourceTemp, float targetTemp)
        {
            return targetTemp <= sourceTemp;
        }

        /// <summary>
        /// Returns a human-readable label for the thermal tier.
        /// </summary>
        public static string GetLabel(ThermalTier tier)
        {
            switch (tier)
            {
                case ThermalTier.Hot: return "Creative Engine";
                case ThermalTier.Balanced: return "Gameplay Integrator";
                case ThermalTier.Cool: return "Deterministic Anchor";
                default: return "Unknown";
            }
        }
    }
}

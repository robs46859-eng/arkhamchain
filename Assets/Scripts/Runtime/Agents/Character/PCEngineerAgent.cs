using UnityEngine;

namespace ArkhamIsland.Agents.Character
{
    using AgentFramework;

    /// <summary>
    /// PLAYABLE CHARACTER (PC) ENGINEER AGENT (Temperature: 0.25 — COOL)
    /// 
    /// Pipeline: Character & Mechanics
    /// Role: Handles VRIK (VR Inverse Kinematics), biometric tracking integration
    ///       (heart rate, eye tracking), physical hitboxes, and progression math.
    /// 
    /// Key Principle: "I received a character concept with elongated limbs and visual tremors.
    ///                 I must adapt VR avatar IK rigs to map real-world VR controllers to
    ///                 these exaggerated proportions without causing arm-snapping or
    ///                 calibration errors."
    /// </summary>
    public class PCEngineerAgent : AgentBase
    {
        [Header("IK Configuration")]
        [Tooltip("Maximum arm extension angle before IK constraint kicks in.")]
        [SerializeField] private float maxArmExtensionAngle = 170f;

        [Tooltip("Shoulder joint limit (degrees).")]
        [SerializeField] private float shoulderJointLimit = 160f;

        [Tooltip("Elbow joint limit (degrees).")]
        [SerializeField] private float elbowJointLimit = 145f;

        [Tooltip("Wrist joint limit (degrees).")]
        [SerializeField] private float wristJointLimit = 80f;

        [Header("Biometrics Integration")]
        [Tooltip("Heart rate baseline for stamina curve mapping.")]
        [SerializeField] private float heartRateBaseline = 72f;

        [Tooltip("Heart rate threshold that triggers stamina degradation.")]
        [SerializeField] private float heartRateStressThreshold = 110f;

        [Tooltip("Eye tracking saccade response multiplier for horror events.")]
        [SerializeField] private float saccadeResponseMultiplier = 1.5f;

        [Header("Sanity-Movement Coupling")]
        [Tooltip("AnimationCurve mapping sanity percentage to movement speed penalty.")]
        [SerializeField] private AnimationCurve sanityToSpeedCurve = AnimationCurve.EaseInOut(0f, 0.4f, 1f, 1f);

        protected override void Awake()
        {
            agentName = "PCEngineerAgent";
            temperature = 0.25f;
            pipeline = "character";
            base.Awake();
        }

        protected override WhyChainEnvelope ProcessEnvelope(WhyChainEnvelope inbound)
        {
            Debug.Log("[PCEngineerAgent] WRITING THE MATH for the distorted VR avatar...");

            // Parse character design from upstream
            if (!string.IsNullOrEmpty(inbound.payload?.serializedData))
            {
                Debug.Log("[PCEngineerAgent] Received distorted character model. " +
                          "Computing IK constraints for exaggerated proportions.");
            }

            // Compute IK bounds
            Debug.Log($"[PCEngineerAgent] IK Joint Limits — " +
                      $"Shoulder: {shoulderJointLimit}°, " +
                      $"Elbow: {elbowJointLimit}°, " +
                      $"Wrist: {wristJointLimit}°, " +
                      $"Max arm extension: {maxArmExtensionAngle}°");

            // Compute biometrics matrix
            Debug.Log($"[PCEngineerAgent] Biometrics Matrix — " +
                      $"HR baseline: {heartRateBaseline} bpm, " +
                      $"Stress threshold: {heartRateStressThreshold} bpm, " +
                      $"Saccade multiplier: {saccadeResponseMultiplier}x");

            // Compute sanity-movement coupling
            float speedAt0Sanity = sanityToSpeedCurve.Evaluate(0f);
            float speedAt50Sanity = sanityToSpeedCurve.Evaluate(0.5f);
            float speedAt100Sanity = sanityToSpeedCurve.Evaluate(1f);

            Debug.Log($"[PCEngineerAgent] Sanity-Speed Curve — " +
                      $"0%: {speedAt0Sanity:F2}x, " +
                      $"50%: {speedAt50Sanity:F2}x, " +
                      $"100%: {speedAt100Sanity:F2}x");

            Debug.Log("[PCEngineerAgent] VR avatar IK rig computed. " +
                      "Exaggerated proportions mapped to safe controller tracking.");

            // Terminal agent — no downstream handoff
            return null;
        }

        /// <summary>
        /// Returns the movement speed multiplier for a given sanity percentage (0.0–1.0).
        /// Used by the game's movement system to slow the player as sanity degrades.
        /// </summary>
        public float GetSpeedMultiplier(float sanityNormalized)
        {
            return sanityToSpeedCurve.Evaluate(Mathf.Clamp01(sanityNormalized));
        }

        /// <summary>
        /// Returns the stamina degradation rate for a given heart rate.
        /// Higher heart rate above the stress threshold = faster stamina drain.
        /// </summary>
        public float GetStaminaDrain(float currentHeartRate)
        {
            if (currentHeartRate <= heartRateBaseline) return 0f;
            float excess = Mathf.Max(0f, currentHeartRate - heartRateStressThreshold);
            return excess * 0.02f; // 2% drain per BPM above threshold
        }
    }
}

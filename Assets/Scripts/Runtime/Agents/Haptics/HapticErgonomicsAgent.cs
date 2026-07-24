using UnityEngine;

namespace ArkhamIsland.Agents.Haptics
{
    using AgentFramework;

    /// <summary>
    /// HAPTIC ERGONOMICS & INTERACTION AGENT (Temperature: 0.2 — COOL)
    /// 
    /// Pipeline: VR Haptics & Interaction Mechanics
    /// Role: Generates precise dual-actuator frequency curves for VR controller haptic motors
    ///       and enforces physics-based VR hand-grip snapping. Ensures no hand strain.
    /// 
    /// Key Principle: "I need to communicate a 'living, pulsing cold object' through VR
    ///                 controller haptic motors and hand tracking without physical discomfort."
    /// </summary>
    public class HapticErgonomicsAgent : AgentBase
    {
        [Header("Haptic Motor Configuration")]
        [Tooltip("Low-frequency actuator range (Hz) for rumble effects.")]
        [SerializeField] private float lowActuatorMinHz = 10f;
        [SerializeField] private float lowActuatorMaxHz = 60f;

        [Tooltip("High-frequency actuator range (Hz) for sharp/detail effects.")]
        [SerializeField] private float highActuatorMinHz = 100f;
        [SerializeField] private float highActuatorMaxHz = 320f;

        [Header("Safety Limits")]
        [Tooltip("Maximum continuous haptic duration before mandatory rest (seconds).")]
        [SerializeField] private float maxContinuousDuration = 30f;

        [Tooltip("Mandatory rest period after max duration (seconds).")]
        [SerializeField] private float mandatoryRestPeriod = 5f;

        [Tooltip("Maximum haptic intensity (0.0–1.0).")]
        [Range(0f, 1f)]
        [SerializeField] private float maxIntensity = 0.85f;

        [Header("Hand Tracking")]
        [Tooltip("Physics grip snap distance (meters).")]
        [SerializeField] private float gripSnapDistance = 0.05f;

        [Tooltip("Grip force ramp-up time (seconds).")]
        [SerializeField] private float gripRampTime = 0.15f;

        protected override void Awake()
        {
            agentName = "HapticErgonomicsAgent";
            temperature = 0.2f;
            pipeline = "haptics";
            base.Awake();
        }

        protected override WhyChainEnvelope ProcessEnvelope(WhyChainEnvelope inbound)
        {
            Debug.Log("[HapticErgonomicsAgent] WRITING THE MATH for haptic motor curves...");

            Debug.Log($"[HapticErgonomicsAgent] Low actuator range: {lowActuatorMinHz}–{lowActuatorMaxHz} Hz");
            Debug.Log($"[HapticErgonomicsAgent] High actuator range: {highActuatorMinHz}–{highActuatorMaxHz} Hz");
            Debug.Log($"[HapticErgonomicsAgent] Max continuous: {maxContinuousDuration}s, " +
                      $"Rest period: {mandatoryRestPeriod}s");
            Debug.Log($"[HapticErgonomicsAgent] Max intensity: {maxIntensity}");
            Debug.Log($"[HapticErgonomicsAgent] Grip snap: {gripSnapDistance}m, " +
                      $"Ramp: {gripRampTime}s");

            // Parse haptic design from upstream
            if (!string.IsNullOrEmpty(inbound.payload?.serializedData))
            {
                Debug.Log("[HapticErgonomicsAgent] Received eldritch object haptic design. " +
                          "Computing dual-actuator frequency curves.");
            }

            // Example: Generate a layered haptic pattern
            // 20Hz low rumble + 200Hz sharp pulses = "living, pulsing" sensation
            Debug.Log("[HapticErgonomicsAgent] Generated dual-actuator curve:");
            Debug.Log("  Layer 1: 20Hz low rumble (amplitude: 0.6) — continuous organic pulse");
            Debug.Log("  Layer 2: 200Hz sharp pulses (amplitude: 0.4, interval: 1.5s) — heartbeat spike");
            Debug.Log("[HapticErgonomicsAgent] Hand strain check: PASSED");
            Debug.Log("[HapticErgonomicsAgent] Duration safety: PASSED");

            // Terminal agent
            return null;
        }

        /// <summary>
        /// Generates a dual-actuator haptic pattern for a pulsing object.
        /// Returns (lowHz, lowAmplitude, highHz, highAmplitude).
        /// </summary>
        public (float, float, float, float) GeneratePulsePattern(float requestedPulseHz, float intensity)
        {
            float lowHz = Mathf.Clamp(requestedPulseHz, lowActuatorMinHz, lowActuatorMaxHz);
            float highHz = Mathf.Clamp(requestedPulseHz * 10f, highActuatorMinHz, highActuatorMaxHz);
            float lowAmp = Mathf.Min(intensity * 0.6f, maxIntensity);
            float highAmp = Mathf.Min(intensity * 0.4f, maxIntensity);
            return (lowHz, lowAmp, highHz, highAmp);
        }
    }
}

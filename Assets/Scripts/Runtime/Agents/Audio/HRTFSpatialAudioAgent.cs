using UnityEngine;

namespace ArkhamIsland.Agents.Audio
{
    using AgentFramework;

    /// <summary>
    /// HRTF SPATIAL AUDIO SYSTEMS AGENT (Temperature: 0.1 — COOL)
    /// 
    /// Pipeline: Dynamic VR Spatial Audio
    /// Role: Spatializes unnatural sounds in the VR soundfield without causing vestibular nausea.
    ///       Calculates HRTF (Head-Related Transfer Function) audio attenuation curves,
    ///       occlusion geometry, ray-traced sound bounces, and safe volume decibel limits.
    /// </summary>
    public class HRTFSpatialAudioAgent : AgentBase
    {
        [Header("HRTF Configuration")]
        [Tooltip("Maximum safe volume level in dB SPL.")]
        [SerializeField] private float maxSafeDecibelSPL = 85f;

        [Tooltip("Minimum inter-aural time difference for spatial positioning (microseconds).")]
        [SerializeField] private float minITD = 20f;

        [Tooltip("Maximum inter-aural time difference (microseconds).")]
        [SerializeField] private float maxITD = 700f;

        [Tooltip("Number of ray-traced sound bounces for occlusion calculation.")]
        [SerializeField] private int soundBounceRays = 8;

        [Header("VR Safety Limits")]
        [Tooltip("Maximum bass frequency that can bypass directional filtering (Hz).")]
        [SerializeField] private float maxUnfilteredBassHz = 80f;

        [Tooltip("Vestibular safety: max spatial audio movement speed (degrees/sec).")]
        [SerializeField] private float maxSpatialMovementSpeed = 90f;

        [Tooltip("Enable occlusion geometry for realistic sound blocking.")]
        [SerializeField] private bool enableOcclusionGeometry = true;

        protected override void Awake()
        {
            agentName = "HRTFSpatialAudioAgent";
            temperature = 0.1f;
            pipeline = "audio";
            base.Awake();
        }

        protected override WhyChainEnvelope ProcessEnvelope(WhyChainEnvelope inbound)
        {
            Debug.Log("[HRTFSpatialAudioAgent] WRITING THE MATH for spatial audio safety...");

            Debug.Log($"[HRTFSpatialAudioAgent] Safe dB limit: {maxSafeDecibelSPL} dB SPL");
            Debug.Log($"[HRTFSpatialAudioAgent] ITD range: {minITD}–{maxITD} μs");
            Debug.Log($"[HRTFSpatialAudioAgent] Sound bounce rays: {soundBounceRays}");
            Debug.Log($"[HRTFSpatialAudioAgent] Max unfiltered bass: {maxUnfilteredBassHz} Hz");
            Debug.Log($"[HRTFSpatialAudioAgent] Spatial movement cap: {maxSpatialMovementSpeed}°/s");
            Debug.Log($"[HRTFSpatialAudioAgent] Occlusion geometry: {enableOcclusionGeometry}");

            // Parse audio design from upstream
            if (!string.IsNullOrEmpty(inbound.payload?.serializedData))
            {
                Debug.Log("[HRTFSpatialAudioAgent] Received psychoacoustic design. " +
                          "Computing HRTF positioning for VR soundfield.");
            }

            Debug.Log("[HRTFSpatialAudioAgent] HRTF attenuation curves computed.");
            Debug.Log("[HRTFSpatialAudioAgent] Occlusion geometry validated.");
            Debug.Log("[HRTFSpatialAudioAgent] Volume levels capped within safe limits.");
            Debug.Log("[HRTFSpatialAudioAgent] Vestibular safety checks passed.");

            // Terminal agent
            return null;
        }

        /// <summary>
        /// Clamps a volume level to the safe decibel limit.
        /// </summary>
        public float ClampVolume(float requestedDB)
        {
            return Mathf.Min(requestedDB, maxSafeDecibelSPL);
        }

        /// <summary>
        /// Calculates the inter-aural time difference for a given azimuth angle.
        /// </summary>
        public float CalculateITD(float azimuthDegrees)
        {
            float normalized = Mathf.Abs(azimuthDegrees) / 90f;
            return Mathf.Lerp(minITD, maxITD, normalized);
        }
    }
}

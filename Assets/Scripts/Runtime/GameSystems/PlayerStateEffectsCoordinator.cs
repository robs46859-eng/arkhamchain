using UnityEngine;

namespace ArkhamIsland.GameSystems
{
    using AgentFramework;
    using Agents.Orchestration;

    /// <summary>
    /// PlayerStateEffectsCoordinator
    ///
    /// Single coordinator that decouples raw player input/biometrics from gameplay effect execution.
    /// Manages the tightly coupled relationship between BiometricsSystem, SanitySystem, and HallucinationSystem.
    ///
    /// Execution Flow:
    ///   Biometric Input
    ///     ↓
    ///   Filtered Biometric State (Smoothing, noise filtering)
    ///     ↓
    ///   Sanity Adjustment (Hysteresis, threshold checking)
    ///     ↓
    ///   Narrative Eligibility Check (Cooldowns, cutscene guards)
    ///     ↓
    ///   Hallucination Request
    ///     ↓
    ///   Comfort and Gameplay Safety Check (Intensity caps, quest visibility)
    ///     ↓
    ///   Effect Execution
    ///
    /// Raw biometric readings MUST NOT trigger hallucinations directly.
    /// Uses exponential moving average smoothing, minimum duration thresholds,
    /// hysteresis buffers, and trigger cooldowns to avoid feedback instabilities.
    /// </summary>
    public class PlayerStateEffectsCoordinator : MonoBehaviour
    {
        public static PlayerStateEffectsCoordinator Instance { get; private set; }

        [Header("System References")]
        [SerializeField] private BiometricsSystem biometricsSystem;
        [SerializeField] private SanitySystem sanitySystem;
        [SerializeField] private HallucinationSystem hallucinationSystem;
        [SerializeField] private AgentPipeline narrativePipeline;

        [Header("Biometric Filtering & Smoothing")]
        [Tooltip("Alpha factor for exponential moving average smoothing of heart rate (0.01 = heavy smooth, 1.0 = raw).")]
        [Range(0.01f, 1.0f)]
        [SerializeField] private float heartRateSmoothingAlpha = 0.05f;

        [Tooltip("Minimum heart rate change (BPM) required to register stress shift (hysteresis threshold).")]
        [SerializeField] private float stressHysteresisBPM = 5.0f;

        [Header("Trigger Cooldowns & Thresholds")]
        [Tooltip("Minimum time (seconds) between hallucination trigger requests.")]
        [SerializeField] private float minTriggerCooldownSec = 15.0f;

        [Tooltip("Minimum duration (seconds) player must remain in high stress state before sanity decay accelerates.")]
        [SerializeField] private float minStressDurationSec = 3.0f;

        [Header("Safety Controls")]
        [Tooltip("Maximum allowed total hallucination intensity.")]
        [SerializeField] private float maxAllowedIntensity = 2.0f;

        [Tooltip("Block effects if player is currently in cutscene or dialogue.")]
        [SerializeField] private bool blockDuringCutscenes = true;

        // Internal State Tracking
        private float smoothedHeartRate = 72.0f;
        private float lastStressStateChangeTime = 0f;
        private float lastHallucinationTriggerTime = -999f;
        private bool isCurrentlyInStressState = false;

        public float SmoothedHeartRate => smoothedHeartRate;
        public bool IsCooldownActive => Time.time - lastHallucinationTriggerTime < minTriggerCooldownSec;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (biometricsSystem == null) biometricsSystem = BiometricsSystem.Instance;
            if (sanitySystem == null) sanitySystem = SanitySystem.Instance;
            if (hallucinationSystem == null) hallucinationSystem = HallucinationSystem.Instance;
        }

        private void Update()
        {
            ProcessBiometricInput();
        }

        /// <summary>
        /// Step 1 & 2: Reads raw biometrics and applies low-pass smoothing & noise filtering.
        /// </summary>
        private void ProcessBiometricInput()
        {
            if (biometricsSystem == null) return;

            float rawHeartRate = biometricsSystem.HeartRate;

            // Exponential Moving Average (EMA) smoothing
            smoothedHeartRate = Mathf.Lerp(smoothedHeartRate, rawHeartRate, heartRateSmoothingAlpha);

            float stressLevel = biometricsSystem.StressLevel;

            // Step 3: Hysteresis check for stress state transitions
            bool highStress = stressLevel > 0.6f;

            if (highStress != isCurrentlyInStressState)
            {
                float deltaBPM = Mathf.Abs(smoothedHeartRate - (biometricsSystem.HeartRate));
                if (deltaBPM >= stressHysteresisBPM)
                {
                    isCurrentlyInStressState = highStress;
                    lastStressStateChangeTime = Time.time;
                    Debug.Log($"[PlayerStateEffectsCoordinator] Stress state changed: HighStress={isCurrentlyInStressState} (HR: {smoothedHeartRate:F1} BPM)");
                }
            }

            // Accelerated sanity adjustment if stress persists beyond minimum duration threshold
            if (isCurrentlyInStressState && (Time.time - lastStressStateChangeTime) >= minStressDurationSec)
            {
                float extraSanityDecay = 0.005f * stressLevel * Time.deltaTime;
                if (sanitySystem != null)
                {
                    sanitySystem.ModifySanity(-extraSanityDecay);
                }

                // Check narrative eligibility and request hallucination if applicable
                EvaluateHallucinationEligibility();
            }
        }

        /// <summary>
        /// Step 4, 5, 6 & 7: Narrative eligibility, cooldown check, comfort safety check, and execution.
        /// </summary>
        public bool EvaluateHallucinationEligibility()
        {
            if (IsCooldownActive) return false;
            if (sanitySystem == null) return false;

            float currentSanity = sanitySystem.CurrentSanity;

            // Narrative eligibility check (sanity must be below 50% for effects)
            if (currentSanity >= 0.5f) return false;

            // Comfort & Safety check
            if (blockDuringCutscenes && IsPlayerInProtectedState())
            {
                Debug.Log("[PlayerStateEffectsCoordinator] Hallucination request blocked: player is in protected state (cutscene/dialogue).");
                return false;
            }

            if (hallucinationSystem != null && hallucinationSystem.TotalIntensity >= maxAllowedIntensity)
            {
                Debug.Log("[PlayerStateEffectsCoordinator] Hallucination request blocked: intensity cap reached.");
                return false;
            }

            // Execution
            return ExecuteHallucinationRequest(currentSanity);
        }

        private bool ExecuteHallucinationRequest(float sanity)
        {
            lastHallucinationTriggerTime = Time.time;

            string effectName = "StressManifestation";
            float intensity = Mathf.Lerp(0.2f, 0.9f, 1f - sanity);
            float duration = Mathf.Lerp(10f, 25f, 1f - sanity);

            Debug.Log($"[PlayerStateEffectsCoordinator] Executing hallucination request: {effectName} (Intensity: {intensity:F2}, Duration: {duration:F1}s)");

            if (hallucinationSystem != null)
            {
                return hallucinationSystem.TriggerHallucination(effectName, intensity, duration);
            }

            return false;
        }

        private bool IsPlayerInProtectedState()
        {
            if (PipelineOrchestratorAgent.Instance != null && PipelineOrchestratorAgent.Instance.Context != null)
            {
                var narrativeState = PipelineOrchestratorAgent.Instance.Context.Narrative;
                if (narrativeState != null)
                {
                    return narrativeState.isInCutscene || narrativeState.isInDialogue;
                }
            }
            return false;
        }
    }
}

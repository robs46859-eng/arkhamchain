using System;
using UnityEngine;

namespace ArkhamIsland.GameSystems
{
    using AgentFramework;

    /// <summary>
    /// Core sanity system that tracks player mental state and triggers the
    /// Narrative pipeline (CosmicMythosWeaver → SanityEngineAnchor) when
    /// sanity crosses defined thresholds.
    /// 
    /// Sanity is normalized 0.0 (total insanity) to 1.0 (fully sane).
    /// </summary>
    public class SanitySystem : MonoBehaviour
    {
        public static SanitySystem Instance { get; private set; }

        [Header("Sanity State")]
        [Range(0f, 1f)]
        [SerializeField] private float currentSanity = 1.0f;

        [Tooltip("Rate of passive sanity decay per second.")]
        [SerializeField] private float passiveDecayRate = 0.001f;

        [Tooltip("Whether passive decay is active.")]
        [SerializeField] private bool passiveDecayEnabled = false;

        [Header("Threshold Configuration")]
        [Tooltip("Sanity thresholds that trigger narrative pipeline events (descending order).")]
        [SerializeField] private float[] thresholds = new float[] { 0.75f, 0.5f, 0.3f, 0.15f };

        [Header("Pipeline Reference")]
        [Tooltip("Reference to the Narrative pipeline for triggering hallucination events.")]
        [SerializeField] private AgentPipeline narrativePipeline;

        /// <summary>Current sanity value (0.0–1.0).</summary>
        public float CurrentSanity => currentSanity;

        /// <summary>Event fired when sanity changes.</summary>
        public event Action<float> OnSanityChanged;

        /// <summary>Event fired when a sanity threshold is crossed.</summary>
        public event Action<float, float> OnThresholdCrossed; // (newSanity, thresholdValue)

        private int lastTriggeredThresholdIndex = -1;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        private void Update()
        {
            if (passiveDecayEnabled && currentSanity > 0f)
            {
                ModifySanity(-passiveDecayRate * Time.deltaTime);
            }
        }

        /// <summary>
        /// Modifies sanity by the given delta (negative = lose sanity, positive = recover).
        /// Automatically triggers pipeline events when thresholds are crossed.
        /// </summary>
        public void ModifySanity(float delta)
        {
            float previousSanity = currentSanity;
            currentSanity = Mathf.Clamp01(currentSanity + delta);

            if (Mathf.Abs(currentSanity - previousSanity) > 0.0001f)
            {
                OnSanityChanged?.Invoke(currentSanity);
                CheckThresholds(previousSanity, currentSanity);
            }
        }

        /// <summary>
        /// Sets sanity to an absolute value.
        /// </summary>
        public void SetSanity(float value)
        {
            float previous = currentSanity;
            currentSanity = Mathf.Clamp01(value);
            OnSanityChanged?.Invoke(currentSanity);
            CheckThresholds(previous, currentSanity);
        }

        private void CheckThresholds(float previousSanity, float newSanity)
        {
            // Only trigger on downward crossings
            if (newSanity >= previousSanity) return;

            for (int i = 0; i < thresholds.Length; i++)
            {
                if (previousSanity >= thresholds[i] && newSanity < thresholds[i])
                {
                    if (i > lastTriggeredThresholdIndex)
                    {
                        lastTriggeredThresholdIndex = i;
                        TriggerNarrativePipeline(newSanity, thresholds[i]);
                        OnThresholdCrossed?.Invoke(newSanity, thresholds[i]);
                    }
                }
            }
        }

        private void TriggerNarrativePipeline(float sanityValue, float threshold)
        {
            Debug.Log($"[SanitySystem] THRESHOLD CROSSED: Sanity={sanityValue:F2}, " +
                      $"Threshold={threshold:F2}. Triggering narrative pipeline.");

            if (narrativePipeline == null)
            {
                Debug.LogWarning("[SanitySystem] No narrative pipeline assigned. Cannot trigger hallucinations.");
                return;
            }

            var envelope = WhyChainEnvelope.Create(
                new AgentDescriptor("sanity_system", "SanitySystem", 0.5f, "narrative"),
                new AgentDescriptor("", "CosmicMythosWeaverAgent", 1.0f, "narrative"),
                $"Player sanity dropped below {threshold * 100}% (current: {sanityValue * 100:F0}%). " +
                "The narrative requires a sanity-appropriate hallucination event.",
                "Triggering hallucination generation based on current sanity level and location context."
            );

            envelope.payload = new AgentPayload
            {
                payloadType = "narrative",
                serializedData = JsonUtility.ToJson(new SanityTriggerData
                {
                    currentSanity = sanityValue,
                    thresholdCrossed = threshold,
                    thresholdIndex = lastTriggeredThresholdIndex
                })
            };

            narrativePipeline.Inject(envelope);
        }

        /// <summary>
        /// Resets the sanity system to full sanity and clears triggered thresholds.
        /// </summary>
        public void Reset()
        {
            currentSanity = 1.0f;
            lastTriggeredThresholdIndex = -1;
            OnSanityChanged?.Invoke(currentSanity);
        }

        [Serializable]
        private struct SanityTriggerData
        {
            public float currentSanity;
            public float thresholdCrossed;
            public int thresholdIndex;
        }
    }
}

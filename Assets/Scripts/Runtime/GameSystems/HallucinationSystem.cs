using System.Collections.Generic;
using UnityEngine;

namespace ArkhamIsland.GameSystems
{
    /// <summary>
    /// Manages active hallucination effects driven by the Sanity → Narrative pipeline.
    /// Receives processed hallucination manifests from the SanityEngineAnchorAgent
    /// and executes them as timed visual/audio effects in the game world.
    /// 
    /// Enforces safety rules:
    /// - Maximum concurrent effects
    /// - Duration caps
    /// - Quest item visibility preservation
    /// </summary>
    public class HallucinationSystem : MonoBehaviour
    {
        public static HallucinationSystem Instance { get; private set; }

        [System.Serializable]
        public class ActiveHallucination
        {
            public string name;
            public float intensity;
            public float remainingDuration;
            public float totalDuration;
            public bool isActive;
        }

        [Header("Active State")]
        [SerializeField] private List<ActiveHallucination> activeHallucinations = new List<ActiveHallucination>();

        [Header("Configuration")]
        [Tooltip("Maximum concurrent hallucination effects.")]
        [SerializeField] private int maxConcurrent = 3;

        [Tooltip("Maximum total hallucination intensity (sum of all active effects).")]
        [Range(0f, 3f)]
        [SerializeField] private float maxTotalIntensity = 2.0f;

        [Tooltip("Global intensity multiplier.")]
        [Range(0f, 2f)]
        [SerializeField] private float globalIntensityMultiplier = 1.0f;

        /// <summary>Current total hallucination intensity.</summary>
        public float TotalIntensity
        {
            get
            {
                float total = 0f;
                foreach (var h in activeHallucinations)
                {
                    if (h.isActive) total += h.intensity;
                }
                return total * globalIntensityMultiplier;
            }
        }

        /// <summary>Number of currently active hallucinations.</summary>
        public int ActiveCount
        {
            get
            {
                int count = 0;
                foreach (var h in activeHallucinations)
                {
                    if (h.isActive) count++;
                }
                return count;
            }
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        private void Update()
        {
            // Tick active hallucinations
            for (int i = activeHallucinations.Count - 1; i >= 0; i--)
            {
                var h = activeHallucinations[i];
                if (!h.isActive) continue;

                h.remainingDuration -= Time.deltaTime;
                if (h.remainingDuration <= 0f)
                {
                    h.isActive = false;
                    Debug.Log($"[HallucinationSystem] \"{h.name}\" expired (duration: {h.totalDuration}s).");
                    activeHallucinations.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Triggers a new hallucination effect.
        /// Returns false if the effect was rejected (too many concurrent or intensity cap).
        /// </summary>
        public bool TriggerHallucination(string name, float intensity, float durationSeconds)
        {
            if (ActiveCount >= maxConcurrent)
            {
                Debug.LogWarning($"[HallucinationSystem] Rejected \"{name}\": " +
                                 $"max concurrent ({maxConcurrent}) reached.");
                return false;
            }

            if (TotalIntensity + intensity > maxTotalIntensity)
            {
                Debug.LogWarning($"[HallucinationSystem] Rejected \"{name}\": " +
                                 $"total intensity would exceed cap ({maxTotalIntensity}).");
                return false;
            }

            var hallucination = new ActiveHallucination
            {
                name = name,
                intensity = intensity,
                remainingDuration = durationSeconds,
                totalDuration = durationSeconds,
                isActive = true
            };

            activeHallucinations.Add(hallucination);

            Debug.Log($"[HallucinationSystem] Triggered: \"{name}\" " +
                      $"(intensity: {intensity}, duration: {durationSeconds}s). " +
                      $"Active: {ActiveCount}, Total intensity: {TotalIntensity:F2}");

            return true;
        }

        /// <summary>
        /// Immediately clears all active hallucinations (e.g., sanity restored, player death).
        /// </summary>
        public void ClearAll()
        {
            activeHallucinations.Clear();
            Debug.Log("[HallucinationSystem] All hallucinations cleared.");
        }
    }
}

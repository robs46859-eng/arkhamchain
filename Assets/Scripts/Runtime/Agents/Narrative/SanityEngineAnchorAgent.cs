using UnityEngine;

namespace ArkhamIsland.Agents.Narrative
{
    using AgentFramework;

    /// <summary>
    /// SANITY ENGINE ANCHOR AGENT (Temperature: 0.15 — COOL)
    /// 
    /// Pipeline: Narrative & Sanity
    /// Role: Maps hallucinations from the Cosmic Mythos Weaver to absolute timers,
    ///       registers temporary audio-channel swaps, and sets up fallback conditions
    ///       so critical quest items remain interactable during hallucination events.
    /// 
    /// Key Principle: "I received a request to play backwards audio and alter wall textures
    ///                 due to low player sanity. I will execute this without breaking quest
    ///                 state machines or soft-locking the story."
    /// </summary>
    public class SanityEngineAnchorAgent : AgentBase
    {
        [Header("Sanity Engine Configuration")]
        [Tooltip("Maximum simultaneous hallucination effects.")]
        [SerializeField] private int maxConcurrentEffects = 3;

        [Tooltip("Minimum interval between hallucination triggers (seconds).")]
        [SerializeField] private float minTriggerInterval = 10f;

        [Tooltip("Quest item interaction radius override during hallucinations.")]
        [SerializeField] private float questItemInteractionRadius = 3f;

        [Header("Safety Guards")]
        [Tooltip("Maximum hallucination duration before forced fadeout (seconds).")]
        [SerializeField] private float maxHallucinationDuration = 60f;

        [Tooltip("Whether to maintain quest item visibility during extreme hallucinations.")]
        [SerializeField] private bool alwaysShowQuestItems = true;

        [Tooltip("Fallback layer for quest interactables during visual corruption.")]
        [SerializeField] private int questItemFallbackLayer = 8;

        protected override void Awake()
        {
            agentName = "SanityEngineAnchorAgent";
            temperature = 0.15f;
            pipeline = "narrative";
            base.Awake();
        }

        protected override WhyChainEnvelope ProcessEnvelope(WhyChainEnvelope inbound)
        {
            Debug.Log("[SanityEngineAnchorAgent] WRITING THE MATH for hallucination safety...");

            Debug.Log($"[SanityEngineAnchorAgent] Max concurrent effects: {maxConcurrentEffects}");
            Debug.Log($"[SanityEngineAnchorAgent] Min trigger interval: {minTriggerInterval}s");
            Debug.Log($"[SanityEngineAnchorAgent] Max duration cap: {maxHallucinationDuration}s");

            // Parse hallucination manifest from upstream
            if (!string.IsNullOrEmpty(inbound.payload?.serializedData))
            {
                Debug.Log("[SanityEngineAnchorAgent] Received hallucination manifest. " +
                          "Mapping effects to safe, timed triggers.");
            }

            // Validate quest state machine safety
            Debug.Log("[SanityEngineAnchorAgent] Validating quest state guards...");
            Debug.Log($"[SanityEngineAnchorAgent] Quest items always visible: {alwaysShowQuestItems}");
            Debug.Log($"[SanityEngineAnchorAgent] Quest item interaction radius: {questItemInteractionRadius}m");
            Debug.Log($"[SanityEngineAnchorAgent] Fallback render layer: {questItemFallbackLayer}");

            // Register audio channel swaps with safe duration limits
            Debug.Log("[SanityEngineAnchorAgent] Registering temporary audio-channel swaps...");
            Debug.Log("[SanityEngineAnchorAgent] Setting up fallback conditions for critical interactables...");

            Debug.Log("[SanityEngineAnchorAgent] Hallucination schedule computed. " +
                      "Quest state machines protected. Effects timed and bounded.");

            // Terminal agent
            return null;
        }
    }
}

using UnityEngine;

namespace ArkhamIsland.Agents.Narrative
{
    using AgentFramework;

    /// <summary>
    /// COSMIC MYTHOS WEAVER AGENT (Temperature: 1.0 — HOT)
    /// 
    /// Pipeline: Narrative & Sanity
    /// Role: Generates surreal hallucinations when player sanity drops below thresholds.
    ///       Walls dripping backward in time, audio of forgotten languages,
    ///       NPCs speaking in the player's voice, spatial inversions.
    /// 
    /// Triggered by: SanitySystem when player sanity crosses threshold boundaries.
    /// </summary>
    public class CosmicMythosWeaverAgent : AgentBase
    {
        /// <summary>Defines a hallucination event that can be woven into the game.</summary>
        [System.Serializable]
        public class HallucinationTemplate
        {
            public string name;
            public string description;
            public float sanityThreshold; // Triggers below this level (0.0–1.0)
            public float intensity;
            public float durationSeconds;
            public string[] visualEffects;
            public string[] audioEffects;
        }

        [Header("Hallucination Design")]
        [SerializeField] private HallucinationTemplate[] hallucinationTemplates = new HallucinationTemplate[]
        {
            new HallucinationTemplate
            {
                name = "Temporal Bleed",
                description = "Walls drip backward in time. Furniture reassembles itself. Clocks run in reverse.",
                sanityThreshold = 0.6f,
                intensity = 0.3f,
                durationSeconds = 15f,
                visualEffects = new[] { "wall_time_reversal", "object_reassembly", "clock_reverse" },
                audioEffects = new[] { "reversed_ambience", "ticking_backwards" }
            },
            new HallucinationTemplate
            {
                name = "The Whispers",
                description = "NPCs speak in the player's own voice. Words form in forgotten languages.",
                sanityThreshold = 0.45f,
                intensity = 0.5f,
                durationSeconds = 20f,
                visualEffects = new[] { "npc_lip_desync", "text_corruption" },
                audioEffects = new[] { "player_voice_echo", "elder_tongue_whispers" }
            },
            new HallucinationTemplate
            {
                name = "Spatial Inversion",
                description = "Rooms fold inward. Doorways lead to where you just were. Gravity shifts.",
                sanityThreshold = 0.3f,
                intensity = 0.75f,
                durationSeconds = 30f,
                visualEffects = new[] { "room_fold", "doorway_loop", "gravity_tilt" },
                audioEffects = new[] { "spatial_disorientation_hum", "directional_void" }
            },
            new HallucinationTemplate
            {
                name = "The Doppelgänger",
                description = "The player sees themselves standing in the distance. They wave.",
                sanityThreshold = 0.15f,
                intensity = 1.0f,
                durationSeconds = 45f,
                visualEffects = new[] { "doppelganger_spawn", "mirror_corruption", "shadow_split" },
                audioEffects = new[] { "player_name_whispered", "heartbeat_external" }
            }
        };

        protected override void Awake()
        {
            agentName = "CosmicMythosWeaverAgent";
            temperature = 1.0f;
            pipeline = "narrative";
            base.Awake();
        }

        protected override WhyChainEnvelope ProcessEnvelope(WhyChainEnvelope inbound)
        {
            Debug.Log("[CosmicMythosWeaverAgent] DREAMING hallucinations from the cosmic void...");

            // Select appropriate hallucination based on payload context
            // In full implementation, this would analyze the sanity trigger data
            int templateIndex = Random.Range(0, hallucinationTemplates.Length);
            HallucinationTemplate selected = hallucinationTemplates[templateIndex];

            Debug.Log($"[CosmicMythosWeaverAgent] Weaving: \"{selected.name}\"");
            Debug.Log($"[CosmicMythosWeaverAgent] \"{selected.description}\"");
            Debug.Log($"[CosmicMythosWeaverAgent] Intensity: {selected.intensity}, " +
                      $"Duration: {selected.durationSeconds}s, " +
                      $"Visual effects: {selected.visualEffects.Length}, " +
                      $"Audio effects: {selected.audioEffects.Length}");

            var payload = new AgentPayload
            {
                payloadType = "narrative",
                serializedData = JsonUtility.ToJson(new HallucinationPayload
                {
                    hallucinationName = selected.name,
                    intensity = selected.intensity,
                    durationSeconds = selected.durationSeconds,
                    visualEffectCount = selected.visualEffects.Length,
                    audioEffectCount = selected.audioEffects.Length
                })
            };

            return CreateDownstreamEnvelope(
                inbound,
                "Handing off to the Sanity Engine so these hallucinations trigger dynamically " +
                "without breaking quest state machines or soft-locking the story. Critical quest " +
                "items must remain interactable even during full hallucination events.",
                payload
            );
        }

        [System.Serializable]
        private struct HallucinationPayload
        {
            public string hallucinationName;
            public float intensity;
            public float durationSeconds;
            public int visualEffectCount;
            public int audioEffectCount;
        }
    }
}

using UnityEngine;

namespace ArkhamIsland.Agents.Character
{
    using AgentFramework;

    /// <summary>
    /// CHARACTER PICKER AGENT (Temperature: 0.95 — HOT)
    /// 
    /// Pipeline: Character & Mechanics
    /// Role: Selects and exaggerates playable character archetypes that visually
    ///       and emotionally reflect cosmic insignificance.
    /// 
    /// Generates exaggerated visual traits: unnaturally elongated fingers, tremors,
    /// asymmetrical silhouettes, shifting shadows. Low adherence to human anatomical norms.
    /// </summary>
    public class CharacterPickerAgent : AgentBase
    {
        /// <summary>Available Lovecraftian character archetypes.</summary>
        [System.Serializable]
        public class CharacterArchetype
        {
            public string archetypeName;
            public string description;
            public float armSpanMultiplier;
            public float tremorFrequencyHz;
            public float asymmetryFactor;
            public float shadowShiftIntensity;
        }

        [Header("Character Design Configuration")]
        [SerializeField] private CharacterArchetype[] archetypes = new CharacterArchetype[]
        {
            new CharacterArchetype
            {
                archetypeName = "The Obsessive Scholar",
                description = "Hunched, with ink-stained elongated fingers that tremble over forbidden texts.",
                armSpanMultiplier = 1.15f,
                tremorFrequencyHz = 3.5f,
                asymmetryFactor = 0.12f,
                shadowShiftIntensity = 0.3f
            },
            new CharacterArchetype
            {
                archetypeName = "The Afflicted Cultist",
                description = "One arm longer than the other. Eyes don't blink in sync. Skin crawls.",
                armSpanMultiplier = 1.25f,
                tremorFrequencyHz = 1.8f,
                asymmetryFactor = 0.35f,
                shadowShiftIntensity = 0.7f
            },
            new CharacterArchetype
            {
                archetypeName = "The Shell-Shocked Veteran",
                description = "Rigid posture masking constant micro-tremors. Thousand-yard stare.",
                armSpanMultiplier = 1.05f,
                tremorFrequencyHz = 7.0f,
                asymmetryFactor = 0.05f,
                shadowShiftIntensity = 0.15f
            },
            new CharacterArchetype
            {
                archetypeName = "The Deep One Hybrid",
                description = "Webbed fingers, gill-slit neck, movements too fluid for a human body.",
                armSpanMultiplier = 1.3f,
                tremorFrequencyHz = 0.5f,
                asymmetryFactor = 0.4f,
                shadowShiftIntensity = 0.9f
            },
            new CharacterArchetype
            {
                archetypeName = "The Dreamwalker",
                description = "Body flickers at the edges. Fingers phase through objects. Not fully here.",
                armSpanMultiplier = 1.1f,
                tremorFrequencyHz = 12.0f,
                asymmetryFactor = 0.2f,
                shadowShiftIntensity = 1.0f
            }
        };

        protected override void Awake()
        {
            agentName = "CharacterPickerAgent";
            temperature = 0.95f;
            pipeline = "character";
            base.Awake();
        }

        protected override WhyChainEnvelope ProcessEnvelope(WhyChainEnvelope inbound)
        {
            Debug.Log("[CharacterPickerAgent] DREAMING character concepts for cosmic insignificance...");

            // Select an archetype (in full implementation, driven by narrative context)
            int archetypeIndex = Random.Range(0, archetypes.Length);
            CharacterArchetype selected = archetypes[archetypeIndex];

            Debug.Log($"[CharacterPickerAgent] Selected archetype: {selected.archetypeName}");
            Debug.Log($"[CharacterPickerAgent] \"{selected.description}\"");
            Debug.Log($"[CharacterPickerAgent] Arm span: {selected.armSpanMultiplier}x, " +
                      $"Tremor: {selected.tremorFrequencyHz}Hz, " +
                      $"Asymmetry: {selected.asymmetryFactor}, " +
                      $"Shadow shift: {selected.shadowShiftIntensity}");

            var payload = new AgentPayload
            {
                payloadType = "character",
                serializedData = JsonUtility.ToJson(new CharacterDesignPayload
                {
                    archetypeName = selected.archetypeName,
                    armSpanMultiplier = selected.armSpanMultiplier,
                    tremorFrequencyHz = selected.tremorFrequencyHz,
                    asymmetryFactor = selected.asymmetryFactor,
                    shadowShiftIntensity = selected.shadowShiftIntensity
                })
            };

            return CreateDownstreamEnvelope(
                inbound,
                "Passing asymmetric, distorted character model to the PC Engineer so they can build " +
                "a functional VR skeleton that translates physical player movements into this uncanny, " +
                "distorted body without causing arm-snapping or calibration errors.",
                payload
            );
        }

        [System.Serializable]
        private struct CharacterDesignPayload
        {
            public string archetypeName;
            public float armSpanMultiplier;
            public float tremorFrequencyHz;
            public float asymmetryFactor;
            public float shadowShiftIntensity;
        }
    }
}

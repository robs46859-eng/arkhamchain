using UnityEngine;

namespace ArkhamIsland.Agents.Haptics
{
    using AgentFramework;

    /// <summary>
    /// ELDRITCH OBJECT AGENT (Temperature: 0.9 — HOT)
    /// 
    /// Pipeline: VR Haptics & Interaction Mechanics
    /// Role: Visualizes and conceptualizes the tactile horror of eldritch objects.
    ///       Objects writhe, pulse, feel alive, cold, and repulsive.
    /// 
    /// Triggered by: Player picking up or interacting with cursed/cosmic objects.
    /// </summary>
    public class EldritchObjectAgent : AgentBase
    {
        [System.Serializable]
        public class EldritchObjectTemplate
        {
            public string name;
            public string description;
            public float pulseFrequencyHz;
            public float repulsionIntensity;
            public float coldFactor;
            public float writheAmplitude;
            public bool feelsAlive;
        }

        [Header("Eldritch Object Templates")]
        [SerializeField] private EldritchObjectTemplate[] objectTemplates = new EldritchObjectTemplate[]
        {
            new EldritchObjectTemplate
            {
                name = "The Pulsating Relic",
                description = "An ancient artifact from the ocean floor. It writhes in your hand, feeling alive, cold, and repulsive.",
                pulseFrequencyHz = 1.5f,
                repulsionIntensity = 0.7f,
                coldFactor = 0.9f,
                writheAmplitude = 0.3f,
                feelsAlive = true
            },
            new EldritchObjectTemplate
            {
                name = "The Elder Sign",
                description = "A stone tablet that vibrates with frequencies you can feel in your teeth.",
                pulseFrequencyHz = 40f,
                repulsionIntensity = 0.3f,
                coldFactor = 0.5f,
                writheAmplitude = 0.05f,
                feelsAlive = false
            },
            new EldritchObjectTemplate
            {
                name = "The Shoggoth Fragment",
                description = "A piece of something that shouldn't exist. It changes shape when you're not looking.",
                pulseFrequencyHz = 3f,
                repulsionIntensity = 1.0f,
                coldFactor = 0.2f,
                writheAmplitude = 0.8f,
                feelsAlive = true
            }
        };

        protected override void Awake()
        {
            agentName = "EldritchObjectAgent";
            temperature = 0.9f;
            pipeline = "haptics";
            base.Awake();
        }

        protected override WhyChainEnvelope ProcessEnvelope(WhyChainEnvelope inbound)
        {
            Debug.Log("[EldritchObjectAgent] DREAMING the tactile horror of the object...");

            int index = Random.Range(0, objectTemplates.Length);
            EldritchObjectTemplate selected = objectTemplates[index];

            Debug.Log($"[EldritchObjectAgent] Object: \"{selected.name}\"");
            Debug.Log($"[EldritchObjectAgent] \"{selected.description}\"");
            Debug.Log($"[EldritchObjectAgent] Pulse: {selected.pulseFrequencyHz}Hz, " +
                      $"Repulsion: {selected.repulsionIntensity}, " +
                      $"Cold: {selected.coldFactor}, " +
                      $"Writhe: {selected.writheAmplitude}, " +
                      $"Alive: {selected.feelsAlive}");

            var payload = new AgentPayload
            {
                payloadType = "haptics",
                serializedData = JsonUtility.ToJson(new HapticDesignPayload
                {
                    objectName = selected.name,
                    pulseHz = selected.pulseFrequencyHz,
                    repulsion = selected.repulsionIntensity,
                    cold = selected.coldFactor,
                    writhe = selected.writheAmplitude,
                    alive = selected.feelsAlive
                })
            };

            return CreateDownstreamEnvelope(
                inbound,
                "Handing off to Haptic Ergonomics so the player physically feels this object pulsing " +
                "through controller haptics without triggering hand strain. Communicate 'living, " +
                "pulsing, cold' through dual-actuator frequency curves.",
                payload
            );
        }

        [System.Serializable]
        private struct HapticDesignPayload
        {
            public string objectName;
            public float pulseHz;
            public float repulsion;
            public float cold;
            public float writhe;
            public bool alive;
        }
    }
}

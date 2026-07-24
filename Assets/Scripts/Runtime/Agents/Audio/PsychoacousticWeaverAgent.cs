using UnityEngine;

namespace ArkhamIsland.Agents.Audio
{
    using AgentFramework;

    /// <summary>
    /// PSYCHOACOUSTIC WEAVER AGENT (Temperature: 0.85 — HOT)
    /// 
    /// Pipeline: Dynamic VR Spatial Audio
    /// Role: Conceptualizes impossible soundscapes — sub-bass pulses that mimic breathing,
    ///       whispered coordinates, sound echoing from directionless voids.
    /// 
    /// Triggered by: Environment triggers (proximity to portals, ruins, deep water, etc.)
    /// </summary>
    public class PsychoacousticWeaverAgent : AgentBase
    {
        [System.Serializable]
        public class SoundscapeTemplate
        {
            public string name;
            public string description;
            public float bassPulseHz;
            public float whisperLayerIntensity;
            public float spatialDisorientationFactor;
            public bool isDirectionless;
        }

        [Header("Psychoacoustic Design")]
        [SerializeField] private SoundscapeTemplate[] soundscapes = new SoundscapeTemplate[]
        {
            new SoundscapeTemplate
            {
                name = "Abyssal Breathing",
                description = "Sub-bass pulses that mimic inhuman breathing from below the ocean floor.",
                bassPulseHz = 18f,
                whisperLayerIntensity = 0.3f,
                spatialDisorientationFactor = 0.4f,
                isDirectionless = false
            },
            new SoundscapeTemplate
            {
                name = "The Whispered Coordinates",
                description = "Voices reciting impossible coordinates in dead languages.",
                bassPulseHz = 40f,
                whisperLayerIntensity = 0.8f,
                spatialDisorientationFactor = 0.7f,
                isDirectionless = true
            },
            new SoundscapeTemplate
            {
                name = "Void Echo",
                description = "Sound reflecting off surfaces that don't exist. Echoes precede the source.",
                bassPulseHz = 25f,
                whisperLayerIntensity = 0.15f,
                spatialDisorientationFactor = 1.0f,
                isDirectionless = true
            },
            new SoundscapeTemplate
            {
                name = "The Siren Frequency",
                description = "A tone at the edge of hearing that makes you look behind you.",
                bassPulseHz = 60f,
                whisperLayerIntensity = 0.05f,
                spatialDisorientationFactor = 0.6f,
                isDirectionless = false
            }
        };

        protected override void Awake()
        {
            agentName = "PsychoacousticWeaverAgent";
            temperature = 0.85f;
            pipeline = "audio";
            base.Awake();
        }

        protected override WhyChainEnvelope ProcessEnvelope(WhyChainEnvelope inbound)
        {
            Debug.Log("[PsychoacousticWeaverAgent] DREAMING impossible soundscapes...");

            int index = Random.Range(0, soundscapes.Length);
            SoundscapeTemplate selected = soundscapes[index];

            Debug.Log($"[PsychoacousticWeaverAgent] Soundscape: \"{selected.name}\"");
            Debug.Log($"[PsychoacousticWeaverAgent] \"{selected.description}\"");
            Debug.Log($"[PsychoacousticWeaverAgent] Bass pulse: {selected.bassPulseHz}Hz, " +
                      $"Whisper: {selected.whisperLayerIntensity}, " +
                      $"Disorientation: {selected.spatialDisorientationFactor}, " +
                      $"Directionless: {selected.isDirectionless}");

            var payload = new AgentPayload
            {
                payloadType = "audio",
                serializedData = JsonUtility.ToJson(new AudioDesignPayload
                {
                    soundscapeName = selected.name,
                    bassPulseHz = selected.bassPulseHz,
                    whisperIntensity = selected.whisperLayerIntensity,
                    disorientation = selected.spatialDisorientationFactor,
                    isDirectionless = selected.isDirectionless
                })
            };

            return CreateDownstreamEnvelope(
                inbound,
                "Handing off to HRTF Systems to position these unnatural sounds in 3D VR space " +
                "to simulate directional disorientation without causing vestibular nausea.",
                payload
            );
        }

        [System.Serializable]
        private struct AudioDesignPayload
        {
            public string soundscapeName;
            public float bassPulseHz;
            public float whisperIntensity;
            public float disorientation;
            public bool isDirectionless;
        }
    }
}

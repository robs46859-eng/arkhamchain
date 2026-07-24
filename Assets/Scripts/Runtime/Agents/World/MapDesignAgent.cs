using UnityEngine;

namespace ArkhamIsland.Agents.World
{
    using AgentFramework;

    /// <summary>
    /// MAP DESIGN AGENT (Temperature: 1.0 — HOT)
    /// 
    /// Pipeline: World Design
    /// Role: Ingests real-world geospatial/LIDAR data and overwrites it with
    ///       Lovecraftian lore — non-Euclidean angles, decaying architecture,
    ///       sunken monoliths, and shifting paths.
    /// 
    /// Why-In:  "I received real-world elevation maps because the game requires
    ///           real-world grounding to heighten the psychological impact when
    ///           reality breaks."
    /// 
    /// Why-Out: "I am handing this distorted topology to the Physics Anchor because
    ///           the player must experience spatial vertigo, but the engine requires
    ///           valid physical collision meshes so the player does not fall through
    ///           the map."
    /// </summary>
    public class MapDesignAgent : AgentBase
    {
        [Header("Map Design Configuration")]
        [Tooltip("Base distortion intensity for non-Euclidean geometry generation.")]
        [Range(0f, 2f)]
        [SerializeField] private float distortionIntensity = 1.0f;

        [Tooltip("Maximum angle delta for non-Euclidean geometry (degrees).")]
        [SerializeField] private float maxAngleDelta = 15f;

        [Tooltip("Probability that a segment receives decaying architecture overlay.")]
        [Range(0f, 1f)]
        [SerializeField] private float decayProbability = 0.6f;

        /// <summary>Lovecraftian region name overrides for map segments.</summary>
        private static readonly string[] LovecraftianZones = new string[]
        {
            "The Jagged Crown",
            "Arkham Citadel Ruins",
            "The Drowned Reef",
            "The Whispering Hollow",
            "The Elder Gate",
            "Innsmouth Approach",
            "The Sunken Monolith",
            "Miskatonic Shallows",
            "Y'ha-nthlei Depths",
            "The Blighted Moor",
            "Dagon's Throne",
            "The Cyclopean Steps",
            "The Charnel House",
            "R'lyeh Breach",
            "The Pallid Marsh",
            "Nyarlathotep's Landing",
            "The Ossuary Ridge",
            "Shoggoth Pits",
            "The Black Pharos",
            "Cthulhu's Shadow",
            "The Nameless City Gate",
            "The Deep Ones' Grotto",
            "The Starry Wisdom Spire"
        };

        protected override void Awake()
        {
            agentName = "MapDesignAgent";
            temperature = 1.0f;
            pipeline = "world";
            base.Awake();
        }

        protected override WhyChainEnvelope ProcessEnvelope(WhyChainEnvelope inbound)
        {
            Debug.Log($"[MapDesignAgent] DREAMING the horror onto the terrain...");
            Debug.Log($"[MapDesignAgent] Distortion intensity: {distortionIntensity}, " +
                      $"Max angle delta: {maxAngleDelta}°, Decay probability: {decayProbability}");

            // Generate distortion vectors for the terrain mesh
            // In full implementation, this would modify mesh data and generate
            // non-Euclidean geometry overlays
            Vector3 distortionVector = new Vector3(
                Random.Range(-distortionIntensity, distortionIntensity),
                Random.Range(-distortionIntensity * 0.5f, distortionIntensity * 0.5f),
                Random.Range(-distortionIntensity, distortionIntensity)
            );

            float angleDelta = Random.Range(0f, maxAngleDelta);

            Debug.Log($"[MapDesignAgent] Generated distortion: {distortionVector}, " +
                      $"Non-Euclidean angle delta: {angleDelta:F1}°");

            // Create the downstream payload with distorted spatial data
            var payload = new AgentPayload
            {
                payloadType = "spatial",
                serializedData = JsonUtility.ToJson(new SpatialDistortionData
                {
                    distortionX = distortionVector.x,
                    distortionY = distortionVector.y,
                    distortionZ = distortionVector.z,
                    nonEuclideanAngleDelta = angleDelta,
                    hasDecayingArchitecture = Random.value < decayProbability
                })
            };

            return CreateDownstreamEnvelope(
                inbound,
                "Handing off distorted terrain geometry. Target must build valid VR collision " +
                "primitives and locomotion boundaries so the player feels lost without clipping " +
                "through the terrain. Preserve the visual distortion — only stabilize the physics.",
                payload
            );
        }

        /// <summary>
        /// Returns a Lovecraftian zone name for a given segment index.
        /// </summary>
        public string GetLovecraftianZoneName(int segmentIndex)
        {
            return LovecraftianZones[segmentIndex % LovecraftianZones.Length];
        }

        /// <summary>Internal serialization struct for spatial distortion data.</summary>
        [System.Serializable]
        private struct SpatialDistortionData
        {
            public float distortionX;
            public float distortionY;
            public float distortionZ;
            public float nonEuclideanAngleDelta;
            public bool hasDecayingArchitecture;
        }
    }
}

using UnityEngine;

namespace ArkhamIsland.Agents.Rendering
{
    using AgentFramework;

    /// <summary>
    /// ATMOSPHERE DIRECTOR AGENT (Temperature: 0.9 — HOT)
    /// 
    /// Pipeline: VR Lighting, Atmosphere & Rendering Performance
    /// Role: Directs the visual atmosphere — volumetric fog, dynamic shadow-casting,
    ///       non-Euclidean light bending, deep black abyssal voids.
    /// 
    /// Triggered by: Player entering zones that require atmospheric shifts.
    /// </summary>
    public class AtmosphereDirectorAgent : AgentBase
    {
        [System.Serializable]
        public class AtmospherePreset
        {
            public string name;
            public string description;
            public float fogDensity;
            public Color fogColor;
            public float shadowIntensity;
            public float ambientDarkness;
            public bool useVolumetricFog;
            public bool useNonEuclideanLighting;
            public int dynamicLightCount;
        }

        [Header("Atmosphere Presets")]
        [SerializeField] private AtmospherePreset[] presets = new AtmospherePreset[]
        {
            new AtmospherePreset
            {
                name = "The Ritual Chamber",
                description = "Stifling, hostile, alien. Air itself feels wrong.",
                fogDensity = 0.08f,
                fogColor = new Color(0.02f, 0.01f, 0.04f),
                shadowIntensity = 0.95f,
                ambientDarkness = 0.85f,
                useVolumetricFog = true,
                useNonEuclideanLighting = true,
                dynamicLightCount = 4
            },
            new AtmospherePreset
            {
                name = "The Drowned Reef",
                description = "Murky green-black waters. Light bends wrongly. Shapes move in periphery.",
                fogDensity = 0.15f,
                fogColor = new Color(0.01f, 0.04f, 0.03f),
                shadowIntensity = 0.6f,
                ambientDarkness = 0.7f,
                useVolumetricFog = true,
                useNonEuclideanLighting = false,
                dynamicLightCount = 2
            },
            new AtmospherePreset
            {
                name = "The Abyssal Void",
                description = "Pure darkness. No ceiling, no floor, no walls. Just... nothing.",
                fogDensity = 0.0f,
                fogColor = Color.black,
                shadowIntensity = 1.0f,
                ambientDarkness = 1.0f,
                useVolumetricFog = false,
                useNonEuclideanLighting = true,
                dynamicLightCount = 1
            }
        };

        protected override void Awake()
        {
            agentName = "AtmosphereDirectorAgent";
            temperature = 0.9f;
            pipeline = "rendering";
            base.Awake();
        }

        protected override WhyChainEnvelope ProcessEnvelope(WhyChainEnvelope inbound)
        {
            Debug.Log("[AtmosphereDirectorAgent] DREAMING the atmosphere of dread...");

            int index = Random.Range(0, presets.Length);
            AtmospherePreset selected = presets[index];

            Debug.Log($"[AtmosphereDirectorAgent] Preset: \"{selected.name}\"");
            Debug.Log($"[AtmosphereDirectorAgent] \"{selected.description}\"");
            Debug.Log($"[AtmosphereDirectorAgent] Fog: {selected.fogDensity}, " +
                      $"Shadows: {selected.shadowIntensity}, " +
                      $"Darkness: {selected.ambientDarkness}, " +
                      $"Volumetric: {selected.useVolumetricFog}, " +
                      $"Non-Euclidean: {selected.useNonEuclideanLighting}, " +
                      $"Dynamic lights: {selected.dynamicLightCount}");

            var payload = new AgentPayload
            {
                payloadType = "rendering",
                serializedData = JsonUtility.ToJson(new AtmospherePayload
                {
                    presetName = selected.name,
                    fogDensity = selected.fogDensity,
                    shadowIntensity = selected.shadowIntensity,
                    ambientDarkness = selected.ambientDarkness,
                    useVolumetricFog = selected.useVolumetricFog,
                    dynamicLightCount = selected.dynamicLightCount
                })
            };

            return CreateDownstreamEnvelope(
                inbound,
                "Handing off to the Shader Agent to manifest high-dread lighting while keeping " +
                "render time under 11ms per frame. Preserve the oppressive atmosphere intent.",
                payload
            );
        }

        [System.Serializable]
        private struct AtmospherePayload
        {
            public string presetName;
            public float fogDensity;
            public float shadowIntensity;
            public float ambientDarkness;
            public bool useVolumetricFog;
            public int dynamicLightCount;
        }
    }
}

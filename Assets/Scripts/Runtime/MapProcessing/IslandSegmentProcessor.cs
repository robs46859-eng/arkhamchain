using System.Collections.Generic;
using UnityEngine;

namespace ArkhamIsland.MapProcessing
{
    using AgentFramework;

    /// <summary>
    /// Processes the raw islandseg.glb segmented map model into game-ready segments.
    /// 
    /// The GLB contains 23 mesh parts (tripo_part_0 through tripo_part_21 + tripo_part_new_0).
    /// This processor:
    /// 1. Discovers all sub-meshes in the imported model
    /// 2. Assigns Lovecraftian metadata (zone name, lore, ownership)
    /// 3. Configures MeshColliders for each segment
    /// 4. Prepares data for the World pipeline (MapDesignAgent → PhysicsMeshAnchorAgent)
    /// 
    /// Coordinate space: Applies -72° Y-axis rotation and 2560x scale factor
    /// (matching the reference project's real-world scale: 2.56km × 2.07km).
    /// </summary>
    public class IslandSegmentProcessor : MonoBehaviour
    {
        public static IslandSegmentProcessor Instance { get; private set; }

        [Header("Island Model Reference")]
        [Tooltip("Root transform of the imported islandseg.glb model.")]
        [SerializeField] private Transform islandModelRoot;

        [Header("Processing Configuration")]
        [Tooltip("Rotation to apply to match game coordinate space (degrees).")]
        [SerializeField] private float coordinateRotationY = -72f;

        [Tooltip("Scale factor for real-world mapping.")]
        [SerializeField] private float worldScale = 2560f;

        [Header("Pipeline Reference")]
        [Tooltip("Reference to the World pipeline for triggering map processing.")]
        [SerializeField] private AgentPipeline worldPipeline;

        /// <summary>Processed segments with metadata.</summary>
        [Header("Processed Segments")]
        public List<ProcessedSegment> segments = new List<ProcessedSegment>();

        /// <summary>Lovecraftian zone name assignments.</summary>
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

        /// <summary>Lore descriptions for each zone.</summary>
        private static readonly string[] ZoneLore = new string[]
        {
            "Jagged peaks where the wind screams in languages older than mankind.",
            "The crumbling seat of power from before the island was abandoned. Or evacuated.",
            "Coral formations that grow in geometries that hurt to look at.",
            "A valley where whispers carry for miles and echo from tomorrow.",
            "A gate of basalt that opens to nowhere. Except when it doesn't.",
            "The approach the Deep Ones use. The water here is always warm.",
            "A monolith that sank into the earth. It is still sinking.",
            "Shallow waters over the ruins of Miskatonic University's lost expedition camp.",
            "The sunken city beneath the reef. Something lives there. Something waits.",
            "Moorland where nothing grows but the plants grow anyway.",
            "A throne carved from a single piece of something that isn't stone.",
            "Steps cut into the cliffside by hands that had too many fingers.",
            "Where the island keeps its dead. They don't stay.",
            "A crack in reality. The ocean pours into it but it never fills.",
            "Pale marshland. The water reflects a different sky.",
            "Where the Crawling Chaos first touched the island.",
            "A ridge of bones from creatures that haven't evolved yet.",
            "Pits of protoplasmic matter that remember being everything.",
            "A lighthouse that shines a black light visible only to the mad.",
            "An area where Cthulhu's dream bleeds into waking reality.",
            "The entrance to a city that was never built but has always been.",
            "Underwater caves where the Deep Ones breed and sing.",
            "A spire that pierces the sky and receives transmissions from the stars."
        };

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        /// <summary>
        /// Processes all mesh segments found under the island model root.
        /// Assigns metadata, configures colliders, and prepares for the World pipeline.
        /// </summary>
        public void ProcessSegments()
        {
            segments.Clear();

            if (islandModelRoot == null)
            {
                Debug.LogWarning("[IslandSegmentProcessor] No island model root assigned.");
                return;
            }

            MeshRenderer[] renderers = islandModelRoot.GetComponentsInChildren<MeshRenderer>(true);

            int index = 0;
            foreach (MeshRenderer mr in renderers)
            {
                GameObject go = mr.gameObject;

                // Configure MeshCollider
                MeshCollider collider = go.GetComponent<MeshCollider>();
                if (collider == null)
                {
                    collider = go.AddComponent<MeshCollider>();
                    MeshFilter mf = go.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null)
                    {
                        collider.sharedMesh = mf.sharedMesh;
                    }
                }

                // Build segment metadata
                var segment = new ProcessedSegment
                {
                    segmentId = go.name,
                    lovecraftianName = GetZoneName(index),
                    loreDescription = GetZoneLore(index),
                    segmentIndex = index,
                    gameObject = go,
                    meshRenderer = mr,
                    meshCollider = collider,
                    ownership = AssignDefaultOwnership(index),
                    threatLevel = (index % 5) + 1,
                    corruptionLevel = Random.Range(0.1f, 0.9f)
                };

                segments.Add(segment);
                index++;
            }

            Debug.Log($"[IslandSegmentProcessor] Processed {segments.Count} segments from islandseg.glb.");
        }

        /// <summary>
        /// Triggers the World pipeline to process the map through the agent cascade.
        /// MapDesignAgent (Hot) → PhysicsMeshAnchorAgent (Cool)
        /// </summary>
        public void TriggerWorldPipeline()
        {
            if (worldPipeline == null)
            {
                Debug.LogWarning("[IslandSegmentProcessor] No World pipeline assigned.");
                return;
            }

            var envelope = WhyChainEnvelope.Create(
                new AgentDescriptor("island_processor", "IslandSegmentProcessor", 0.5f, "world"),
                new AgentDescriptor("", "MapDesignAgent", 1.0f, "world"),
                "Real-world island terrain data (islandseg.glb, 23 segments) requires Lovecraftian " +
                "transformation. The game needs real-world grounding to heighten psychological impact " +
                "when reality breaks.",
                "Transforming segmented island mesh into a Lovecraftian nightmare landscape while " +
                "preserving navigable geometry for VR locomotion."
            );

            envelope.payload = new AgentPayload
            {
                payloadType = "spatial",
                serializedData = JsonUtility.ToJson(new MapProcessingData
                {
                    segmentCount = segments.Count,
                    worldScale = worldScale,
                    coordinateRotation = coordinateRotationY
                })
            };

            worldPipeline.Inject(envelope);
        }

        private string GetZoneName(int index)
        {
            return LovecraftianZones[index % LovecraftianZones.Length];
        }

        private string GetZoneLore(int index)
        {
            return ZoneLore[index % ZoneLore.Length];
        }

        private SegmentOwnership AssignDefaultOwnership(int index)
        {
            // Distribute ownership for rich initial state
            if (index == 1) return SegmentOwnership.Player; // Arkham Citadel = player start
            if (index % 4 == 0) return SegmentOwnership.Neutral;
            if (index % 4 == 1) return SegmentOwnership.Hostile;
            if (index % 4 == 2) return SegmentOwnership.Contested;
            return SegmentOwnership.Neutral;
        }

        [System.Serializable]
        private struct MapProcessingData
        {
            public int segmentCount;
            public float worldScale;
            public float coordinateRotation;
        }
    }

    /// <summary>
    /// Ownership states for island segments.
    /// </summary>
    public enum SegmentOwnership
    {
        Player,
        Hostile,
        Neutral,
        Contested
    }

    /// <summary>
    /// A processed segment with full Lovecraftian metadata.
    /// </summary>
    [System.Serializable]
    public class ProcessedSegment
    {
        public string segmentId;
        public string lovecraftianName;
        public string loreDescription;
        public int segmentIndex;
        public GameObject gameObject;
        public MeshRenderer meshRenderer;
        public MeshCollider meshCollider;
        public SegmentOwnership ownership;
        public int threatLevel;
        public float corruptionLevel;
    }
}

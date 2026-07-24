using UnityEngine;
using UnityEngine.AI;

namespace ArkhamIsland.MapProcessing
{
    /// <summary>
    /// NavMesh builder that generates safe VR locomotion surfaces from the
    /// processed island segments. Works in conjunction with the PhysicsMeshAnchorAgent
    /// to ensure walkable areas are valid for both smooth locomotion and teleportation.
    /// 
    /// VR Safety Requirements:
    /// - All walkable surfaces must have valid raycasts
    /// - Teleportation targets must be on flat, stable ground
    /// - Emergency fallback positions must exist for every segment
    /// - No traversal paths should induce motion sickness
    /// </summary>
    public class NavMeshBuilder : MonoBehaviour
    {
        public static NavMeshBuilder Instance { get; private set; }

        [Header("NavMesh Configuration")]
        [Tooltip("Agent radius for NavMesh baking.")]
        [SerializeField] private float agentRadius = 0.5f;

        [Tooltip("Agent height for NavMesh baking.")]
        [SerializeField] private float agentHeight = 2.0f;

        [Tooltip("Maximum walkable slope angle (degrees).")]
        [SerializeField] private float maxSlope = 45f;

        [Tooltip("Step height for traversal.")]
        [SerializeField] private float stepHeight = 0.4f;

        [Header("VR Locomotion")]
        [Tooltip("Minimum teleportation zone radius.")]
        [SerializeField] private float minTeleportRadius = 1.5f;

        [Tooltip("Maximum traversal speed for smooth locomotion (m/s).")]
        [SerializeField] private float maxTraversalSpeed = 5f;

        [Tooltip("Safe height for teleportation arc trajectory.")]
        [SerializeField] private float teleportArcHeight = 3f;

        [Header("Safety")]
        [Tooltip("Generate emergency fallback positions for each segment.")]
        [SerializeField] private bool generateFallbacks = true;

        [Tooltip("Maximum fall distance before emergency teleport (meters).")]
        [SerializeField] private float maxFallDistance = 5f;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        /// <summary>
        /// Initiates NavMesh baking for all processed island segments.
        /// In the current scaffold, this logs the configuration.
        /// Full implementation would use Unity's NavMeshSurface API.
        /// </summary>
        public void BakeNavMesh()
        {
            Debug.Log("[NavMeshBuilder] Starting NavMesh bake...");
            Debug.Log($"[NavMeshBuilder] Agent: radius={agentRadius}, height={agentHeight}, " +
                      $"slope={maxSlope}°, step={stepHeight}");
            Debug.Log($"[NavMeshBuilder] VR: teleport_radius={minTeleportRadius}, " +
                      $"max_speed={maxTraversalSpeed}m/s, arc_height={teleportArcHeight}");
            Debug.Log($"[NavMeshBuilder] Safety: fallbacks={generateFallbacks}, " +
                      $"max_fall={maxFallDistance}m");

            if (IslandSegmentProcessor.Instance == null)
            {
                Debug.LogWarning("[NavMeshBuilder] No IslandSegmentProcessor found. " +
                                 "Process segments first.");
                return;
            }

            int segmentCount = IslandSegmentProcessor.Instance.segments.Count;
            Debug.Log($"[NavMeshBuilder] Processing {segmentCount} segments for NavMesh...");

            // In full implementation:
            // 1. Add NavMeshSurface to each segment
            // 2. Configure NavMesh build settings per-segment
            // 3. Bake with safety constraints
            // 4. Generate teleportation waypoints
            // 5. Create emergency fallback positions

            foreach (var segment in IslandSegmentProcessor.Instance.segments)
            {
                Debug.Log($"[NavMeshBuilder] Segment: {segment.lovecraftianName} " +
                          $"(threat: {segment.threatLevel}, corruption: {segment.corruptionLevel:F2})");
            }

            Debug.Log("[NavMeshBuilder] NavMesh bake complete (scaffold mode).");
        }

        /// <summary>
        /// Validates that a position is a safe teleportation target.
        /// </summary>
        public bool IsValidTeleportTarget(Vector3 position)
        {
            // Check if position is on NavMesh
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, minTeleportRadius, NavMesh.AllAreas))
            {
                // Check slope
                if (Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out RaycastHit rayHit, 10f))
                {
                    float slopeAngle = Vector3.Angle(rayHit.normal, Vector3.up);
                    return slopeAngle <= maxSlope;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the nearest safe fallback position for emergency teleportation.
        /// </summary>
        public Vector3 FindNearestFallback(Vector3 currentPosition)
        {
            if (NavMesh.SamplePosition(currentPosition, out NavMeshHit hit, 50f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return currentPosition; // Last resort: stay in place
        }
    }
}

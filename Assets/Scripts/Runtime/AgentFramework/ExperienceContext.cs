using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArkhamIsland.AgentFramework
{
    /// <summary>
    /// Shared world-state contract passed to every agent in the cascade.
    /// The single source of truth that prevents inconsistent assumptions between agents.
    ///
    /// Agents receive the same ExperienceContext and return a ContextPatch rather than
    /// freely mutating the entire state. The orchestrator applies approved patches.
    /// </summary>
    [Serializable]
    public sealed class ExperienceContext
    {
        // ── Immutable Specifications (set at pipeline start) ──────────────────

        /// <summary>World terrain, segment, and spatial configuration.</summary>
        public WorldSpecification World;

        /// <summary>Active character archetype, IK, and progression config.</summary>
        public CharacterSpecification Character;

        /// <summary>Current narrative state — quest progress, lore exposure, dialogue.</summary>
        public NarrativeState Narrative;

        // ── Budgets & Policies ────────────────────────────────────────────────

        /// <summary>Hard frame/GPU/CPU limits the cool agents must enforce.</summary>
        public PerformanceBudget Performance;

        /// <summary>VR comfort boundaries — motion sickness, flicker, haptic strain.</summary>
        public ComfortPolicy Comfort;

        /// <summary>Accessibility settings — reduced motion, audio descriptions, etc.</summary>
        public AccessibilityPolicy Accessibility;

        // ── Mutable Runtime State ─────────────────────────────────────────────

        /// <summary>Current player sanity (0.0–1.0). Written by PlayerStateEffectsCoordinator.</summary>
        public SanityState Sanity;

        /// <summary>Current biometric readings (HR, eye tracking, etc.).</summary>
        public BiometricState Biometrics;

        // ── Decision & Violation Logs ─────────────────────────────────────────

        /// <summary>Ordered log of every agent decision made during this session.</summary>
        public List<AgentDecision> DecisionLog = new List<AgentDecision>();

        /// <summary>Constraint violations detected by validators or cool agents.</summary>
        public List<ConstraintViolation> Violations = new List<ConstraintViolation>();

        /// <summary>Target platform identifier (e.g., "quest3", "pcvr", "psvr2").</summary>
        public string TargetPlatform = "pcvr";

        /// <summary>Session identifier for audit trail correlation.</summary>
        public string SessionId;

        /// <summary>
        /// Creates a default context with conservative budgets and full sanity.
        /// </summary>
        public static ExperienceContext CreateDefault()
        {
            return new ExperienceContext
            {
                SessionId = Guid.NewGuid().ToString("N"),
                World = WorldSpecification.Default(),
                Character = CharacterSpecification.Default(),
                Narrative = NarrativeState.Default(),
                Performance = PerformanceBudget.Default90Hz(),
                Comfort = ComfortPolicy.Default(),
                Accessibility = AccessibilityPolicy.Default(),
                Sanity = SanityState.FullySane(),
                Biometrics = BiometricState.Baseline(),
                DecisionLog = new List<AgentDecision>(),
                Violations = new List<ConstraintViolation>()
            };
        }

        /// <summary>
        /// Records an agent's decision in the shared log.
        /// </summary>
        public void RecordDecision(AgentDecision decision)
        {
            decision.timestampUtc = DateTime.UtcNow.ToString("o");
            DecisionLog.Add(decision);
        }

        /// <summary>
        /// Records a constraint violation.
        /// </summary>
        public void RecordViolation(ConstraintViolation violation)
        {
            violation.timestampUtc = DateTime.UtcNow.ToString("o");
            Violations.Add(violation);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Specification & State Types
    // ══════════════════════════════════════════════════════════════════════════

    [Serializable]
    public sealed class WorldSpecification
    {
        public int segmentCount;
        public float worldScaleMeters;
        public float coordinateRotationY;
        public string[] activeZones;
        public int navMeshSurfaceCount;

        public static WorldSpecification Default() => new WorldSpecification
        {
            segmentCount = 23,
            worldScaleMeters = 2560f,
            coordinateRotationY = -72f,
            activeZones = new string[0],
            navMeshSurfaceCount = 0
        };
    }

    [Serializable]
    public sealed class CharacterSpecification
    {
        public string archetypeName;
        public float armSpanMultiplier;
        public float tremorFrequencyHz;
        public float asymmetryFactor;
        public bool ikRigConfigured;

        public static CharacterSpecification Default() => new CharacterSpecification
        {
            archetypeName = "Unassigned",
            armSpanMultiplier = 1.0f,
            tremorFrequencyHz = 0f,
            asymmetryFactor = 0f,
            ikRigConfigured = false
        };
    }

    [Serializable]
    public sealed class NarrativeState
    {
        public string currentQuestId;
        public string currentQuestStage;
        public string[] completedQuests;
        public string[] criticalInteractableIds;
        public bool isInCutscene;
        public bool isInDialogue;
        public float loreExposureLevel;

        public static NarrativeState Default() => new NarrativeState
        {
            currentQuestId = "none",
            currentQuestStage = "none",
            completedQuests = new string[0],
            criticalInteractableIds = new string[0],
            isInCutscene = false,
            isInDialogue = false,
            loreExposureLevel = 0f
        };
    }

    [Serializable]
    public sealed class PerformanceBudget
    {
        public float frameBudgetMs;
        public int maxDrawCalls;
        public int maxTrianglesPerEye;
        public float maxTextureMemoryMB;
        public float lightingBudgetMs;
        public float fogBudgetMs;
        public float physicsBudgetMs;
        public float audioBudgetMs;

        /// <summary>Remaining budget after consumed allocations.</summary>
        public float consumedBudgetMs;

        public float RemainingMs => frameBudgetMs - consumedBudgetMs;

        public static PerformanceBudget Default90Hz() => new PerformanceBudget
        {
            frameBudgetMs = 11.11f,
            maxDrawCalls = 150,
            maxTrianglesPerEye = 1500000,
            maxTextureMemoryMB = 2048f,
            lightingBudgetMs = 3.33f,
            fogBudgetMs = 1.67f,
            physicsBudgetMs = 2.0f,
            audioBudgetMs = 1.0f,
            consumedBudgetMs = 0f
        };

        public static PerformanceBudget Default120Hz() => new PerformanceBudget
        {
            frameBudgetMs = 8.33f,
            maxDrawCalls = 100,
            maxTrianglesPerEye = 1000000,
            maxTextureMemoryMB = 1536f,
            lightingBudgetMs = 2.5f,
            fogBudgetMs = 1.25f,
            physicsBudgetMs = 1.5f,
            audioBudgetMs = 0.75f,
            consumedBudgetMs = 0f
        };
    }

    [Serializable]
    public sealed class ComfortPolicy
    {
        public float maxLocomotionSpeedMs;
        public float maxRotationSpeedDegPerSec;
        public float maxHapticIntensity;
        public float maxHapticContinuousDurationSec;
        public float maxAudioDecibelSPL;
        public float maxFlashFrequencyHz;
        public bool requireTeleportOption;
        public bool enableVignetteDuringMotion;

        public static ComfortPolicy Default() => new ComfortPolicy
        {
            maxLocomotionSpeedMs = 5f,
            maxRotationSpeedDegPerSec = 90f,
            maxHapticIntensity = 0.85f,
            maxHapticContinuousDurationSec = 30f,
            maxAudioDecibelSPL = 85f,
            maxFlashFrequencyHz = 3f,
            requireTeleportOption = true,
            enableVignetteDuringMotion = true
        };
    }

    [Serializable]
    public sealed class AccessibilityPolicy
    {
        public bool reducedMotion;
        public bool audioDescriptions;
        public bool highContrastMode;
        public bool subtitles;
        public bool colorBlindAssist;
        public float minimumTextSize;

        public static AccessibilityPolicy Default() => new AccessibilityPolicy
        {
            reducedMotion = false,
            audioDescriptions = false,
            highContrastMode = false,
            subtitles = true,
            colorBlindAssist = false,
            minimumTextSize = 16f
        };
    }

    [Serializable]
    public sealed class SanityState
    {
        public float currentSanity;
        public float previousSanity;
        public int lastTriggeredThresholdIndex;
        public float timeSinceLastThresholdCrossing;
        public int hallucinationsTriggedThisSession;

        public static SanityState FullySane() => new SanityState
        {
            currentSanity = 1.0f,
            previousSanity = 1.0f,
            lastTriggeredThresholdIndex = -1,
            timeSinceLastThresholdCrossing = float.MaxValue,
            hallucinationsTriggedThisSession = 0
        };
    }

    [Serializable]
    public sealed class BiometricState
    {
        public float heartRateBPM;
        public float heartRateSmoothed;
        public float stressLevel;
        public float pupilDilation;
        public float saccadeFrequency;
        public float skinConductance;
        public bool isCalibrated;

        public static BiometricState Baseline() => new BiometricState
        {
            heartRateBPM = 72f,
            heartRateSmoothed = 72f,
            stressLevel = 0f,
            pupilDilation = 0.5f,
            saccadeFrequency = 2f,
            skinConductance = 5f,
            isCalibrated = false
        };
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Decision & Violation Records
    // ══════════════════════════════════════════════════════════════════════════

    [Serializable]
    public sealed class AgentDecision
    {
        public string timestampUtc;
        public string agentName;
        public string pipeline;
        public string decisionType;
        public string description;
        public string rationale;
    }

    [Serializable]
    public sealed class ConstraintViolation
    {
        public string timestampUtc;
        public string sourceAgentName;
        public string constraintName;
        public ConstraintSeverity severity;
        public string description;
        public string suggestedFix;
    }

    public enum ConstraintSeverity
    {
        /// <summary>Informational — does not block execution.</summary>
        Info,
        /// <summary>Non-blocking but should be addressed.</summary>
        Warning,
        /// <summary>Blocks pipeline completion. Must be resolved.</summary>
        Blocking,
        /// <summary>Critical safety violation. Immediately halts cascade.</summary>
        Critical
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Context Patch — Agents propose changes, orchestrator applies them
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Agents return a ContextPatch instead of freely mutating ExperienceContext.
    /// The orchestrator validates and applies approved patches.
    /// </summary>
    [Serializable]
    public sealed class ContextPatch
    {
        public string proposingAgent;
        public string pipeline;
        public string description;

        /// <summary>JSON-serialized partial state changes.</summary>
        public string serializedChanges;

        /// <summary>Decisions made during this patch generation.</summary>
        public List<AgentDecision> decisions = new List<AgentDecision>();

        /// <summary>Violations detected during this patch generation.</summary>
        public List<ConstraintViolation> violations = new List<ConstraintViolation>();

        /// <summary>Whether this patch was approved by the orchestrator.</summary>
        public bool isApproved;

        /// <summary>Rejection reason if not approved.</summary>
        public string rejectionReason;
    }
}

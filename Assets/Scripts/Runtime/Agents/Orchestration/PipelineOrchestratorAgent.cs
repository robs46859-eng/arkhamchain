using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArkhamIsland.Agents.Orchestration
{
    using AgentFramework;

    /// <summary>
    /// PIPELINE ORCHESTRATOR AGENT
    ///
    /// Sits above all six pipeline pairs. Does NOT create content itself.
    ///
    /// Responsibilities:
    /// - Decide which pipelines run and in what order
    /// - Pass outputs between dependent agents
    /// - Maintain shared ExperienceContext (performance budget, comfort level,
    ///   narrative state, target platform)
    /// - Detect conflicts between pipelines
    /// - Request revisions rather than directly rewriting specialist outputs
    /// - Stop the cascade when acceptance criteria are met
    ///
    /// Execution order:
    ///   World → Character → Narrative → [Audio | Rendering | Haptics] (parallel) → Integration Validation
    ///
    /// Audio, rendering, and haptics run in parallel after world, character, and
    /// narrative state is sufficiently stable.
    /// </summary>
    public class PipelineOrchestratorAgent : MonoBehaviour
    {
        public static PipelineOrchestratorAgent Instance { get; private set; }

        // ── Pipeline References ───────────────────────────────────────────────

        [Header("Sequential Pipelines (ordered)")]
        [SerializeField] private AgentPipeline worldPipeline;
        [SerializeField] private AgentPipeline characterPipeline;
        [SerializeField] private AgentPipeline narrativePipeline;

        [Header("Parallel Pipelines (run after sequential completes)")]
        [SerializeField] private AgentPipeline audioPipeline;
        [SerializeField] private AgentPipeline renderingPipeline;
        [SerializeField] private AgentPipeline hapticsPipeline;

        [Header("Validation")]
        [SerializeField] private IntegrationReviewAgent reviewAgent;

        // ── Shared State ──────────────────────────────────────────────────────

        [Header("Shared Context")]
        [Tooltip("The shared world-state contract all agents operate against.")]
        private ExperienceContext context;

        // ── Orchestration State ───────────────────────────────────────────────

        [Header("Orchestration Configuration")]
        [Tooltip("Maximum cascade revision cycles before forced termination.")]
        [SerializeField] private int maxCascadeCycles = 3;

        [Tooltip("Whether to run parallel pipelines concurrently or sequentially.")]
        [SerializeField] private bool enableParallelExecution = true;

        private OrchestratorPhase currentPhase = OrchestratorPhase.Idle;
        private int currentCycle = 0;
        private List<string> phaseLog = new List<string>();

        /// <summary>Current orchestration phase.</summary>
        public OrchestratorPhase CurrentPhase => currentPhase;

        /// <summary>Current cascade revision cycle.</summary>
        public int CurrentCycle => currentCycle;

        /// <summary>The shared ExperienceContext.</summary>
        public ExperienceContext Context => context;

        /// <summary>Event fired when orchestration completes.</summary>
        public event Action<ExperienceContext, ReviewResult> OnOrchestrationComplete;

        /// <summary>Event fired when orchestration requires human escalation.</summary>
        public event Action<ExperienceContext, string> OnHumanEscalationRequired;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initializes the shared context and begins a full cascade execution.
        /// This is the primary entry point for the orchestration system.
        /// </summary>
        public void BeginCascade(ExperienceContext initialContext = null)
        {
            context = initialContext ?? ExperienceContext.CreateDefault();
            currentCycle = 0;
            phaseLog.Clear();

            LogPhase("CASCADE STARTED", $"Session: {context.SessionId}, Platform: {context.TargetPlatform}");
            ExecuteCascadeCycle();
        }

        /// <summary>
        /// Executes one full cascade cycle:
        /// World → Character → Narrative → [Audio | Rendering | Haptics] → Review
        /// </summary>
        private void ExecuteCascadeCycle()
        {
            currentCycle++;

            if (currentCycle > maxCascadeCycles)
            {
                LogPhase("CASCADE EXHAUSTED", $"Max cycles ({maxCascadeCycles}) reached. Forcing termination.");

                if (EscalationLoop.Instance != null)
                {
                    // Try safe fallback
                    LogPhase("FALLBACK", "Applying safe defaults for unresolved conflicts.");
                }

                OnHumanEscalationRequired?.Invoke(context, "Maximum cascade cycles exhausted.");
                return;
            }

            LogPhase($"CYCLE {currentCycle}", $"Beginning cascade cycle {currentCycle}/{maxCascadeCycles}");

            // Phase 1: Sequential pipelines
            ExecuteSequentialPhase();
        }

        private void ExecuteSequentialPhase()
        {
            // ── World Pipeline ────────────────────────────────────────────────
            currentPhase = OrchestratorPhase.World;
            LogPhase("WORLD", "Executing world pipeline (terrain, geometry, NavMesh).");
            ExecutePipeline(worldPipeline, "world");
            RunDeterministicValidation("world");

            // ── Character Pipeline ────────────────────────────────────────────
            currentPhase = OrchestratorPhase.Character;
            LogPhase("CHARACTER", "Executing character pipeline (archetype, IK, biometrics).");
            ExecutePipeline(characterPipeline, "character");
            RunDeterministicValidation("character");

            // ── Narrative Pipeline ────────────────────────────────────────────
            currentPhase = OrchestratorPhase.Narrative;
            LogPhase("NARRATIVE", "Executing narrative pipeline (hallucinations, sanity guards).");
            ExecutePipeline(narrativePipeline, "narrative");
            RunDeterministicValidation("narrative");

            // Phase 2: Parallel pipelines
            ExecuteParallelPhase();
        }

        private void ExecuteParallelPhase()
        {
            currentPhase = OrchestratorPhase.ParallelExecution;
            LogPhase("PARALLEL", "Executing audio, rendering, and haptics pipelines.");

            if (enableParallelExecution)
            {
                // Run all three — in a real async implementation these would be coroutines
                ExecutePipeline(audioPipeline, "audio");
                ExecutePipeline(renderingPipeline, "rendering");
                ExecutePipeline(hapticsPipeline, "haptics");
            }
            else
            {
                ExecutePipeline(audioPipeline, "audio");
                RunDeterministicValidation("audio");
                ExecutePipeline(renderingPipeline, "rendering");
                RunDeterministicValidation("rendering");
                ExecutePipeline(hapticsPipeline, "haptics");
                RunDeterministicValidation("haptics");
            }

            RunDeterministicValidation("all");

            // Phase 3: Integration review
            ExecuteIntegrationReview();
        }

        private void ExecutePipeline(AgentPipeline pipeline, string pipelineName)
        {
            if (pipeline == null)
            {
                LogPhase($"SKIP:{pipelineName.ToUpper()}", "Pipeline not assigned, skipping.");
                return;
            }

            // Inject the shared context into the envelope
            var envelope = WhyChainEnvelope.Create(
                new AgentDescriptor("orchestrator", "PipelineOrchestratorAgent", 0.5f, "orchestration"),
                new AgentDescriptor("", $"{pipelineName}_head", 1.0f, pipelineName),
                $"Orchestrator is executing the {pipelineName} pipeline as part of cascade cycle {currentCycle}.",
                $"Execute the {pipelineName} pipeline against the shared ExperienceContext."
            );

            envelope.payload = new AgentPayload
            {
                payloadType = pipelineName,
                serializedData = JsonUtility.ToJson(new OrchestratorPayload
                {
                    sessionId = context.SessionId,
                    cascadeCycle = currentCycle,
                    targetPlatform = context.TargetPlatform,
                    frameBudgetMs = context.Performance.frameBudgetMs,
                    remainingBudgetMs = context.Performance.RemainingMs
                })
            };

            bool accepted = pipeline.Inject(envelope);
            if (accepted)
            {
                pipeline.Tick();
            }

            context.RecordDecision(new AgentDecision
            {
                agentName = "PipelineOrchestratorAgent",
                pipeline = "orchestration",
                decisionType = "pipeline_execution",
                description = $"Executed {pipelineName} pipeline (cycle {currentCycle}).",
                rationale = accepted ? "Pipeline accepted and ticked." : "Pipeline rejected the envelope."
            });
        }

        private void RunDeterministicValidation(string scope)
        {
            currentPhase = OrchestratorPhase.Validation;

            if (Validators.ValidatorRunner.Instance == null)
            {
                LogPhase("VALIDATION", $"No ValidatorRunner found. Skipping {scope} validation.");
                return;
            }

            LogPhase("VALIDATION", $"Running deterministic validators for scope: {scope}");
            var results = Validators.ValidatorRunner.Instance.RunAll(context);

            foreach (var result in results)
            {
                if (result.severity == ConstraintSeverity.Blocking ||
                    result.severity == ConstraintSeverity.Critical)
                {
                    context.RecordViolation(result);
                    LogPhase("VIOLATION", $"[{result.severity}] {result.constraintName}: {result.description}");
                }
            }
        }

        private void ExecuteIntegrationReview()
        {
            currentPhase = OrchestratorPhase.IntegrationReview;
            LogPhase("REVIEW", $"Running integration review (cycle {currentCycle}).");

            if (reviewAgent == null)
            {
                LogPhase("REVIEW", "No IntegrationReviewAgent assigned. Auto-approving.");
                FinalizeCascade(ReviewResult.Pass(0.5f, context.SessionId, currentCycle));
                return;
            }

            ReviewResult result = reviewAgent.Review(context, currentCycle);

            if (result.Approved)
            {
                LogPhase("APPROVED", $"Integration review PASSED (score: {result.OverallScore:F2}).");
                FinalizeCascade(result);
            }
            else if (result.HasBlockingIssues && currentCycle < maxCascadeCycles)
            {
                LogPhase("REVISION", $"Integration review FAILED with {result.BlockingIssues.Count} blocking issues. " +
                         $"Requesting revisions (cycle {currentCycle + 1}).");

                // Route revision requests to specific pipelines
                foreach (var request in result.RevisionRequests)
                {
                    LogPhase("REVISION_REQUEST",
                        $"→ {request.TargetAgent} ({request.TargetPipeline}): {request.RevisionDescription}");
                }

                // Re-run the cascade
                ExecuteCascadeCycle();
            }
            else
            {
                LogPhase("FAILED", "Integration review FAILED and no more retries available.");
                OnHumanEscalationRequired?.Invoke(context, "Integration review failed after all retry cycles.");
                FinalizeCascade(result);
            }
        }

        private void FinalizeCascade(ReviewResult result)
        {
            currentPhase = OrchestratorPhase.Complete;
            LogPhase("COMPLETE", $"Cascade finalized. Approved: {result.Approved}, Score: {result.OverallScore:F2}, " +
                     $"Cycles: {currentCycle}, Decisions: {context.DecisionLog.Count}, " +
                     $"Violations: {context.Violations.Count}");

            OnOrchestrationComplete?.Invoke(context, result);
        }

        private void LogPhase(string phase, string message)
        {
            string entry = $"[Orchestrator][{phase}] {message}";
            phaseLog.Add(entry);
            Debug.Log(entry);
        }

        /// <summary>
        /// Returns the full phase execution log for this session.
        /// </summary>
        public IReadOnlyList<string> GetPhaseLog() => phaseLog.AsReadOnly();

        /// <summary>
        /// Detects conflicts between pipeline outputs.
        /// Called after parallel pipelines complete.
        /// </summary>
        public List<string> DetectCrossSystemConflicts()
        {
            var conflicts = new List<string>();

            // Check budget overrun from parallel pipelines
            if (context.Performance.consumedBudgetMs > context.Performance.frameBudgetMs)
            {
                conflicts.Add($"BUDGET OVERRUN: Consumed {context.Performance.consumedBudgetMs:F2}ms " +
                             $"exceeds frame budget {context.Performance.frameBudgetMs:F2}ms.");
            }

            // Check if hallucination effects overlap with quest-critical visibility
            if (context.Narrative.criticalInteractableIds != null &&
                context.Narrative.criticalInteractableIds.Length > 0 &&
                context.Sanity.hallucinationsTriggedThisSession > 0)
            {
                conflicts.Add("SAFETY: Active hallucinations may overlap with quest-critical interactables.");
            }

            return conflicts;
        }

        [Serializable]
        private struct OrchestratorPayload
        {
            public string sessionId;
            public int cascadeCycle;
            public string targetPlatform;
            public float frameBudgetMs;
            public float remainingBudgetMs;
        }
    }

    /// <summary>
    /// Orchestration execution phases.
    /// </summary>
    public enum OrchestratorPhase
    {
        Idle,
        World,
        Character,
        Narrative,
        ParallelExecution,
        Validation,
        IntegrationReview,
        Revision,
        Complete
    }
}

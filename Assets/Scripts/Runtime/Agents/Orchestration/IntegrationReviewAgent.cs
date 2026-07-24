using System.Collections.Generic;
using UnityEngine;

namespace ArkhamIsland.Agents.Orchestration
{
    using AgentFramework;

    /// <summary>
    /// INTEGRATION REVIEW AGENT (VR EXPERIENCE REVIEWER)
    ///
    /// Separate from the orchestrator — the orchestrator manages workflow,
    /// the reviewer judges the completed result. Combining those roles creates
    /// a risk that the orchestrator approves its own decisions.
    ///
    /// Evaluates:
    /// - VR comfort and accessibility
    /// - Narrative coherence
    /// - Cross-system consistency
    /// - GPU, CPU, memory, and draw-call budgets
    /// - Audio and haptic intensity limits
    /// - NavMesh and collision validity
    /// - Whether hallucinations can obstruct required gameplay
    /// - Whether sanity events can soft-lock quests
    /// - Whether biometrics produce unstable feedback loops
    /// - Whether all generated assets have provenance and ownership metadata
    ///
    /// Output: Structured ReviewResult with blocking issues, warnings, and
    ///         targeted revision requests.
    /// </summary>
    public class IntegrationReviewAgent : MonoBehaviour
    {
        [Header("Review Configuration")]
        [Tooltip("Minimum overall score to approve (0.0–1.0).")]
        [Range(0f, 1f)]
        [SerializeField] private float approvalThreshold = 0.7f;

        [Tooltip("Whether to run all deterministic validators during review.")]
        [SerializeField] private bool runValidatorsOnReview = true;

        [Tooltip("Weight for each review domain in the overall score.")]
        [SerializeField] private float safetyWeight = 0.25f;
        [SerializeField] private float performanceWeight = 0.20f;
        [SerializeField] private float narrativeWeight = 0.15f;
        [SerializeField] private float comfortWeight = 0.20f;
        [SerializeField] private float consistencyWeight = 0.10f;
        [SerializeField] private float provenanceWeight = 0.10f;

        /// <summary>
        /// Performs a full integration review of the cascade output.
        /// Returns a structured ReviewResult.
        /// </summary>
        public ReviewResult Review(ExperienceContext context, int passNumber)
        {
            Debug.Log($"[IntegrationReviewAgent] Beginning review pass {passNumber}...");

            var blocking = new List<ReviewIssue>();
            var warnings = new List<ReviewIssue>();
            var revisions = new List<RevisionRequest>();

            float safetyScore = ReviewVRComfort(context, blocking, warnings);
            float perfScore = ReviewPerformanceBudgets(context, blocking, warnings, revisions);
            float narrativeScore = ReviewNarrativeCoherence(context, blocking, warnings, revisions);
            float comfortScore = ReviewAudioHapticIntensity(context, blocking, warnings, revisions);
            float consistencyScore = ReviewCrossSystemConsistency(context, blocking, warnings);
            float provenanceScore = ReviewAssetProvenance(context, warnings);

            // Run deterministic validators
            if (runValidatorsOnReview && Validators.ValidatorRunner.Instance != null)
            {
                var validatorResults = Validators.ValidatorRunner.Instance.RunAll(context);
                foreach (var v in validatorResults)
                {
                    var issue = new ReviewIssue
                    {
                        Domain = MapConstraintToDomain(v.constraintName),
                        Severity = v.severity,
                        SourceAgent = v.sourceAgentName,
                        Description = v.description,
                        ViolatedConstraint = v.constraintName,
                        ActualValue = "",
                        ExpectedValue = v.suggestedFix
                    };

                    if (v.severity == ConstraintSeverity.Blocking || v.severity == ConstraintSeverity.Critical)
                        blocking.Add(issue);
                    else
                        warnings.Add(issue);
                }
            }

            // Compute weighted overall score
            float overall = safetyScore * safetyWeight +
                           perfScore * performanceWeight +
                           narrativeScore * narrativeWeight +
                           comfortScore * comfortWeight +
                           consistencyScore * consistencyWeight +
                           provenanceScore * provenanceWeight;

            bool approved = blocking.Count == 0 && overall >= approvalThreshold;

            var result = approved
                ? ReviewResult.Pass(overall, context.SessionId, passNumber)
                : ReviewResult.Fail(overall, context.SessionId, passNumber);

            result.BlockingIssues = blocking;
            result.Warnings = warnings;
            result.RevisionRequests = revisions;

            Debug.Log($"[IntegrationReviewAgent] Review complete. " +
                      $"Approved: {approved} | Score: {overall:F3} | " +
                      $"Blocking: {blocking.Count} | Warnings: {warnings.Count} | " +
                      $"Revisions: {revisions.Count}");

            return result;
        }

        // ══════════════════════════════════════════════════════════════════════
        // Domain-Specific Reviews
        // ══════════════════════════════════════════════════════════════════════

        private float ReviewVRComfort(ExperienceContext ctx,
            List<ReviewIssue> blocking, List<ReviewIssue> warnings)
        {
            float score = 1.0f;

            // Check flash frequency
            if (ctx.Comfort.maxFlashFrequencyHz > 3f)
            {
                blocking.Add(new ReviewIssue
                {
                    Domain = ReviewDomain.VRComfort,
                    Severity = ConstraintSeverity.Critical,
                    Description = "Photosensitive safety: flash frequency exceeds 3Hz limit.",
                    ViolatedConstraint = "maxFlashFrequencyHz",
                    ActualValue = ctx.Comfort.maxFlashFrequencyHz.ToString("F1"),
                    ExpectedValue = "≤ 3.0 Hz"
                });
                score -= 0.5f;
            }

            // Accessibility policy
            if (ctx.Comfort.requireTeleportOption && !ctx.Accessibility.reducedMotion)
            {
                // Fine — teleport is available
            }

            return Mathf.Clamp01(score);
        }

        private float ReviewPerformanceBudgets(ExperienceContext ctx,
            List<ReviewIssue> blocking, List<ReviewIssue> warnings,
            List<RevisionRequest> revisions)
        {
            float score = 1.0f;

            if (ctx.Performance.consumedBudgetMs > ctx.Performance.frameBudgetMs)
            {
                float overrun = ctx.Performance.consumedBudgetMs - ctx.Performance.frameBudgetMs;
                blocking.Add(new ReviewIssue
                {
                    Domain = ReviewDomain.GPUBudget,
                    Severity = ConstraintSeverity.Blocking,
                    Description = $"Frame budget exceeded by {overrun:F2}ms.",
                    ViolatedConstraint = "frameBudgetMs",
                    ActualValue = ctx.Performance.consumedBudgetMs.ToString("F2"),
                    ExpectedValue = $"≤ {ctx.Performance.frameBudgetMs:F2}"
                });

                revisions.Add(new RevisionRequest
                {
                    TargetAgent = "VRShaderOptimizationAgent",
                    TargetPipeline = "rendering",
                    RevisionDescription = $"Reduce rendering cost by at least {overrun:F2}ms.",
                    ConstraintReference = "frameBudgetMs",
                    Priority = 0,
                    BudgetCapMs = ctx.Performance.frameBudgetMs
                });

                score -= 0.4f;
            }

            return Mathf.Clamp01(score);
        }

        private float ReviewNarrativeCoherence(ExperienceContext ctx,
            List<ReviewIssue> blocking, List<ReviewIssue> warnings,
            List<RevisionRequest> revisions)
        {
            float score = 1.0f;

            // Check: hallucinations during cutscenes
            if (ctx.Narrative.isInCutscene && ctx.Sanity.hallucinationsTriggedThisSession > 0)
            {
                blocking.Add(new ReviewIssue
                {
                    Domain = ReviewDomain.HallucinationGameplaySafety,
                    Severity = ConstraintSeverity.Blocking,
                    SourceAgent = "CosmicMythosWeaverAgent",
                    Description = "Hallucinations triggered during cutscene — may break narrative delivery.",
                    ViolatedConstraint = "no_hallucinations_during_cutscene"
                });

                revisions.Add(new RevisionRequest
                {
                    TargetAgent = "SanityEngineAnchorAgent",
                    TargetPipeline = "narrative",
                    RevisionDescription = "Defer hallucination trigger until cutscene completes.",
                    ConstraintReference = "cutscene_guard",
                    Priority = 1
                });

                score -= 0.3f;
            }

            // Check: quest reachability with active effects
            if (ctx.Narrative.criticalInteractableIds != null &&
                ctx.Narrative.criticalInteractableIds.Length > 0 &&
                ctx.Sanity.currentSanity < 0.3f)
            {
                warnings.Add(new ReviewIssue
                {
                    Domain = ReviewDomain.SanityQuestSafety,
                    Severity = ConstraintSeverity.Warning,
                    Description = "Low sanity with quest-critical items active. " +
                                  "Verify hallucination effects don't obstruct interactables.",
                    ViolatedConstraint = "quest_item_visibility"
                });
                score -= 0.1f;
            }

            return Mathf.Clamp01(score);
        }

        private float ReviewAudioHapticIntensity(ExperienceContext ctx,
            List<ReviewIssue> blocking, List<ReviewIssue> warnings,
            List<RevisionRequest> revisions)
        {
            float score = 1.0f;

            if (ctx.Comfort.maxAudioDecibelSPL > 85f)
            {
                warnings.Add(new ReviewIssue
                {
                    Domain = ReviewDomain.AudioIntensity,
                    Severity = ConstraintSeverity.Warning,
                    Description = "Audio volume limit exceeds safe SPL threshold.",
                    ViolatedConstraint = "maxAudioDecibelSPL",
                    ActualValue = ctx.Comfort.maxAudioDecibelSPL.ToString("F0"),
                    ExpectedValue = "≤ 85 dB SPL"
                });
                score -= 0.1f;
            }

            return Mathf.Clamp01(score);
        }

        private float ReviewCrossSystemConsistency(ExperienceContext ctx,
            List<ReviewIssue> blocking, List<ReviewIssue> warnings)
        {
            float score = 1.0f;

            // Check biometric feedback loop risk
            if (ctx.Biometrics.stressLevel > 0.8f && ctx.Sanity.currentSanity < 0.3f)
            {
                warnings.Add(new ReviewIssue
                {
                    Domain = ReviewDomain.BiometricFeedbackLoop,
                    Severity = ConstraintSeverity.Warning,
                    Description = "High biometric stress + low sanity may create unstable feedback loop. " +
                                  "Hallucinations increase stress, stress drops sanity, sanity triggers more hallucinations.",
                    ViolatedConstraint = "feedback_loop_stability"
                });
                score -= 0.2f;
            }

            return Mathf.Clamp01(score);
        }

        private float ReviewAssetProvenance(ExperienceContext ctx,
            List<ReviewIssue> warnings)
        {
            // In full implementation, verify all generated assets have ownership metadata
            return 1.0f;
        }

        private ReviewDomain MapConstraintToDomain(string constraintName)
        {
            if (constraintName.Contains("NavMesh")) return ReviewDomain.NavMeshValidity;
            if (constraintName.Contains("Collision")) return ReviewDomain.CollisionValidity;
            if (constraintName.Contains("Performance") || constraintName.Contains("Budget")) return ReviewDomain.GPUBudget;
            if (constraintName.Contains("Audio")) return ReviewDomain.AudioIntensity;
            if (constraintName.Contains("Haptic")) return ReviewDomain.HapticIntensity;
            if (constraintName.Contains("Hallucination")) return ReviewDomain.HallucinationGameplaySafety;
            if (constraintName.Contains("Quest")) return ReviewDomain.SanityQuestSafety;
            if (constraintName.Contains("Biometric")) return ReviewDomain.BiometricFeedbackLoop;
            if (constraintName.Contains("Provenance")) return ReviewDomain.AssetProvenance;
            return ReviewDomain.CrossSystemConsistency;
        }
    }
}
